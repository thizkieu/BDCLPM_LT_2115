using System.Text.Json.Serialization;

namespace FootballSeleniumTest.Models
{
    public class TeamData
    {
        // Mã đội bóng - Tương ứng id="MaDB"
        public string teamId { get; set; } = "";

        // Tên đội bóng - Tương ứng id="TenDoi"
        public string teamName { get; set; } = "";

        // Trường học - Tương ứng id="TruongHoc"
        public string school { get; set; } = "";

        // Huấn luyện viên - Tương ứng id="HuanLuyenVien"
        public string coach { get; set; } = "";

        // Số lượng cầu thủ (Kiểu số) - Tương ứng id="SoLuongCauThu"
        public int players { get; set; }

        // Tên file ảnh (vd: default.png) - Tương ứng name="HinhAnhFile"
        public string logo { get; set; } = "";

        // Hành động: add_team, edit_team, delete_team, sort_list, v.v.
        public string action { get; set; } = "";

        // Kết quả mong đợi (true: Thành công / false: Báo lỗi)
        public bool expected { get; set; }
    }
}