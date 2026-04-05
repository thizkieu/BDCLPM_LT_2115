using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FootballSeleniumTest.Models;
using System;
using System.Linq;
using System.IO;

namespace FootballSeleniumTest.Pages
{
    public class RegisterPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public RegisterPage(IWebDriver driver)
        {
            // Kiểm tra driver không được null
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        // --- LOCATORS ---
        private readonly By hoTenBy = By.Id("HoTen");
        private readonly By emailBy = By.Id("Email");
        private readonly By tenDangNhapBy = By.Id("TenDangNhap");
        private readonly By matKhauBy = By.Id("MatKhau");
        private readonly By avatarInputBy = By.Id("avatarInput");
        private readonly By btnSubmitBy = By.CssSelector("button[type='submit']");

        private readonly By errorSummaryBy = By.CssSelector(".field-validation-error, .text-danger");

        // --- HÀNH ĐỘNG (ACTIONS) ---

        public void FillRegisterForm(RegisterData data)
        {
            // 1. Điền Họ tên
            var txtHoTen = _wait.Until(ExpectedConditions.ElementIsVisible(hoTenBy));
            txtHoTen.Clear();
            txtHoTen.SendKeys(data.HoTen ?? ""); // Chống lỗi null

            // 2. Điền Email
            var txtEmail = _driver.FindElement(emailBy);
            txtEmail.Clear();
            txtEmail.SendKeys(data.Email ?? "");

            // 3. Điền Tên đăng nhập
            var txtUser = _driver.FindElement(tenDangNhapBy);
            txtUser.Clear();
            txtUser.SendKeys(data.TenDangNhap ?? "");

            // 4. Điền Mật khẩu
            var txtPass = _driver.FindElement(matKhauBy);
            txtPass.Clear();
            txtPass.SendKeys(data.MatKhau ?? "");

            // 5. Upload ảnh đại diện
            if (!string.IsNullOrEmpty(data.AnhDaiDienFile) && File.Exists(data.AnhDaiDienFile))
            {
                _driver.FindElement(avatarInputBy).SendKeys(data.AnhDaiDienFile);
            }

            // 6. Nhấn nút "TẠO TÀI KHOẢN" bằng JavaScript
            var btnSubmit = _driver.FindElement(btnSubmitBy);
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btnSubmit);
        }

        // SỬA LỖI UNBOXING: Kiểm tra validation phía Client (Case 20)
        public bool IsClientValidationTriggered()
        {
            // Thực hiện script và nhận kết quả về object
            var result = ((IJavaScriptExecutor)_driver).ExecuteScript(
                "return Array.from(document.querySelectorAll('input:invalid')).length > 0;");

            // Ép kiểu an toàn (Safe Unboxing)
            return (bool)(result ?? false);
        }

        // --- KIỂM TRA KẾT QUẢ (VERIFICATIONS) ---

        public bool IsRedirectedToLogin()
        {
            try
            {
                // Kiểm tra URL có chuyển về trang Login không
                return _wait.Until(d => d.Url.Contains("/Account/Login"));
            }
            catch { return false; }
        }

        public bool IsErrorDisplayed()
        {
            // Sử dụng ?. để truy cập Text an toàn
            return _driver.FindElements(errorSummaryBy).Any(e => !string.IsNullOrEmpty(e.Text?.Trim()));
        }

        public string GetSystemErrorMessage()
        {
            try
            {
                var errors = _driver.FindElements(errorSummaryBy)
                                   .Select(e => e.Text?.Trim())
                                   .Where(t => !string.IsNullOrEmpty(t));

                return errors.Any() ? string.Join(" | ", errors) : "Lỗi hệ thống không xác định";
            }
            catch { return "Lỗi không xác định"; }
        }
    }
}