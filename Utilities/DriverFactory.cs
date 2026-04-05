using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Drawing;

namespace FootballSeleniumTest.Utilities
{
    public class DriverFactory
    {
        public static IWebDriver InitDriver()
        {
            // Thêm tùy chọn để trình duyệt chạy "mượt" và sạch sẽ hơn
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--disable-notifications"); // Tắt thông báo
            options.AddArgument("--disable-infobars");      // Tắt dòng "Chrome is being controlled..."

            IWebDriver driver = new ChromeDriver(options);

            // Chỉnh màn hình về kích thước nhỏ gọn
            // Bạn có thể đổi (1024, 768) thành (800, 700) nếu muốn nhỏ hơn nữa nhé
            driver.Manage().Window.Size = new Size(800, 700);

            return driver;
        }
    }
}