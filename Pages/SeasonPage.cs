using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FootballSeleniumTest.Models;
using System;
using System.Linq;

namespace FootballSeleniumTest.Pages
{
    public class SeasonPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public SeasonPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // --- LOCATORS (Khớp 100% ID/Name trong HTML Mùa giải) ---
        private By maMGBy = By.Id("MaMG");
        private By tenMGBy = By.Id("TenMuaGiai");
        private By thoiGianBy = By.Id("ThoiGian");
        private By moTaBy = By.Id("MoTa");
        private By btnSubmitBy = By.CssSelector("button[type='submit']");

        // Locators cho kiểm tra kết quả
        private By successTableBy = By.CssSelector(".table, .alert-info");
        private By errorBy = By.CssSelector(".field-validation-error, .text-danger.field-validation-valid:not(:empty)");

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

        // 1. Điền Form Mùa giải (Dùng cho cả Thêm và Sửa)
        public void FillSeasonForm(SeasonsData data)
        {
            // Kiểm tra ô Mã mùa giải (Chỉ điền nếu ô này hiển thị và không bị ẩn - Trang Create)
            var maElements = driver.FindElements(maMGBy);
            if (maElements.Count > 0 && maElements[0].Displayed && string.IsNullOrEmpty(maElements[0].GetAttribute("readonly")))
            {
                maElements[0].Clear();
                maElements[0].SendKeys(data.MaMG);
            }

            wait.Until(ExpectedConditions.ElementIsVisible(tenMGBy)).Clear();
            driver.FindElement(tenMGBy).SendKeys(data.TenMuaGiai);

            // FIX LỖI THỜI GIAN: Dùng JavaScript để ép đúng định dạng yyyy-MM-ddTHH:mm
            if (!string.IsNullOrEmpty(data.ThoiGian))
            {
                var inputTime = driver.FindElement(thoiGianBy);
                JSSetValue(inputTime, data.ThoiGian);
            }

            driver.FindElement(moTaBy).Clear();
            driver.FindElement(moTaBy).SendKeys(data.MoTa);

            // Nhấn nút Lưu (Dùng chung cho cả Thêm/Sửa theo HTML của Kiều)
            JSClick(driver.FindElement(btnSubmitBy));
        }

        // 2. Xác nhận xóa mùa giải
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

        // 4. Sắp xếp theo ngày (Case 22 & 24)
        public void ClickSortByDate()
        {
            var header = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//th[contains(text(),'Thời gian')]")));
            JSClick(header);
        }

        // --- KIỂM TRA KẾT QUẢ (VERIFICATIONS) ---

        public bool IsActionSuccess()
        {
            try
            {
                // Thành công khi quay lại được trang có bảng danh sách
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