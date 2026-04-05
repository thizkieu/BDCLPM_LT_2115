using Newtonsoft.Json;

namespace FootballSeleniumTest.Models
{
    public class RoundsData
    {
        [JsonProperty("MaVD")]
        public string MaVD { get; set; } = "";

        [JsonProperty("TenVong")]
        public string TenVong { get; set; } = "";

        // Định dạng: yyyy-MM-ddTHH:mm
        [JsonProperty("ThoiGian")]
        public string ThoiGian { get; set; } = "";

        [JsonProperty("MaMG")]
        public string MaMG { get; set; } = "";

        [JsonProperty("action")]
        public string Action { get; set; } = "";

        [JsonProperty("expected")]
        public bool Expected { get; set; }
    }
}