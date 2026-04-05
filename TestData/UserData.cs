using System;

namespace FootballSeleniumTest.TestData
{
    public class UserData
    {
        // Tên đăng nhập hoặc Email
        public string username { get; set; } = "";

        // Mật khẩu
        public string password { get; set; } = "";

        // Hành động cụ thể (login, saved_login, remember_login,...)
        public string action { get; set; } = "";

        // Kết quả mong đợi: true (Thành công), false (Có lỗi)
        public bool expected { get; set; }
    }
}