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
    public class SeasonTests
    {
        private IWebDriver driver;
        private SeasonPage seasonPage;
        private LoginPage loginPage;
        private ExcelHelper excelHelper;

        private readonly string excelPath = @"D:\BDCLPM_LT\DOAN\TestCN.xlsx";
        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "seasons.json");


        [SetUp]
        public void SetUp()
        {
            driver = DriverFactory.InitDriver();
            seasonPage = new SeasonPage(driver);
            loginPage = new LoginPage(driver);
            excelHelper = new ExcelHelper();

            // Mở đúng Sheet quản lý Mùa giải
            excelHelper.OpenExcel(excelPath, "Testcases 7 - QL Mùa giải");

            // --- BƯỚC ĐĂNG NHẬP BẮT BUỘC ---
            driver.Navigate().GoToUrl("http://localhost:5033/Account/Login");
            loginPage.Login("admin", "admin123");

            if (!loginPage.IsLoginSuccess())
            {
                Assert.Fail("Lỗi: Không thể đăng nhập tài khoản Admin.");
            }
        }

        [Test]
        public void RunSeasonTests()
        {
            var testData = JsonDataReader.ReadTestData<SeasonsData>(jsonPath);
            int currentRow = 1;

            foreach (var season in testData)
            {
                string status = "";
                string actualResult = "";
                bool takeScreenshot = false;

                try
                {
                    // 1. THỰC HIỆN HÀNH ĐỘNG THEO "ACTION"
                    switch (season.Action)
                    {
                        case "add_season":
                            driver.Navigate().GoToUrl("http://localhost:5033/MuaGiai/Create");
                            seasonPage.FillSeasonForm(season);
                            break;

                        case "edit_season":
                            driver.Navigate().GoToUrl($"http://localhost:5033/MuaGiai/Edit/{season.MaMG}");
                            seasonPage.FillSeasonForm(season);
                            break;

                        case "delete_season":
                            driver.Navigate().GoToUrl($"http://localhost:5033/MuaGiai/Delete/{season.MaMG}");
                            seasonPage.ConfirmDelete();
                            break;

                        case "delete_cancel":
                            driver.Navigate().GoToUrl($"http://localhost:5033/MuaGiai/Delete/{season.MaMG}");
                            seasonPage.CancelDelete();
                            break;

                        case "view_details":
                            driver.Navigate().GoToUrl($"http://localhost:5033/MuaGiai/Details/{season.MaMG}");
                            break;

                        case "sort_season":
                        case "sort_start_date":
                            driver.Navigate().GoToUrl("http://localhost:5033/MuaGiai");
                            seasonPage.ClickSortByDate();
                            break;

                        default:
                            driver.Navigate().GoToUrl("http://localhost:5033/MuaGiai");
                            break;
                    }

                    // 2. LẤY TRẠNG THÁI HỆ THỐNG
                    bool isSuccess = seasonPage.IsActionSuccess();
                    bool isError = seasonPage.IsErrorDisplayed();
                    string systemError = seasonPage.GetSystemErrorMessage();

                    // 3. XÉT 4 TRƯỜNG HỢP PASS/FAIL THEO YÊU CẦU CỦA KIỀU
                    if (season.Expected == true) // Kịch bản mong đợi THÀNH CÔNG
                    {
                        if (isSuccess)
                        {
                            // [PASS TRƯỜNG HỢP 1]: Thành công đúng ý
                            status = "PASS";
                            actualResult = "Tạo mùa giải thành công"; // Chuỗi văn bản Kiều yêu cầu
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
                            // [PASS TRƯỜNG HỢP 2]: Báo lỗi đúng kịch bản
                            status = "PASS";
                            actualResult = "Hiển thị lỗi từ hệ thống";
                            takeScreenshot = true;
                        }
                        else if (isSuccess)
                        {
                            // [FAIL TRƯỜNG HỢP 1]: Hệ thống không ràng buộc (cho lọt lưới)
                            status = "FAIL";
                            actualResult = "Vẫn tạo mùa giải được không báo lỗi"; // Chuỗi văn bản Kiều yêu cầu
                            takeScreenshot = true;
                        }
                        else
                        {
                            // [FAIL TRƯỜER HỢP 2]: Các lỗi khác
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