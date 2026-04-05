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
    public class MatchTests
    {
        private IWebDriver driver;
        private MatchPage matchPage;
        private LoginPage loginPage;
        private ExcelHelper excelHelper;

        private readonly string excelPath = @"D:\BDCLPM_LT\DOAN\TestCN.xlsx";
        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "matches.json");


        [SetUp]
        public void SetUp()
        {
            driver = DriverFactory.InitDriver();
            matchPage = new MatchPage(driver);
            loginPage = new LoginPage(driver);
            excelHelper = new ExcelHelper();

            excelHelper.OpenExcel(excelPath, "Testcases 6 - QL Trận đấu");

            // --- ĐĂNG NHẬP ADMIN ---
            driver.Navigate().GoToUrl("http://localhost:5033/Account/Login");
            loginPage.Login("admin", "admin123");

            if (!loginPage.IsLoginSuccess())
            {
                Assert.Fail("Lỗi: Không thể đăng nhập tài khoản Admin.");
            }
        }

        [Test]
        public void RunMatchTests()
        {
            var testData = JsonDataReader.ReadTestData<MatchesData>(jsonPath);
            int currentRow = 1;

            foreach (var match in testData)
            {
                string status = "";
                string actualResult = "";
                bool takeScreenshot = false;

                try
                {
                    // 1. THỰC HIỆN HÀNH ĐỘNG THEO "ACTION"
                    switch (match.Action)
                    {
                        case "add_match":
                            driver.Navigate().GoToUrl("http://localhost:5033/TranDau/Create");
                            matchPage.FillMatchForm(match);
                            break;

                        case "edit_match":
                        case "update_result":
                        case "update_result_negative":
                        case "update_result_incomplete":
                        case "update_result_decimal":
                        case "edit_locked":
                            driver.Navigate().GoToUrl($"http://localhost:5033/TranDau/Edit/{match.MaTD}");
                            matchPage.FillMatchForm(match);
                            break;

                        case "delete_match":
                            driver.Navigate().GoToUrl($"http://localhost:5033/TranDau/Delete/{match.MaTD}");
                            matchPage.ConfirmDelete();
                            break;

                        case "filter_round":
                            driver.Navigate().GoToUrl("http://localhost:5033/TranDau");
                            matchPage.FilterByRound(match.MaVD);
                            break;

                        case "view_details":
                            driver.Navigate().GoToUrl($"http://localhost:5033/TranDau/Details/{match.MaTD}");
                            break;

                        default:
                            driver.Navigate().GoToUrl("http://localhost:5033/TranDau");
                            break;
                    }

                    // 2. LẤY TRẠNG THÁI HỆ THỐNG
                    bool isSuccess = matchPage.IsActionSuccess();
                    bool isError = matchPage.IsErrorDisplayed();
                    string systemError = matchPage.GetSystemErrorMessage();

                    // 3. XÉT 4 TRƯỜNG HỢP PASS/FAIL THEO YÊU CẦU CỦA KIỀU
                    if (match.Expected == true) // Kịch bản mong chờ THÀNH CÔNG
                    {
                        if (isSuccess)
                        {
                            status = "PASS";
                            actualResult = "Tạo trận đấu thành công"; // Chuỗi văn bản Kiều yêu cầu
                            takeScreenshot = false;
                        }
                        else
                        {
                            status = "FAIL";
                            actualResult = string.IsNullOrEmpty(systemError) ? "Lỗi không xác định" : systemError;
                            takeScreenshot = true;
                        }
                    }
                    else // Kịch bản mong chờ CÓ BÁO LỖI (Expected: false)
                    {
                        if (isError && !isSuccess)
                        {
                            status = "PASS";
                            actualResult = "Hiển thị lỗi từ hệ thống";
                            takeScreenshot = true;
                        }
                        else if (isSuccess)
                        {
                            status = "FAIL";
                            actualResult = "Vẫn tạo trận đấu được không báo lỗi";
                            takeScreenshot = true;
                        }
                        else
                        {
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
                    // Lấy đường dẫn ảnh thô từ ổ D
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