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
            var queue = new Queue<IAsyncEnumerable<TournamentResult>>();
            foreach (var url in GetUrls(tournamentIdsOrUrls))
            {
                queue.Enqueue(GetTournamentResults(url));
            }

            var results = new List<TournamentResult>();
            while (queue.Count > 0)
            {
                await foreach (var result in queue.Dequeue())
                {
                    results.Add(result);
                }
            }

            foreach (var grouping in GroupResultsByPlayer(results))
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
            var aggregatedResults = new List<AggregatedResult>();
            await foreach (var result in AggregateResults(tournamentIdsOrUrls))
            {
                aggregatedResults.Add(result);
            }

            aggregatedResults = aggregatedResults
                .OrderByDescending(r => r.TotalScores)
                .ThenByDescending(r => r.AveragePerformance)
                .ToList();

            return PopulateCsvStream(fileStream, separator, aggregatedResults);
        }

        internal IEnumerable<Uri> GetUrls(IEnumerable<string> tournamentIdsOrUrls)
        {
            foreach (var item in tournamentIdsOrUrls)
            {
                string tournamentId = item.Trim(new char[] { ' ', '/', '#' });

                var reverse = string.Join("", tournamentId.Reverse());
                tournamentId = string.Join("", reverse.Take(reverse.IndexOf("/")).Reverse());

                yield return new Uri($"https://api.chess.com/pub/tournament/{tournamentId}");
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

        private static FileStream PopulateCsvStream(FileStream fileStream, string separator, IEnumerable<AggregatedResult> aggregatedResults)
        {
            var headers = new List<string> { "#", "Username", "Total Score", "Scores" };
            using var sw = new StreamWriter(fileStream);
            sw.WriteLine(string.Join(separator, headers));

            var internalSeparator = separator == ";" ? ", " : "; ";
            string aggregate<T>(IEnumerable<T> items) => $"[{string.Join(internalSeparator, items)}]";
            for (int i = 0; i < aggregatedResults.Count(); ++i)
            {
                var result = aggregatedResults.ElementAt(i);
                var columns = new string[] { (i +1).ToString(), result.Username, result.TotalScores.ToString(), aggregate(result.Scores) };
                sw.WriteLine(string.Join(separator, columns));
            }

            return fileStream;
        }
    }
}
