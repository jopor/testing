using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TestUI;

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
        RepeatAction(() => EnterValue(), "all process");
        CloseBrowser();
    }

    private static void NavigateToUrl(string url)
    {
        driver.Navigate().GoToUrl(url);
    }

    private static void GetUrl()
    {
        url = ReadAnwser("\n\nEnter URL: https://www.google.com/");
        url = "https://www.google.com";  //TODO: to remove
    }

    private static string GetButtonCss()
    {
        cssSelector = ReadAnwser("Enter button's CSS selector: .lsbb>input");
        cssSelector = ".lsbb>input"; //TODO: to remove

        return cssSelector;
    }

    private static void GetInputCss()
    {
        cssSelector = ReadAnwser("Enter input's CSS selector - #lst-ib:");
        cssSelector = "#lst-ib"; // TODO: to remove
        list.Add(cssSelector);
    }

    private static void EnterValue()
    {
        try
        {
            list = RepeatAction(() => GetInputCss(), "adding another input");
            cssSelector = GetButtonCss();

            foreach (string line in File.ReadAllLines("TestCases.txt"))
            {
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
            ColourMessage(ConsoleColor.Red, "Oops! Something went wrong");
        }
    }

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
            ColourMessage(ConsoleColor.Red, $"Not found '{btnCss}' element");
        }
    }

    private static void FillInInput(string tbxCss, string value)
    {
        element = driver.FindElement(By.CssSelector(tbxCss));
        if (element.Displayed)
        {
            element.SendKeys(value);
            ColourMessage(ConsoleColor.Green, $"Entered: {value}");

            WaitForAction();
        }
    }

    private static List<string> RepeatAction(Action action, string actionName)
    {
        list = new List<string>();

        while (true)
        {
            action();
            var repeat = ReadAnwser($"Repeat {actionName} ? Enter yes or no", ConsoleColor.Yellow).ToLower();
            if (repeat != "y" && repeat != "yes")
                break;
        }

        return list;
    }

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

