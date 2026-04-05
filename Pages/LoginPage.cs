using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;

namespace FootballSeleniumTest.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public LoginPage(IWebDriver driver)
        {
            // Đảm bảo driver không bao giờ null khi khởi tạo
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
        }

        // ================= LOCATORS (Sử dụng hàm để lấy Element an toàn) =================

        private IWebElement TxtUsername => _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("TenDangNhap")));
        private IWebElement TxtPassword => _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("MatKhau")));
        private IWebElement BtnLogin => _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[type='submit'].btn-login")));

        // Checkbox và Link có thể không xuất hiện ngay, nên dùng Until để tìm
        private IWebElement? ChkRemember => _driver.FindElements(By.Id("GhiNho")).FirstOrDefault();
        private IWebElement LinkForgot => _wait.Until(ExpectedConditions.ElementToBeClickable(By.LinkText("Quên mật khẩu?")));

        private readonly By _validationErrorBy = By.CssSelector(".text-danger.field-validation-error, .alert.alert-danger, .validation-summary-errors");
        private readonly By _adminPanelIdentifierBy = By.XPath("//*[contains(text(), 'BanToChuc')]");

        // ================= METHODS =================

        public void Login(string? user, string? pass)
        {
            TxtUsername.Clear();
            // Sử dụng ?? "" để đảm bảo không bao giờ gửi giá trị null vào SendKeys
            TxtUsername.SendKeys(user ?? "");

            TxtPassword.Clear();
            TxtPassword.SendKeys(pass ?? "");

            BtnLogin.Click();
        }

        public void LoginWithRememberMe(string? user, string? pass, bool shouldCheck)
        {
            TxtUsername.Clear();
            TxtUsername.SendKeys(user ?? "");
            TxtPassword.Clear();
            TxtPassword.SendKeys(pass ?? "");

            // Kiểm tra an toàn cho Checkbox
            var checkbox = ChkRemember;
            if (checkbox != null && shouldCheck != checkbox.Selected)
            {
                checkbox.Click();
            }
            BtnLogin.Click();
        }

        public void LoginWithSavedPassword(string? user)
        {
            TxtUsername.Clear();
            TxtUsername.SendKeys(user ?? "");
            // Để trình duyệt tự điền mật khẩu
            BtnLogin.Click();
        }

        public void GoToForgotPassword()
        {
            LinkForgot.Click();
        }

        public void SendForgotRequest(string? emailOrUser)
        {
            var txtForgotEmail = _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Email")));
            txtForgotEmail.Clear();
            txtForgotEmail.SendKeys(emailOrUser ?? "");

            var btnSendRequest = _driver.FindElement(By.CssSelector("button.btn-primary"));
            btnSendRequest.Click();
        }

        public void Logout()
        {
            _driver.Navigate().GoToUrl("http://localhost:5033/Account/Logout");
        }

        public string GetUsernameValue() => TxtUsername.GetAttribute("value") ?? "";
        public string GetPasswordValue() => TxtPassword.GetAttribute("value") ?? "";

        // ================= KIỂM TRA TRẠNG THÁI =================

        public bool IsLoginSuccess()
        {
            try
            {
                var shortWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));
                return shortWait.Until(d => d.FindElement(_adminPanelIdentifierBy)).Displayed;
            }
            catch { return false; }
        }

        public bool IsValidationErrorDisplayed()
        {
            // Kiểm tra Any() an toàn trên danh sách element
            return _driver.FindElements(_validationErrorBy).Any(e => e.Displayed);
        }

        public string GetFirstValidationErrorText()
        {
            try
            {
                // Sử dụng ? để tránh lỗi nếu không tìm thấy element nào
                var error = _driver.FindElements(_validationErrorBy).FirstOrDefault(e => e.Displayed);
                return error?.Text ?? "Không tìm thấy lỗi cụ thể";
            }
            catch { return "Lỗi hệ thống"; }
        }
    }
}