using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.IO;
using System.Linq;

namespace FootballSeleniumTest.Pages
{
    public class TeamPage
    {
        private IWebDriver driver;
        private WebDriverWait wait;

        public TeamPage(IWebDriver driver)
        {
            this.driver = driver;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        // --- LOCATORS (Đã chuẩn hóa theo HTML thực tế) ---
        private By maDBBy = By.Id("MaDB");
        private By tenDoiBy = By.Id("TenDoi");
        private By truongHocBy = By.Id("TruongHoc");
        private By hlvBy = By.Id("HuanLuyenVien");
        private By slCauThuBy = By.Id("SoLuongCauThu");
        private By hinhAnhBy = By.Name("HinhAnhFile");
        private By btnSubmitBy = By.CssSelector("button[type='submit']");

        // Định vị lỗi và nhận diện thành công
        private By errorBy = By.CssSelector(".field-validation-error, .text-danger.field-validation-valid:not(:empty)");
        private By successTableBy = By.CssSelector(".table, .badge.bg-info"); // Thấy bảng danh sách là thành công

        // --- HELPER: Click bằng JavaScript để tránh lỗi bị che (Intercepted) ---
        private void JSClick(IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", element);
        }

        // --- ACTIONS ---

        // Hàm điền form dùng chung cho cả Thêm và Sửa
        public void FillTeamForm(string id, string name, string school, string hlv, int players, string logoPath)
        {
            // Kiểm tra xem ô MaDB có hiển thị không (Create có, Edit là hidden)
            var maDBElements = driver.FindElements(maDBBy);
            if (maDBElements.Count > 0 && maDBElements[0].Displayed)
            {
                maDBElements[0].Clear();
                maDBElements[0].SendKeys(id);
            }

            wait.Until(ExpectedConditions.ElementIsVisible(tenDoiBy)).Clear();
            driver.FindElement(tenDoiBy).SendKeys(name);

            driver.FindElement(truongHocBy).Clear();
            driver.FindElement(truongHocBy).SendKeys(school);

            driver.FindElement(hlvBy).Clear();
            driver.FindElement(hlvBy).SendKeys(hlv);

            driver.FindElement(slCauThuBy).Clear();
            driver.FindElement(slCauThuBy).SendKeys(players.ToString());

            // Upload ảnh nếu có đường dẫn hợp lệ
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                driver.FindElement(hinhAnhBy).SendKeys(logoPath);
            }

            // Dùng JSClick cho nút Lưu/Cập nhật để chắc chắn không bị che
            JSClick(driver.FindElement(btnSubmitBy));
        }

        // Nhấn nút Xóa (màu đỏ) trong trang xác nhận xóa
        public void ConfirmDelete()
        {
            var btnDelete = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.btn-danger")));
            JSClick(btnDelete);
        }

        // Nhấn nút Hủy (màu xám)
        public void CancelDelete()
        {
            var btnCancel = driver.FindElement(By.LinkText("Hủy"));
            JSClick(btnCancel);
        }

        // Nhấn tiêu đề cột để sắp xếp
        public void ClickSortByName()
        {
            var header = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//th[contains(text(),'Tên đội')]")));
            JSClick(header);
        }

        // Chuyển trang
        public void ClickPagination(string pageNum)
        {
            var pageLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.LinkText(pageNum)));
            JSClick(pageLink);
        }

        // --- VERIFICATIONS (Kiểm tra kết quả) ---

        public bool IsActionSuccess()
        {
            try
            {
                // Sau khi Lưu/Sửa/Xóa, nếu quay về được trang danh sách (có bảng) là PASS
                return wait.Until(d => d.FindElements(successTableBy).Count > 0);
            }
            catch { return false; }
        }

        public bool IsErrorDisplayed()
        {
            try
            {
                // Kiểm tra xem có bất kỳ thông báo lỗi đỏ nào hiện lên không
                return driver.FindElements(errorBy).Any(e => !string.IsNullOrEmpty(e.Text.Trim()));
            }
            catch { return false; }
        }

        public string GetSystemErrorMessage()
        {
            try
            {
                var error = driver.FindElements(errorBy).FirstOrDefault(e => !string.IsNullOrEmpty(e.Text.Trim()));
                return error != null ? error.Text : "Lỗi không xác định";
            }
            catch { return "Lỗi không xác định"; }
        }
    }
}