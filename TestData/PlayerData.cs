using Newtonsoft.Json;

namespace FootballSeleniumTest.Models
{
    public class PlayerData
    {
        [JsonProperty("playerId")]
        public string PlayerId { get; set; } = "";

        [JsonProperty("playerName")]
        public string PlayerName { get; set; } = "";

        // Ngày sinh: Selenium sẽ gửi chuỗi này vào ô input date (yyyy-mm-dd)
        [JsonProperty("dob")]
        public string Dob { get; set; } = "";

        [JsonProperty("gender")]
        public string Gender { get; set; } = "";

        [JsonProperty("jerseyNumber")]
        public object JerseyNumber { get; set; } = 0;

        [JsonProperty("position")]
        public string Position { get; set; } = "";

        [JsonProperty("teamId")]
        public string TeamId { get; set; } = "";

        [JsonProperty("status")]
        public string Status { get; set; } = "";

        [JsonProperty("action")]
        public string Action { get; set; } = "";

        [JsonProperty("expected")]
        public bool Expected { get; set; }
    }
}