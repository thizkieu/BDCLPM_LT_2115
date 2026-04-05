using FootballSeleniumTest.Models;
using FootballSeleniumTest.Pages;
using FootballSeleniumTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Collections.Generic;

namespace FootballSeleniumTest.Tests
{
    [TestFixture]
    public class RegisterTests
    {
        private IWebDriver driver;
        private RegisterPage registerPage;
        private ExcelHelper excelHelper;

        private readonly string excelPath = @"D:\BDCLPM_LT\DOAN\TestCN.xlsx";
        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "register.json");


        [SetUp]
        public void SetUp()
        {
            driver = DriverFactory.InitDriver();
            registerPage = new RegisterPage(driver);
            excelHelper = new ExcelHelper();

            // Mở đúng Sheet Quản lý Đăng ký trong file Excel của Kiều
            excelHelper.OpenExcel(excelPath, "Testcases 1 - Đăng ký");
        }

        [Test]
        public void RunRegisterTests()
        {
            var testData = JsonDataReader.ReadTestData<RegisterData>(jsonPath);
            int currentRow = 1;

            foreach (var user in testData)
            {
                string status = "";
                string actualResult = "";
                bool takeScreenshot = false;

                try
                {
                    // 1. Luôn điều hướng về trang Đăng ký trước mỗi case
                    driver.Navigate().GoToUrl("http://localhost:5033/Account/Register");

                    // 2. THỰC HIỆN HÀNH ĐỘNG
                    if (user.Action == "check_client_validation")
                    {
                        // Case 20: Chỉ nhấn nút để check HTML5 validation
                        IWebElement btn = driver.FindElement(By.CssSelector("button[type='submit']"));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", btn);
                    }
                    else
                    {
                        // Các case còn lại: Điền form và nhấn nút
                        registerPage.FillRegisterForm(user);
                    }

                    // 3. LẤY TRẠNG THÁI HỆ THỐNG
                    bool isSuccess = registerPage.IsRedirectedToLogin(); // Thành công là về trang Login
                    bool isError = registerPage.IsErrorDisplayed() || registerPage.IsClientValidationTriggered();
                    string systemError = registerPage.GetSystemErrorMessage();

                    // 4. XÉT 4 TRƯỜNG HỢP PASS/FAIL THEO YÊU CẦU CỦA KIỀU
                    if (user.Expected == true) // Kịch bản mong đợi THÀNH CÔNG
                    {
                        if (isSuccess)
                        {
                            // [PASS TRƯỜNG HỢP 1]: Thành công đúng ý
                            status = "PASS";
                            actualResult = "Đăng ký thành công vào hệ thống"; // Theo yêu cầu văn bản của Kiều
                            takeScreenshot = false; // Không chụp ảnh
                        }
                        else
                        {
                            // [FAIL TRƯỜNG HỢP 2]: Lỗi hệ thống phát sinh (Nhập lỗi thực tế)
                            status = "FAIL";
                            actualResult = systemError;
                            takeScreenshot = true;
                        }
                    }
                    else // Kịch bản mong đợi CÓ BÁO LỖI (Expected: false)
                    {
                        if (isError && !isSuccess)
                        {
                            // [PASS TRƯỜNG HỢP 2]: Báo lỗi đúng kịch bản
                            status = "PASS";
                            actualResult = "Hiển thị lỗi từ hệ thống";
                            takeScreenshot = true; // Chụp ảnh minh chứng có báo lỗi
                        }
                        else if (isSuccess)
                        {
                            // [FAIL TRƯỜNG HỢP 1]: Hệ thống cho lọt lưới dữ liệu sai
                            status = "FAIL";
                            actualResult = "Vẫn đăng ký được không báo lỗi";
                            takeScreenshot = true;
                        }
                        else
                        {
                            // [FAIL TRƯỜNG HỢP 2]: Các lỗi khác không xác định
                            status = "FAIL";
                            actualResult = "Lỗi hệ thống: " + systemError;
                            takeScreenshot = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    status = "FAIL";
                    actualResult = "Lỗi ngoại lệ: " + ex.Message;
                    takeScreenshot = true;
                }
                finally
                {
                    // Chụp ảnh minh chứng nếu cần
                    string shotPath = takeScreenshot ? ScreenshotHelper.Capture(driver) : "";

                    // Ghi vào Excel đúng cột
                    ExcelHelper.WriteResult(currentRow, actualResult, status, shotPath);
                    currentRow++;
                }
            }
        }

        [TearDown]
        public void Cleanup()
        {
            try { excelHelper?.CloseExcel(); } catch { }
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}