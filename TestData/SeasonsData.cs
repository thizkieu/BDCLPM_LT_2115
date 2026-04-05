using Newtonsoft.Json;

namespace FootballSeleniumTest.Models
{
    public class SeasonsData
    {
        [JsonProperty("MaMG")]
        public string MaMG { get; set; } = "";

        [JsonProperty("TenMuaGiai")]
        public string TenMuaGiai { get; set; } = "";

        // Định dạng yyyy-MM-ddTHH:mm để khớp với datetime-local
        [JsonProperty("ThoiGian")]
        public string ThoiGian { get; set; } = "";

        [JsonProperty("MoTa")]
        public string MoTa { get; set; } = "";

        [JsonProperty("action")]
        public string Action { get; set; } = "";

        [JsonProperty("expected")]
        public bool Expected { get; set; }
    }
}