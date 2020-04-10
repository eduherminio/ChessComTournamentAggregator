using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChessComTournamentAggregator.Model
{
    public class Round
    {
        [JsonPropertyName("groups")]
        public ICollection<string> Groups { get; set; }
    }
}
