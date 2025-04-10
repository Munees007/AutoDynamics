using AutoDynamics.Shared.Services;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
namespace AutoDynamics.Services
{
    class WhatsAppService:IWhatsAppService
    {
        private IWebDriver driver;

        public async Task<string> StartWhatsApp()
        {
            if (driver != null)
            {
                return "WhatsApp is already running in the background...";
            }

            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("user-data-dir=C:\\WhatsAppSession"); // Saves login session
                options.AddArgument("disable-gpu"); // Disables GPU rendering
                options.AddArgument("no-sandbox"); // Bypasses sandbox security
                options.AddArgument("disable-dev-shm-usage"); // Optimizes memory usage
                options.AddArgument("window-size=1920,1080"); // Ensures proper window size
                options.AddArgument("start-minimized"); // Opens Chrome minimized

                driver = new ChromeDriver(options);
                driver.Navigate().GoToUrl("https://web.whatsapp.com");

                await Task.Delay(10000); // Wait for WhatsApp Web to load

                return "WhatsApp Web started in the background.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<bool> SendMessage(string phoneNumber, string message)
        {
            if (driver == null)
            {
                string startStatus = await StartWhatsApp();
                if (driver == null) return false;
            }

            try
            {
                driver.Navigate().GoToUrl($"https://web.whatsapp.com/send?phone={phoneNumber}&text={Uri.EscapeDataString(message)}");
                await Task.Delay(5000); // Wait for chat to load

                // Simulate pressing ENTER to send message
                Actions actions = new Actions(driver);
                actions.SendKeys(Keys.Enter).Perform();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<string> StopWhatsApp()
        {
            if (driver != null)
            {
                driver.Quit();
                driver = null;
                return "WhatsApp Web closed.";
            }
            return "WhatsApp is not running.";
        }


    }
}
