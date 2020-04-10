using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChessComTournamentAggregator.Model
{
    public class Group
    {
        [JsonPropertyName("players")]
        public ICollection<Player> Players { get; set; }
    }
}
