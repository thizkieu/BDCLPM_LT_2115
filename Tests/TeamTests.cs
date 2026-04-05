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
    public class TeamTests
    {
        private IWebDriver driver;
        private TeamPage teamPage;
        private LoginPage loginPage;
        private ExcelHelper excelHelper;

        private readonly string excelPath = @"D:\BDCLPM_LT\DOAN\TestCN.xlsx";
        private readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "team.json");


        [SetUp]
        public void SetUp()
        {
            driver = DriverFactory.InitDriver();
            teamPage = new TeamPage(driver);
            loginPage = new LoginPage(driver);
            excelHelper = new ExcelHelper();

            // Mở đúng Sheet quản lý Đội bóng
            excelHelper.OpenExcel(excelPath, "Testcases 4 -QL Đội bóng");

            // --- BƯỚC ĐĂNG NHẬP ADMIN ---
            driver.Navigate().GoToUrl("http://localhost:5033/Account/Login");
            loginPage.Login("admin", "admin123");

            if (!loginPage.IsLoginSuccess())
            {
                Assert.Fail("Lỗi: Không thể đăng nhập tài khoản Admin.");
            }
        }

        [Test]
        public void RunTeamTests()
        {
            var testData = JsonDataReader.ReadTestData<TeamData>(jsonPath);
            int currentRow = 1;

            foreach (var team in testData)
            {
                string status = "";
                string actualResult = "";
                bool takeScreenshot = false;

                try
                {
                    // 1. Chuẩn bị đường dẫn logo
                    string fullLogoPath = string.IsNullOrEmpty(team.logo) ? "" :
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", team.logo);

                    // 2. THỰC HIỆN HÀNH ĐỘNG THEO "ACTION"
                    switch (team.action)
                    {
                        case "add_team":
                            driver.Navigate().GoToUrl("http://localhost:5033/DoiBong/Create");
                            teamPage.FillTeamForm(team.teamId, team.teamName, team.school, team.coach, team.players, fullLogoPath);
                            break;

                        case "edit_team":
                            driver.Navigate().GoToUrl($"http://localhost:5033/DoiBong/Edit/{team.teamId}");
                            teamPage.FillTeamForm(team.teamId, team.teamName, team.school, team.coach, team.players, fullLogoPath);
                            break;

                        case "delete_team":
                        case "delete_linked":
                        case "delete_joined":
                            driver.Navigate().GoToUrl($"http://localhost:5033/DoiBong/Delete/{team.teamId}");
                            teamPage.ConfirmDelete();
                            break;

                        case "delete_cancel":
                            driver.Navigate().GoToUrl($"http://localhost:5033/DoiBong/Delete/{team.teamId}");
                            teamPage.CancelDelete();
                            break;

                        case "sort_list":
                            driver.Navigate().GoToUrl("http://localhost:5033/DoiBong");
                            teamPage.ClickSortByName();
                            break;

                        case "pagination":
                            driver.Navigate().GoToUrl("http://localhost:5033/DoiBong");
                            teamPage.ClickPagination("2");
                            break;

                        default:
                            driver.Navigate().GoToUrl("http://localhost:5033/DoiBong");
                            break;
                    }

                    // 3. LẤY TRẠNG THÁI HỆ THỐNG
                    bool isSuccess = teamPage.IsActionSuccess();
                    bool isError = teamPage.IsErrorDisplayed();
                    string systemError = teamPage.GetSystemErrorMessage();

                    // 4. XÉT 4 TRƯỜNG HỢP PASS/FAIL THEO YÊU CẦU CỦA KIỀU
                    if (team.expected == true) // Kịch bản mong đợi THÀNH CÔNG
                    {
                        if (isSuccess)
                        {
                            // [PASS TRƯỜNG HỢP 1]: Thành công mỹ mãn
                            status = "PASS";
                            actualResult = "Tạo đội bóng thành công"; // Chuẩn hóa văn bản theo yêu cầu của Kiều
                            takeScreenshot = false;
                        }
                        else
                        {
                            // [FAIL TRƯỜNG HỢP 2]: Có lỗi hệ thống (Crash, 404...)
                            status = "FAIL";
                            actualResult = string.IsNullOrEmpty(systemError) ? "Lỗi không xác định" : systemError;
                            takeScreenshot = true;
                        }
                    }
                    else // Kịch bản mong đợi CÓ BÁO LỖI (expected: false)
                    {
                        if (isError && !isSuccess)
                        {
                            // [PASS TRƯỜNG HỢP 2]: Báo lỗi đúng ý đồ test case
                            status = "PASS";
                            actualResult = "Hiển thị lỗi từ hệ thống";
                            takeScreenshot = true;
                        }
                        else if (isSuccess)
                        {
                            // [FAIL TRƯỜNG HỢP 1]: Hệ thống không chặn được dữ liệu sai
                            status = "FAIL";
                            actualResult = "Vẫn tạo đội bóng được không báo lỗi"; // Chuẩn hóa văn bản theo yêu cầu của Kiều
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
                    actualResult = "Lỗi ngoại lệ kỹ thuật: " + ex.Message;
                    takeScreenshot = true;
                }
                finally
                {
                    // Lấy đường dẫn ảnh thô từ ổ D (D:\BDCLPM_LT\DOAN\Screenshots\...)
                    string shotPath = takeScreenshot ? ScreenshotHelper.Capture(driver) : "";

                    // Ghi vào Excel đúng các cột K, L, M
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