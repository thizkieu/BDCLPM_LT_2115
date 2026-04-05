using FootballSeleniumTest.Models; // Đổi sang Models nếu Kiều dùng chung Model
using FootballSeleniumTest.Pages;
using FootballSeleniumTest.TestData;
using FootballSeleniumTest.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FootballSeleniumTest.Tests
{
    [TestFixture]
    public class LoginTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private ExcelHelper excelHelper;

        private readonly string excelPath = @"D:\BDCLPM_LT\DOAN\TestCN.xlsx";
        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "users.json");


        [SetUp]
        public void SetUp()
        {
            driver = DriverFactory.InitDriver();
            loginPage = new LoginPage(driver);
            excelHelper = new ExcelHelper();

            // Mở đúng Sheet Đăng nhập
            excelHelper.OpenExcel(excelPath, "Testcases 2 - Đăng nhập");
        }

        [Test]
        public void RunLoginTestsFromJson()
        {
            var testDataList = JsonDataReader.ReadTestData<UserData>(jsonPath);
            int currentRow = 1; // Ghi từ Dòng 2 trong Excel

            foreach (var user in testDataList)
            {
                string status = "";
                string actualResult = "";
                bool takeScreenshot = false;

                try
                {
                    driver.Navigate().GoToUrl("http://localhost:5033/Account/Login");

                    // --- BƯỚC 1: THỰC HIỆN HÀNH ĐỘNG ---
                    PerformAction(user);

                    // --- BƯỚC 2: KIỂM TRA TRẠNG THÁI ---
                    bool isSuccess = loginPage.IsLoginSuccess();
                    bool isError = loginPage.IsValidationErrorDisplayed();
                    string systemError = loginPage.GetFirstValidationErrorText();

                    // --- BƯỚC 3: XÉT 4 TRƯỜNG HỢP THEO YÊU CẦU CỦA KIỀU ---
                    if (user.expected == true) // Mong đợi thành công
                    {
                        if (isSuccess)
                        {
                            // [PASS 1]: Thành công đúng ý
                            status = "PASS";
                            actualResult = "Đăng nhập thành công vào hệ thống";
                            takeScreenshot = false; // Không chụp ảnh để báo cáo sạch đẹp
                        }
                        else
                        {
                            // [FAIL 2]: Có lỗi hệ thống phát sinh
                            status = "FAIL";
                            actualResult = string.IsNullOrEmpty(systemError) ? "Lỗi không xác định" : systemError;
                            takeScreenshot = true;
                        }
                    }
                    else // Mong đợi báo lỗi (user.expected == false)
                    {
                        if (isError && !isSuccess)
                        {
                            // [PASS 2]: Hệ thống báo lỗi đúng kịch bản
                            status = "PASS";
                            actualResult = "Hiển thị lỗi từ hệ thống";
                            takeScreenshot = true; // Chụp ảnh minh chứng có báo lỗi
                        }
                        else if (isSuccess)
                        {
                            // [FAIL 1]: Sai ràng buộc nhưng hệ thống vẫn cho vào
                            status = "FAIL";
                            actualResult = "Vẫn đăng nhập được không báo lỗi";
                            takeScreenshot = true;
                        }
                        else
                        {
                            // [FAIL 2]: Lỗi hệ thống khác
                            status = "FAIL";
                            actualResult = "Lỗi hệ thống: " + systemError;
                            takeScreenshot = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    status = "FAIL";
                    actualResult = "Lỗi kỹ thuật: " + ex.Message;
                    takeScreenshot = true;
                }
                finally
                {
                    // Lấy đường dẫn ảnh từ ổ D (D:\BDCLPM_LT\DOAN\Screenshots\...)
                    string screenshotPath = takeScreenshot ? ScreenshotHelper.Capture(driver) : "";

                    // Ghi trực tiếp vào Excel (Cột K, L, M)
                    ExcelHelper.WriteResult(currentRow, actualResult, status, screenshotPath);
                    currentRow++;
                }
            }
        }

        private void PerformAction(UserData user)
        {
            // (Giữ nguyên logic chuyển mạch các Case của Kiều)
            switch (user.action)
            {
                case "saved_login": loginPage.LoginWithSavedPassword(user.username); break;
                case "remember_login": loginPage.LoginWithRememberMe(user.username, user.password, true); break;
                case "dont_remember": loginPage.LoginWithRememberMe(user.username, user.password, false); break;
                case "forgot_form": loginPage.GoToForgotPassword(); break;
                case "forgot_request":
                case "forgot_empty":
                case "forgot_invalid":
                    loginPage.GoToForgotPassword();
                    loginPage.SendForgotRequest(user.username);
                    break;
                case "mobile_view":
                    driver.Manage().Window.Size = new System.Drawing.Size(375, 812);
                    loginPage.Login(user.username, user.password);
                    break;
                default: loginPage.Login(user.username, user.password); break;
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