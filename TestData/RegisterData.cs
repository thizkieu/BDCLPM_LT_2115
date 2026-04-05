using Newtonsoft.Json;

namespace FootballSeleniumTest.Models
{
    public class RegisterData
    {
        [JsonProperty("HoTen")]
        public string HoTen { get; set; } = "";

        [JsonProperty("Email")]
        public string Email { get; set; } = "";

        [JsonProperty("TenDangNhap")]
        public string TenDangNhap { get; set; } = "";

        [JsonProperty("MatKhau")]
        public string MatKhau { get; set; } = "";

        // Chứa đường dẫn tuyệt đối đến file ảnh trên máy tính Kiều
        [JsonProperty("AnhDaiDienFile")]
        public string AnhDaiDienFile { get; set; } = "";

        [JsonProperty("action")]
        public string Action { get; set; } = "";

        [JsonProperty("expected")]
        public bool Expected { get; set; }
    }
}