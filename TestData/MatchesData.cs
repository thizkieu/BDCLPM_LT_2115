using Newtonsoft.Json;

namespace FootballSeleniumTest.Models // Nên để trong Models cho đồng bộ với TeamData/PlayerData
{
    public class MatchesData
    {
        [JsonProperty("MaTD")]
        public string MaTD { get; set; } = "";

        [JsonProperty("TenTran")]
        public string TenTran { get; set; } = "";

        [JsonProperty("Doi1")]
        public string Doi1 { get; set; } = "";

        [JsonProperty("Doi2")]
        public string Doi2 { get; set; } = "";

        // Định dạng yyyy-MM-ddTHH:mm để khớp với ô datetime-local trong HTML
        [JsonProperty("ThoiGian")]
        public string ThoiGian { get; set; } = "";

        [JsonProperty("DiaDiem")]
        public string DiaDiem { get; set; } = "";

        [JsonProperty("MaVD")]
        public string MaVD { get; set; } = "";

        [JsonProperty("action")]
        public string Action { get; set; } = "";

        [JsonProperty("expected")]
        public bool Expected { get; set; }
    }
}