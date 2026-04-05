using OpenQA.Selenium;
using System;
using System.IO;

namespace FootballSeleniumTest.Utilities
{
    public class ScreenshotHelper
    {
        // THAY ĐỔI TẠI ĐÂY: Kiều hãy tạo một thư mục Screenshots cố định trong thư mục đồ án ở ổ D
        private static readonly string FolderPath = @"D:\BDCLPM_LT\DOAN\Screenshots";

        public static void ClearOldScreenshots()
        {
            // LƯU Ý: Nếu Kiều muốn giữ lại hình của những ngày trước, hãy TẮT (Comment) dòng này trong RoundTests.cs
            try
            {
                if (Directory.Exists(FolderPath))
                {
                    DirectoryInfo di = new DirectoryInfo(FolderPath);
                    foreach (FileInfo file in di.GetFiles()) { file.Delete(); }
                    Console.WriteLine("--- Đã dọn dẹp thư mục Screenshots ---");
                }
                else
                {
                    Directory.CreateDirectory(FolderPath);
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi dọn ảnh: " + ex.Message); }
        }

        public static string Capture(IWebDriver driver)
        {
            try
            {
                if (driver == null) return "N/A (Driver Null)";

                if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

                // Tên file có ngày giờ để không bao giờ bị trùng
                string fileName = "shot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png";
                string fullPath = Path.Combine(FolderPath, fileName);

                Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
                ss.SaveAsFile(fullPath);

                // Trả về đường dẫn Windows chuẩn (D:\...) thay vì định dạng file:///
                return fullPath;
            }
            catch (Exception ex) { return "Lỗi chụp ảnh: " + ex.Message; }
        }
    }
}