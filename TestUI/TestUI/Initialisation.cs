using OpenQA.Selenium.Chrome;

namespace TestUI
{
    class Initialisation
    {
        public static ChromeDriver driver;

        public static void Initialise()
        {
            driver = new ChromeDriver();
        }

        public static void CloseBrowser()
        {
            driver.Quit();
        }
    }
}
