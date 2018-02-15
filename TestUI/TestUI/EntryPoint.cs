using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TestUI;

// you should always put your classes inside namespace
namespace Inputer
{

    // why inherit from initialisation class?
    // inheritance is tricky and is better to be limited to only where definitely needed.
    // here - you do not need it. What you need is composition - create class that will handle
    // integration of your app with selenium's driver.
    // Checkout how IDisposable may be of use here - you may want to implement it on this new driver-using class
    // to make sure browser is closed correctly once you no longer plan to use it.
    // It also makes sense since drivers are themselves dispisable! So once you are done using them, you should call Dispose() on them
    // or enclose their usage inside using() { } clause
    class EntryPoint : Initialisation
    {
        static IWebElement element;
        static string cssSelector;
        static string url;
        static List<string> list;

        static void Main()
        {
            Initialise();
            GetUrl();
            NavigateToUrl(url);
            // since EnterValue is method with no parameters, and RepeatAction expects parameterless action
            // too - you can skip creating lambda and simplify the call to just
            // RepeatAction(EnterValue, "all process");
            RepeatAction(() => EnterValue(), "all process");
            CloseBrowser();
        }

        // I like to keep my methods, where it makes sense, in the same order they are called.
        // Here you first call getUrl() and only then NavigatetoUrl() - reordering them makes it easier 
        // to find methods in file
        private static void GetUrl()
        {
            url = ReadAnwser("\n\nEnter URL: https://www.google.com/");

            // so remove it! Why not use entry parameters on main method?
            // (string[] args) - you could pass parameters like site to test there
            // then you can either start app from console
            // or go to project properties, and in Debug tab setup startup parameters
            // for when you debug code from visual studio!
            url = "https://www.google.com";  //TODO: to remove
        }

        // this navigation etc. parts could be moved to separate class that would handle actual business logic
        // keeping everything in the same file is fine for now, when project is tiny, but will be problematic
        // once things are starting to grow
        // splitting code where one part gathers user input and config, and other part covers actual business
        // logic is helpful in long term
        private static void NavigateToUrl(string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        private static void EnterValue()
        {
            try
            {
                // EnterValue method is meant to be used as action to be repeated multiple times
                // why is this method responsible for getting config data like input css selector?
                // This data should probably be gathered before you start repeating actions and entering values into page?

                // like before - this can be simplified to: RepeatAction(GetInputCss, "adding another intput")
                list = RepeatAction(() => GetInputCss(), "adding another input");
                cssSelector = GetButtonCss();

                // file name could probably come from input parameters of application too, but I get it - for now it's ok ;)
                foreach (string line in File.ReadAllLines("TestCases.txt"))
                {
                    // string split is tricky in that it may return some empty values.
                    // do you want those? If yes - that's cool. But be aware that some things may go wrong. Check those samples:
                    //"a,b,c,".Split(',').Count()
                    //4
                    //"a,b,c,".Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Count()
                    //3
                    // and see the difference in elements that came out of split
                    string[] values = line.Split(',');
                    foreach (string value in values)
                    {

                        foreach (string item in list)
                        {
                            FillInInput(item, value);
                        }
                        ClickButton(cssSelector);
                    }
                }
            }
            catch (NotFoundException)
            {
                // great that you are handling exceptions like that.
                // but you know what would be really useful? If you would log what was not found.
                // What were you looking for and it was not there? That's what you will want to know
                // when trying to fix issues with application (or users of application want to know when something goes wrong)
                ColourMessage(ConsoleColor.Red, "Oops! Something went wrong");
            }
        }

        // it is not getting input css - it is getting input's selector!
        // also - GetXxx suggests that value will be returned from function.
        // in this case it is not returning it but adding to list. Maybe name should be amended?
        // This is not the best pattern anyway to gather values this way, see other comments on global variable
        private static void GetInputCss()
        {
            cssSelector = ReadAnwser("Enter input's CSS selector - #lst-ib:");
            cssSelector = "#lst-ib"; // TODO: to remove
            list.Add(cssSelector);
        }

        // it is not getting button css - it is getting button's selector!
        private static string GetButtonCss()
        {
            cssSelector = ReadAnwser("Enter button's CSS selector: .lsbb>input");
            cssSelector = ".lsbb>input"; //TODO: to remove

            return cssSelector;
        }

        // you call this methdo ClickButton but- it does not click button. At least that's not all this method does.
        // what it does is: 1) click button/perform test action, and 2) resets page to default value if I understand
        // this correctly (for next test to execute).
        // First of all - this probably should not be in the same method - two different actions, not connected to each other
        // second - navigation/reseting test scene should probably be more "official" step, not something hidden inside ClickButton
        private static void ClickButton(string btnCss)
        {
            element = driver.FindElement(By.CssSelector(btnCss));

            if (element.Displayed)
            {
                element.Click();
                NavigateToUrl(url);

                WaitForAction();
            }
            else
            {
                // message should be more like
                // Element '{btncCss}' not found
                ColourMessage(ConsoleColor.Red, $"Not found '{btnCss}' element");
            }
        }

        // tbxCss - this is not text box css - this is inputSelector
        private static void FillInInput(string tbxCss, string value)
        {
            element = driver.FindElement(By.CssSelector(tbxCss));

            // and if it is not displayed? Wait for it? Throw exception? Write message?
            if (element.Displayed)
            {
                element.SendKeys(value);
                ColourMessage(ConsoleColor.Green, $"Entered: {value}");

                WaitForAction();
            }
        }

        private static List<string> RepeatAction(Action action, string actionName)
        {
            // this list is created, but nothing is ever inserted into it.
            // Oh, wait - this is global variable! That's not pretty.
            // So what you want to do is reset this variable every time you run your loop again?
            // That's probably something that action itself should do. After all not all actions you pass
            // to this method use this list right? So why reseting list when you may want to actually re-use
            // values here? This may make it impossible to do some things!
            // Also - global state is tricky to handle. This could be embedded in separate class maybe - but not sure.
            list = new List<string>();

            // instead of while(true) you could use do {} while() construct here - and check if loop should continue there
            // not use break. Would maybe be a bit cleaner
            while (true)
            {
                action();
                var repeat = ReadAnwser($"Repeat {actionName} ? Enter yes or no", ConsoleColor.Yellow).ToLower();
                if (repeat != "y" && repeat != "yes")
                    break;
            }

            return list;
        }

        // wait should probably be more complicated than just thread.sleep right?
        // you can either check if elements are already on page or something.
        // I assume this was done just for simplicity.
        public static void WaitForAction()
        {
            Thread.Sleep(1000);
        }

        private static string ReadAnwser(string message, ConsoleColor colour = ConsoleColor.Gray)
        {
            ColourMessage(colour, message);
            return Console.ReadLine();
        }

        private static void ColourMessage(ConsoleColor colour, string message)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}