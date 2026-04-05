using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FootballSeleniumTest.Models;
using System;
using System.Linq;

namespace FootballSeleniumTest.Pages
{
    public class RoundPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public RoundPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // --- LOCATORS (Khớp 100% với HTML Vòng đấu Kiều gửi) ---
        private By maVDBy = By.Id("MaVD");
        private By tenVongBy = By.Id("TenVong");
        private By thoiGianBy = By.Id("ThoiGian");
        private By maMGBy = By.Id("MaMG"); // Dropdown Mùa giải
        private By btnSubmitBy = By.CssSelector("button[type='submit']");

        // Locators cho danh sách và thông báo
        private By successTableBy = By.CssSelector(".table, .alert-info");
        private By errorBy = By.CssSelector(".field-validation-error, .text-danger.field-validation-valid:not(:empty)");
        private By nextButtonBy = By.LinkText("Next"); // Cho case phân trang

        // --- HELPER METHODS ---
        private void JSClick(IWebElement element)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
        }

        private void JSSetValue(IWebElement element, string value)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = arguments[1];", element, value);
        }

        // --- HÀNH ĐỘNG (ACTIONS) ---

        // 1. Điền Form Vòng đấu (Dùng cho cả Thêm và Sửa)
        public void FillRoundForm(RoundsData data)
        {
            // Điền Mã vòng (Nếu ô này không bị readonly - thường ở trang Create)
            var maElem = driver.FindElement(maVDBy);
            if (maElem.Enabled && string.IsNullOrEmpty(maElem.GetAttribute("readonly")))
            {
                maElem.Clear();
                maElem.SendKeys(data.MaVD);
            }

            wait.Until(ExpectedConditions.ElementIsVisible(tenVongBy)).Clear();
            driver.FindElement(tenVongBy).SendKeys(data.TenVong);

            // XỬ LÝ THỜI GIAN: Dùng JavaScript để ép đúng định dạng yyyy-MM-ddTHH:mm
            if (!string.IsNullOrEmpty(data.ThoiGian))
            {
                var inputTime = driver.FindElement(thoiGianBy);
                JSSetValue(inputTime, data.ThoiGian);
            }

            // Chọn Mùa giải từ Dropdown
            if (!string.IsNullOrEmpty(data.MaMG))
            {
                new SelectElement(driver.FindElement(maMGBy)).SelectByValue(data.MaMG);
            }

            // Nhấn Lưu vòng đấu / Cập nhật
            JSClick(driver.FindElement(btnSubmitBy));
        }

        // 2. Xác nhận xóa
        public void ConfirmDelete()
        {
            // Tìm nút "Xóa" màu đỏ trong trang xác nhận
            var btnDelete = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.btn-danger")));
            JSClick(btnDelete);
        }

        // 3. Hủy xóa
        public void CancelDelete()
        {
            var btnCancel = driver.FindElement(By.LinkText("Hủy"));
            JSClick(btnCancel);
        }

        // 4. Sắp xếp theo thời gian (Case 15)
        public void ClickSortByTime()
        {
            var header = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//th[contains(text(),'Thời gian dự kiến')]")));
            JSClick(header);
        }

        // 5. Chuyển trang (Case 16)
        public void ClickNextPage()
        {
            var btnNext = driver.FindElements(nextButtonBy).FirstOrDefault();
            if (btnNext != null) JSClick(btnNext);
        }

        // --- KIỂM TRA KẾT QUẢ (VERIFICATIONS) ---

        public bool IsActionSuccess()
        {
            try
            {
                // Kiểm tra xem có quay lại trang danh sách và thấy bảng dữ liệu không
                return wait.Until(d => d.FindElements(successTableBy).Count > 0);
            }
            catch { return false; }
        }

        public bool IsErrorDisplayed()
        {
            return driver.FindElements(errorBy).Any(e => !string.IsNullOrEmpty(e.Text.Trim()));
        }

        public string GetSystemErrorMessage()
        {
            var error = driver.FindElements(errorBy).FirstOrDefault(e => !string.IsNullOrEmpty(e.Text.Trim()));
            return error != null ? error.Text : "Lỗi không xác định từ hệ thống";
        }
    }
}