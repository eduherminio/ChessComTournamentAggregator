using LichessTournamentAggregator.Model;
using System.Text.Json.Serialization;

namespace ChessComTournamentAggregator.Model
{
    public class Player
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("points")]
        public double Points { get; set; }

        [JsonPropertyName("is_winner")]
        public bool IsWinner { get; set; }

        public TournamentResult ToTournamentResult()
        {
            return new TournamentResult()
            {
                Username = Username,
                Score = Points
            };
        }
    }
}
