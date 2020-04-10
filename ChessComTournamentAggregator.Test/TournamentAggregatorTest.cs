using System.Linq;
using Xunit;

namespace ChessComTournamentAggregator.Test
{
    public class TournamentAggregatorTest
    {
        private readonly TournamentAggregator _aggregator;

        public TournamentAggregatorTest()
        {
            _aggregator = new TournamentAggregator();
        }

        [Fact]
        public void GetUrls()
        {
            const string tournamentId = "chess-england-blitz-1180073";

            var validInputs = new[]
            {
                $"https://chess.com/tournament/live/{tournamentId}#",
                $"https://chess.com/tournament/live/{tournamentId}/",
                $"https://chess.com/tournament/live/{tournamentId}#/",
                $"https://chess.com/tournament/live/{tournamentId}/#",
                $"www.chess.com/tournament/live/{tournamentId}#",
                $"www.chess.com/tournament/live/{tournamentId}/",
                $"www.chess.com/tournament/live/{tournamentId}#/",
                $"www.chess.com/tournament/live/{tournamentId}/#",
                $"chess.com/tournament/live/{tournamentId}#",
                $"chess.com/tournament/live/{tournamentId}/",
                $"chess.com/tournament/live/{tournamentId}#/",
                $"chess.com/tournament/live/{tournamentId}/#"
            };

            var results = _aggregator.GetUrls(validInputs).ToList();

            Assert.Equal(
                $"https://api.chess.com/pub/tournament/{tournamentId}",
                results.ToHashSet().Single().OriginalString);
        }
    }
}
