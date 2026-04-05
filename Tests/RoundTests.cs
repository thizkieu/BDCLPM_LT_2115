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
    public class RoundTests
    {
        private IWebDriver driver;
        private RoundPage roundPage;
        private LoginPage loginPage;
        private ExcelHelper excelHelper;

        private readonly string excelPath = @"D:\BDCLPM_LT\DOAN\TestCN.xlsx";
        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "rounds.json");


        [SetUp]
        public void SetUp()
        {
            driver = DriverFactory.InitDriver();
            roundPage = new RoundPage(driver);
            loginPage = new LoginPage(driver);
            excelHelper = new ExcelHelper();

            // Mở đúng Sheet quản lý Vòng đấu
            excelHelper.OpenExcel(excelPath, "Testcases 8 - QL Vòng đấu");

            // --- BƯỚC ĐĂNG NHẬP BẮT BUỘC ---
            driver.Navigate().GoToUrl("http://localhost:5033/Account/Login");
            loginPage.Login("admin", "admin123");

            if (!loginPage.IsLoginSuccess())
            {
                Assert.Fail("Lỗi: Không thể đăng nhập tài khoản Admin.");
            }
        }

        [Test]
        public void RunRoundTests()
        {
            var testData = JsonDataReader.ReadTestData<RoundsData>(jsonPath);
            int currentRow = 1;

            foreach (var round in testData)
            {
                string status = "";
                string actualResult = "";
                bool takeScreenshot = false;

                try
                {
                    // 1. THỰC HIỆN HÀNH ĐỘNG THEO "ACTION"
                    switch (round.Action)
                    {
                        case "add_round":
                            driver.Navigate().GoToUrl("http://localhost:5033/VongDau/Create");
                            roundPage.FillRoundForm(round);
                            break;

                        case "edit_round":
                            driver.Navigate().GoToUrl($"http://localhost:5033/VongDau/Edit/{round.MaVD}");
                            roundPage.FillRoundForm(round);
                            break;

                        case "delete_round":
                            driver.Navigate().GoToUrl($"http://localhost:5033/VongDau/Delete/{round.MaVD}");
                            roundPage.ConfirmDelete();
                            break;

                        case "delete_cancel":
                            driver.Navigate().GoToUrl($"http://localhost:5033/VongDau/Delete/{round.MaVD}");
                            roundPage.CancelDelete();
                            break;

                        case "sort_round":
                            driver.Navigate().GoToUrl("http://localhost:5033/VongDau");
                            roundPage.ClickSortByTime();
                            break;

                        case "pagination":
                            driver.Navigate().GoToUrl("http://localhost:5033/VongDau");
                            roundPage.ClickNextPage();
                            break;

                        default:
                            driver.Navigate().GoToUrl("http://localhost:5033/VongDau");
                            break;
                    }

                    // 2. LẤY TRẠNG THÁI HỆ THỐNG
                    bool isSuccess = roundPage.IsActionSuccess();
                    bool isError = roundPage.IsErrorDisplayed();
                    string systemError = roundPage.GetSystemErrorMessage();

                    // 3. XÉT 4 TRƯỜNG HỢP PASS/FAIL THEO YÊU CẦU CỦA KIỀU
                    if (round.Expected == true) // Kịch bản mong đợi THÀNH CÔNG
                    {
                        if (isSuccess)
                        {
                            // [PASS TRƯỜNG HỢP 1]: Thành công đúng ý
                            status = "PASS";
                            actualResult = "Tạo vòng đấu thành công";
                            takeScreenshot = false;
                        }
                        else
                        {
                            // [FAIL TRƯỜNG HỢP 2]: Lỗi hệ thống phát sinh
                            status = "FAIL";
                            actualResult = string.IsNullOrEmpty(systemError) ? "Lỗi không xác định" : systemError;
                            takeScreenshot = true;
                        }
                    }
                    else // Kịch bản mong đợi CÓ BÁO LỖI (Expected: false)
                    {
                        if (isError && !isSuccess)
                        {
                            // [PASS TRƯỜNG HỢP 2]: Báo lỗi đúng yêu cầu test case
                            status = "PASS";
                            actualResult = "Hiển thị lỗi từ hệ thống";
                            takeScreenshot = true; // Chụp ảnh minh chứng báo lỗi
                        }
                        else if (isSuccess)
                        {
                            // [FAIL TRƯỜNG HỢP 1]: Hệ thống không ràng buộc (lọt lưới)
                            status = "FAIL";
                            actualResult = "Vẫn tạo vòng đấu được không báo lỗi";
                            takeScreenshot = true;
                        }
                        else
                        {
                            // [FAIL TRƯỜNG HỢP 2]: Lỗi hệ thống khác
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
                    // Lấy đường dẫn ảnh thô từ ổ D (D:\BDCLPM_LT\DOAN\Screenshots\...)
                    string shotPath = takeScreenshot ? ScreenshotHelper.Capture(driver) : "";

                    // Ghi vào Excel đúng cột K, L, M
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