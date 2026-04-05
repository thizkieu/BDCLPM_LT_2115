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
    public class ScheduleTests
    {
        private IWebDriver driver;
        private SchedulePage schedulePage;
        private LoginPage loginPage;
        private ExcelHelper excelHelper;

        private readonly string excelPath = @"D:\BDCLPM_LT\DOAN\TestCN.xlsx";
        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "schedules.json");


        [SetUp]
        public void SetUp()
        {
            driver = DriverFactory.InitDriver();
            schedulePage = new SchedulePage(driver);
            loginPage = new LoginPage(driver);
            excelHelper = new ExcelHelper();

            // Mở đúng Sheet quản lý Lịch thi đấu
            excelHelper.OpenExcel(excelPath, "Testcases 9 - QL Lịch thi đấu");

            // --- BƯỚC ĐĂNG NHẬP ADMIN BẮT BUỘC ---
            driver.Navigate().GoToUrl("http://localhost:5033/Account/Login");
            loginPage.Login("admin", "admin123");

            if (!loginPage.IsLoginSuccess())
            {
                Assert.Fail("Lỗi: Không thể đăng nhập tài khoản Admin.");
            }
        }

        [Test]
        public void RunScheduleTests()
        {
            var testData = JsonDataReader.ReadTestData<SchedulesData>(jsonPath);
            int currentRow = 1;

            foreach (var schedule in testData)
            {
                string status = "";
                string actualResult = "";
                bool takeScreenshot = false;

                try
                {
                    // 1. THỰC HIỆN HÀNH ĐỘNG THEO "ACTION"
                    switch (schedule.Action)
                    {
                        case "add_schedule":
                            driver.Navigate().GoToUrl("http://localhost:5033/LichThiDau/Create");
                            schedulePage.FillScheduleForm(schedule);
                            break;

                        case "edit_schedule":
                            driver.Navigate().GoToUrl($"http://localhost:5033/LichThiDau/Edit/{schedule.MaLich}");
                            schedulePage.FillScheduleForm(schedule);
                            break;

                        case "delete_schedule":
                            driver.Navigate().GoToUrl($"http://localhost:5033/LichThiDau/Delete/{schedule.MaLich}");
                            schedulePage.ConfirmDelete();
                            break;

                        case "delete_cancel":
                            driver.Navigate().GoToUrl($"http://localhost:5033/LichThiDau/Delete/{schedule.MaLich}");
                            schedulePage.CancelDelete();
                            break;

                        case "sort_id":
                            driver.Navigate().GoToUrl("http://localhost:5033/LichThiDau");
                            schedulePage.SortByColumn("Mã lịch");
                            break;

                        case "sort_date":
                            driver.Navigate().GoToUrl("http://localhost:5033/LichThiDau");
                            schedulePage.SortByColumn("Ngày thi đấu");
                            break;

                        case "pagination":
                            driver.Navigate().GoToUrl("http://localhost:5033/LichThiDau");
                            schedulePage.ClickNextPage();
                            break;

                        case "auth_check":
                            driver.Navigate().GoToUrl("http://localhost:5033/Account/Logout");
                            driver.Navigate().GoToUrl("http://localhost:5033/LichThiDau/Create");
                            break;

                        default:
                            driver.Navigate().GoToUrl("http://localhost:5033/LichThiDau");
                            break;
                    }

                    // 2. LẤY TRẠNG THÁI HỆ THỐNG
                    bool isSuccess = schedulePage.IsActionSuccess();
                    bool isError = schedulePage.IsErrorDisplayed();
                    string systemError = schedulePage.GetSystemErrorMessage();

                    // 3. XÉT 4 TRƯỜNG HỢP PASS/FAIL THEO YÊU CẦU CỦA KIỀU
                    if (schedule.Expected == true) // Kịch bản mong đợi THÀNH CÔNG
                    {
                        if (isSuccess)
                        {
                            status = "PASS";
                            actualResult = "Tạo lịch thi đấu thành công";
                            takeScreenshot = false;
                        }
                        else
                        {
                            status = "FAIL";
                            actualResult = string.IsNullOrEmpty(systemError) ? "Lỗi không xác định" : systemError;
                            takeScreenshot = true;
                        }
                    }
                    else // Kịch bản mong đợi CÓ BÁO LỖI (Expected: false)
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
                            actualResult = "Vẫn tạo lịch thi đấu được không báo lỗi";
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