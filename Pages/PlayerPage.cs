using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FootballSeleniumTest.Models;
using System;
using System.Linq;

namespace FootballSeleniumTest.Pages
{
    public class PlayerPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public PlayerPage(IWebDriver driver)
        {
            // Kiểm tra driver truyền vào không được null
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        // --- LOCATORS ---
        private readonly By maCTBy = By.Id("MaCT");
        private readonly By tenCTBy = By.Id("TenCauThu");
        private readonly By ngaySinhBy = By.Id("NgaySinh");
        private readonly By gioiTinhBy = By.Id("GioiTinh");
        private readonly By soAoBy = By.Id("SoAo");
        private readonly By viTriBy = By.Id("ViTri");
        private readonly By maDBBy = By.Id("MaDB");
        private readonly By trangThaiBy = By.Id("TrangThai");
        private readonly By btnSubmitBy = By.CssSelector("button[type='submit']");

        private readonly By filterSelectBy = By.Name("maDB");
        private readonly By btnFilterBy = By.CssSelector("button.btn-outline-success");
        private readonly By successTableBy = By.CssSelector(".table, .alert-info");
        private readonly By errorBy = By.CssSelector(".field-validation-error, .text-danger.field-validation-valid:not(:empty)");

        // --- HELPER METHODS ---
        private void JSClick(IWebElement? element)
        {
            if (element == null) return;
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            js.ExecuteScript("arguments[0].click();", element);
        }

        // --- ACTIONS ---

        public void FillPlayerForm(PlayerData data)
        {
            // 1. Điền Mã CT (Nếu có ô này - thường trang Create mới hiện)
            var maElements = _driver.FindElements(maCTBy);
            if (maElements.Any() && maElements[0].Displayed)
            {
                maElements[0].Clear();
                maElements[0].SendKeys(data.PlayerId ?? ""); // Chống lỗi SendKeys null
            }

            // 2. Điền Tên cầu thủ
            var txtTen = _wait.Until(ExpectedConditions.ElementIsVisible(tenCTBy));
            txtTen.Clear();
            txtTen.SendKeys(data.PlayerName ?? "");

            // 3. Ngày sinh
            if (!string.IsNullOrEmpty(data.Dob))
            {
                _driver.FindElement(ngaySinhBy).SendKeys(data.Dob);
            }

            // 4. Chọn Giới tính
            if (!string.IsNullOrEmpty(data.Gender))
            {
                new SelectElement(_driver.FindElement(gioiTinhBy)).SelectByText(data.Gender);
            }

            // 5. Số áo (Xử lý ToString an toàn)
            var txtSoAo = _driver.FindElement(soAoBy);
            txtSoAo.Clear();
            txtSoAo.SendKeys(data.JerseyNumber.ToString() ?? "");

            // 6. Vị trí
            var txtViTri = _driver.FindElement(viTriBy);
            txtViTri.Clear();
            txtViTri.SendKeys(data.Position ?? "");

            // 7. Mã đội
            var txtMaDB = _driver.FindElement(maDBBy);
            txtMaDB.Clear();
            txtMaDB.SendKeys(data.TeamId ?? "");

            // 8. Chọn Trạng thái
            if (!string.IsNullOrEmpty(data.Status))
            {
                new SelectElement(_driver.FindElement(trangThaiBy)).SelectByText(data.Status);
            }

            // Click Lưu
            JSClick(_driver.FindElement(btnSubmitBy));
        }

        public void ConfirmDelete()
        {
            var btnConfirm = _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.btn-danger")));
            JSClick(btnConfirm);
        }

        public void CancelDelete()
        {
            var btnCancel = _driver.FindElement(By.LinkText("⬅ Hủy"));
            JSClick(btnCancel);
        }

        public void FilterByTeam(string? teamId)
        {
            var selectElement = _driver.FindElement(filterSelectBy);
            var select = new SelectElement(selectElement);
            select.SelectByValue(teamId ?? ""); // Chống lỗi null
            JSClick(_driver.FindElement(btnFilterBy));
        }

        // --- VERIFICATIONS ---

        public bool IsActionSuccess()
        {
            try
            {
                return _wait.Until(d => d.FindElements(successTableBy).Count > 0);
            }
            catch { return false; }
        }

        public bool IsErrorDisplayed()
        {
            try
            {
                return _driver.FindElements(errorBy).Any(e => !string.IsNullOrEmpty(e.Text?.Trim()));
            }
            catch { return false; }
        }

        public string GetSystemErrorMessage()
        {
            try
            {
                // Sử dụng ? để lấy Text an toàn
                var error = _driver.FindElements(errorBy).FirstOrDefault(e => !string.IsNullOrEmpty(e.Text?.Trim()));
                return error?.Text ?? "Lỗi hệ thống không xác định";
            }
            catch { return "Lỗi không xác định"; }
        }
    }
}