using ChessComTournamentAggregator.Model;
using LichessTournamentAggregator;
using LichessTournamentAggregator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChessComTournamentAggregator
{
    public class TournamentAggregator : ITournamentAggregator
    {
        public async IAsyncEnumerable<AggregatedResult> AggregateResults(IEnumerable<string> tournamentIdsOrUrls)
        {
            var aggregatedResults = await GetUrls(tournamentIdsOrUrls)
                .Select(GetTournamentResults)
                .Aggregate((result, next) => result.Concat(next))
                .ToListAsync()
            .ConfigureAwait(false);

            foreach (var grouping in GroupResultsByPlayer(aggregatedResults))
            {
                yield return new AggregatedResult(grouping);
            }
        }

        public IEnumerable<AggregatedResult> AggregateResults(IEnumerable<TournamentResult> tournamentResults)
        {
            foreach (var grouping in GroupResultsByPlayer(tournamentResults))
            {
                yield return new AggregatedResult(grouping);
            }
        }

        public async Task<FileStream> AggregateResultsAndExportToCsv(IEnumerable<string> tournamentIdsOrUrls, FileStream fileStream, string separator = ";")
        {
            var aggregatedResults = AggregateResults(tournamentIdsOrUrls)
                .OrderByDescending(r => r.TotalScores)
                .ThenByDescending(r => r.AveragePerformance);

            return await PopulateCsvStreamAsync(fileStream, separator, aggregatedResults).ConfigureAwait(false);
        }

        internal IEnumerable<Uri> GetUrls(IEnumerable<string> tournamentIdsOrUrls)
        {
            foreach (var item in tournamentIdsOrUrls)
            {
                var tournamentId = item.AsSpan().Trim(new char[] { ' ', '/', '#' });

                tournamentId = tournamentId.Slice(tournamentId.LastIndexOf('/') + 1);

                yield return new Uri($"https://api.chess.com/pub/tournament/{tournamentId.ToString()}");
            }
        }

        private async IAsyncEnumerable<TournamentResult> GetTournamentResults(Uri tournamentUrl)
        {
            var client = new HttpClient();

            var response = await client.GetAsync(tournamentUrl).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ArgumentException("The following tournament url doesn't seem to exist",
                    tournamentUrl.OriginalString.Replace("api.chess.com/pub/tournament/", "chess.com/tournament/live/"));
            }

            var rawContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            foreach (var roundUrl in JsonSerializer.Deserialize<Tournament>(rawContent).Rounds)
            {
                response = await client.GetAsync(roundUrl).ConfigureAwait(false);
                rawContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var round = JsonSerializer.Deserialize<Round>(rawContent);
                bool isSwiss = round.Groups != null;

                if (isSwiss)
                {
                    foreach (var groupUrl in round.Groups)
                    {
                        response = await client.GetAsync(groupUrl).ConfigureAwait(false);
                        rawContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var group = JsonSerializer.Deserialize<Group>(rawContent);

                        foreach (var player in group.Players)
                        {
                            yield return player.ToTournamentResult();
                        }
                    }
                }
                else
                {
                    foreach (var player in round.Players)
                    {
                        yield return player.ToTournamentResult();
                    }
                }
            }
        }

        private static IEnumerable<IGrouping<string, TournamentResult>> GroupResultsByPlayer(IEnumerable<TournamentResult> results)
        {
            return results
                .Where(r => !string.IsNullOrWhiteSpace(r?.Username))
                .GroupBy(r => r.Username);
        }

        private static async Task<FileStream> PopulateCsvStreamAsync(FileStream fileStream, string separator, IAsyncEnumerable<AggregatedResult> aggregatedResults)
        {
            var headers = new List<string> { "#", "Username", "Total Score", "Scores" };
            using var sw = new StreamWriter(fileStream);
            sw.WriteLine(string.Join(separator, headers));

            var internalSeparator = separator == ";" ? ", " : "; ";
            string aggregate<T>(IEnumerable<T> items) => $"[{string.Join(internalSeparator, items)}]";

            await foreach (var aggregatedResult in aggregatedResults.Select((value, i) => new { i, value }))
            {
                var result = aggregatedResult.value;
                var columns = new string[] { (aggregatedResult.i + 1).ToString(), result.Username, result.TotalScores.ToString(), aggregate(result.Scores) };
                sw.WriteLine(string.Join(separator, columns));
            }

            return fileStream;
        }
    }
}
