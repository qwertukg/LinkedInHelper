using System;
using System.Linq;
using LinkedInHelper.Db;
using OpenQA.Selenium.Chrome;

namespace LinkedInHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("LinkedIn.com helper\n");

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            Console.Write("How many contacts you want to add: ");
            var count = Convert.ToInt16(Console.ReadLine());

            Console.WriteLine("\nJob started...");

            using (var driver = new ChromeDriver())
            {
                using (var context = new Context())
                {
                    var crawler = new Crawler(driver, context);

                    Console.WriteLine();
                    crawler.Login(email, password);
                    crawler.AddFriendsFromOffers(count);
                    crawler.AddFriendsByWalk(count);
                }
            }

            Console.WriteLine("Job done! Press any key to exit...");
            Console.ReadKey();
        }
    }
}
