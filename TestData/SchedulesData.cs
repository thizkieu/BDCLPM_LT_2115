using Newtonsoft.Json;

namespace FootballSeleniumTest.Models
{
    public class SchedulesData
    {
        [JsonProperty("MaLich")]
        public string MaLich { get; set; } = "";

        [JsonProperty("MaTD")]
        public string MaTD { get; set; } = "";

        // Định dạng yyyy-MM-dd
        [JsonProperty("NgayThiDau")]
        public string NgayThiDau { get; set; } = "";

        // Định dạng HH:mm
        [JsonProperty("GioThiDau")]
        public string GioThiDau { get; set; } = "";

        [JsonProperty("DiaDiem")]
        public string DiaDiem { get; set; } = "";

        [JsonProperty("TrangThai")]
        public string TrangThai { get; set; } = "";

        [JsonProperty("action")]
        public string Action { get; set; } = "";

        [JsonProperty("expected")]
        public bool Expected { get; set; }
    }
}