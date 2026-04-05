using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FootballSeleniumTest.Models;
using System;
using System.Linq;

namespace FootballSeleniumTest.Pages
{
    public class SchedulePage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public SchedulePage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // --- LOCATORS (Khớp 100% ID trong HTML Kiều gửi) ---
        private By maLichBy = By.Id("MaLich");
        private By maTDBy = By.Id("MaTD"); // Dropdown Trận đấu
        private By ngayBy = By.Id("NgayThiDau");
        private By gioBy = By.Id("GioThiDau");
        private By diaDiemBy = By.Id("DiaDiem");
        private By trangThaiBy = By.Id("TrangThai"); // Dropdown Trạng thái
        private By btnSubmitBy = By.CssSelector("button[type='submit']");

        // Locators kiểm tra
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

        // 1. Điền Form Lịch thi đấu (Thêm và Sửa)
        public void FillScheduleForm(SchedulesData data)
        {
            // Điền Mã Lịch (Chỉ điền nếu ô này hiển thị và không bị khóa - Trang Create)
            var maElements = driver.FindElements(maLichBy);
            if (maElements.Count > 0 && maElements[0].Displayed && string.IsNullOrEmpty(maElements[0].GetAttribute("readonly")))
            {
                maElements[0].Clear();
                maElements[0].SendKeys(data.MaLich);
            }

            // Chọn Trận Đấu từ Dropdown
            if (!string.IsNullOrEmpty(data.MaTD))
            {
                new SelectElement(driver.FindElement(maTDBy)).SelectByValue(data.MaTD);
            }

            // FIX LỖI NGÀY/GIỜ: Dùng JS gán trực tiếp để tránh lỗi mặt nạ (mask)
            if (!string.IsNullOrEmpty(data.NgayThiDau))
            {
                JSSetValue(driver.FindElement(ngayBy), data.NgayThiDau); // yyyy-MM-dd
            }
            if (!string.IsNullOrEmpty(data.GioThiDau))
            {
                JSSetValue(driver.FindElement(gioBy), data.GioThiDau); // HH:mm
            }

            // Điền Địa điểm
            var txtDiaDiem = wait.Until(ExpectedConditions.ElementIsVisible(diaDiemBy));
            txtDiaDiem.Clear();
            txtDiaDiem.SendKeys(data.DiaDiem);

            // Chọn Trạng thái từ Dropdown
            if (!string.IsNullOrEmpty(data.TrangThai))
            {
                new SelectElement(driver.FindElement(trangThaiBy)).SelectByText(data.TrangThai);
            }

            // Nhấn Lưu / Cập nhật
            JSClick(driver.FindElement(btnSubmitBy));
        }

        // 2. Xác nhận xóa
        public void ConfirmDelete()
        {
            var btnDelete = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.btn-danger")));
            JSClick(btnDelete);
        }

        // 3. Hủy xóa
        public void CancelDelete()
        {
            var btnCancel = driver.FindElement(By.LinkText("Hủy"));
            JSClick(btnCancel);
        }

        // 4. Sắp xếp (Case 22 & 23)
        public void SortByColumn(string columnName)
        {
            var header = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath($"//th[contains(text(),'{columnName}')]")));
            JSClick(header);
        }

        // 5. Phân trang (Case 24)
        public void ClickNextPage()
        {
            var nextBtn = driver.FindElements(By.LinkText("Next")).FirstOrDefault()
                          ?? driver.FindElements(By.LinkText("2")).FirstOrDefault();
            if (nextBtn != null) JSClick(nextBtn);
        }

        // --- KIỂM TRA KẾT QUẢ (VERIFICATIONS) ---

        public bool IsActionSuccess()
        {
            try
            {
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