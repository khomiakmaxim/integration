using DTO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace SiteParser
{
    class Program
    {        
        public static IEnumerable<NewsDTO> Crawl()
        {            
            string homeUrl = "https://www.pravda.com.ua/news/";

            //options skip errors
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddArgument("--ignore-certificate-errors-spki-list");
            chromeOptions.AddArgument("--ignore-ssl-errors");
            chromeOptions.AddArgument("test-type");
            chromeOptions.AddArgument("no-sandbox");
            chromeOptions.AddArgument("-incognito");
            chromeOptions.AddArgument("--start-maximized");


            IWebDriver driver = new ChromeDriver(@"C:\Users\khomiak\Documents\Integration\Integration\chromedriver_win32", chromeOptions);
            driver.Navigate().GoToUrl(homeUrl);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);


            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
            // wait.Until(d => d.FindElement(By.CssSelector(".headline__text")));

            ////get all news
            //var elements = driver.FindElements(By.CssSelector(".article_header"));

            //var test = elements.First();

            //List<NewsDTO> news = elements
            //.Select(el => new NewsDTO
            //{
            //    Title = el.Text,
            //    Url = el.GetAttribute("href")                
            //}).ToList();

            // get all news
            var elements = driver.FindElements(By.CssSelector(".article_header"));

            List<NewsDTO> news = new List<NewsDTO>();

            foreach (var el in elements)
            {
                var linkElement = el.FindElement(By.TagName("a"));
                var newsDto = new NewsDTO
                {
                    Title = linkElement.Text,
                    Url = linkElement.GetAttribute("href")
                };
                news.Add(newsDto);
            }


            for (int i = 0; i < news.Count; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));

                var n = news[i];
                try
                {
                    driver.Navigate().GoToUrl(n.Url);

                    // wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".pg-headline")));

                    n.ID = (i + 1).ToString();
                    n.Author = driver.FindElement(By.CssSelector(".post_author")).Text;                   
                }
                catch (Exception) { }

                yield return n;
            }

            driver.Close();
        }
        
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = ConnectionFactory.DefaultUser;
            factory.Password = ConnectionFactory.DefaultPass;
            factory.VirtualHost = ConnectionFactory.DefaultVHost;
            factory.HostName = "localhost";
            factory.Port = AmqpTcpEndpoint.UseDefaultPort;


            using (IConnection conn = factory.CreateConnection())
            using (var model = conn.CreateModel())
            {
                model.QueueDeclare("news", false, false, false, null);
                using (StreamWriter sw = new StreamWriter("text.txt", false, System.Text.Encoding.Default))
                {
                    foreach (var x in Crawl())
                    {
                        //sw.WriteLine(x.GetStr());
                        Console.WriteLine(x);
                        var properties = model.CreateBasicProperties();
                        properties.Persistent = true;
                        model.BasicPublish(string.Empty, "news", basicProperties: properties, body: BinaryConverter.ObjectToByteArray(x));
                    }
                }
            }
        }
    }
}