using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChessComTournamentAggregator.Model
{
    public class Tournament
    {
        [JsonPropertyName("rounds")]
        public ICollection<string> Rounds { get; set; }

        [JsonPropertyName("name")]
        public string Username { get; set; }

        [JsonPropertyName("creator")]
        public string Creator { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
