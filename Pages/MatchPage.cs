using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using FootballSeleniumTest.Models;
using System;
using System.Linq;

namespace FootballSeleniumTest.Pages
{
    public class MatchPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public MatchPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // --- LOCATORS ---
        private By maTDBy = By.Id("MaTD");
        private By tenTranBy = By.Id("TenTran");
        private By doi1By = By.Id("Doi1");
        private By doi2By = By.Id("Doi2");
        private By thoiGianBy = By.Id("ThoiGian");
        private By diaDiemBy = By.Id("DiaDiem");
        private By maVDBy = By.Id("MaVD"); // ID trong Form
        private By btnSubmitBy = By.CssSelector("button[type='submit']");

        private By successTableBy = By.CssSelector(".table, .alert-info");
        private By errorBy = By.CssSelector(".field-validation-error, .text-danger.field-validation-valid:not(:empty)");

        // --- HELPER: JavaScript Click & Set Value ---
        private void JSClick(IWebElement element)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
        }

        private void JSSetValue(IWebElement element, string value)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = arguments[1];", element, value);
        }

        // --- ACTIONS ---

        public void FillMatchForm(MatchesData data)
        {
            // 1. Điền Mã trận (Nếu hiển thị)
            var maElems = driver.FindElements(maTDBy);
            if (maElems.Count > 0 && maElems[0].Displayed)
            {
                maElems[0].Clear();
                maElems[0].SendKeys(data.MaTD);
            }

            // 2. Điền các trường văn bản
            wait.Until(ExpectedConditions.ElementIsVisible(tenTranBy)).Clear();
            driver.FindElement(tenTranBy).SendKeys(data.TenTran);
            driver.FindElement(doi1By).Clear();
            driver.FindElement(doi1By).SendKeys(data.Doi1);
            driver.FindElement(doi2By).Clear();
            driver.FindElement(doi2By).SendKeys(data.Doi2);

            // 3. FIX LỖI THỜI GIAN: Dùng JavaScript để gán thẳng giá trị yyyy-MM-ddTHH:mm
            if (!string.IsNullOrEmpty(data.ThoiGian))
            {
                var inputTime = driver.FindElement(thoiGianBy);
                JSSetValue(inputTime, data.ThoiGian);
            }

            driver.FindElement(diaDiemBy).Clear();
            driver.FindElement(diaDiemBy).SendKeys(data.DiaDiem);

            // 4. Chọn Vòng đấu
            if (!string.IsNullOrEmpty(data.MaVD))
            {
                new SelectElement(driver.FindElement(maVDBy)).SelectByValue(data.MaVD);
            }

            // 5. Nhấn Lưu
            JSClick(driver.FindElement(btnSubmitBy));
        }

        public void ConfirmDelete()
        {
            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.btn-danger")));
            JSClick(btn);
        }

        // FIX LỖI CASE 23: Cập nhật lại Selector cho ô lọc trên trang danh sách
        public void FilterByRound(string roundId)
        {
            try
            {
                // Thử tìm theo ID trước (vì Form lọc thường dùng ID tương tự Form thêm)
                var filterDropdown = wait.Until(d => d.FindElement(By.Id("MaVD")) ?? d.FindElement(By.Name("MaVD")));
                new SelectElement(filterDropdown).SelectByValue(roundId);

                // Tìm nút "Lọc" (Dựa vào icon fas fa-filter trong HTML bạn gửi cho Cầu thủ)
                var btnFilter = driver.FindElements(By.CssSelector("button.btn-outline-success, button[type='submit']")).FirstOrDefault();
                if (btnFilter != null) JSClick(btnFilter);
            }
            catch (Exception ex)
            {
                throw new Exception("Không tìm thấy bộ lọc Vòng đấu trên trang này: " + ex.Message);
            }
        }

        public void ClickSortByDate()
        {
            var header = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//th[contains(text(),'Thời gian')]")));
            JSClick(header);
        }

        // --- VERIFICATIONS ---

        public bool IsActionSuccess() => driver.FindElements(By.CssSelector(".table, .alert-info")).Count > 0;

        public bool IsErrorDisplayed() => driver.FindElements(errorBy).Any(e => !string.IsNullOrEmpty(e.Text.Trim()));

        public string GetSystemErrorMessage()
        {
            var error = driver.FindElements(errorBy).FirstOrDefault(e => !string.IsNullOrEmpty(e.Text.Trim()));
            return error != null ? error.Text : "Lỗi không xác định từ hệ thống";
        }
    }
}