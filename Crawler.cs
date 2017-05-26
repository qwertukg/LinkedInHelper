using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkedInHelper.Db;
using LinkedInHelper.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace LinkedInHelper
{
    class Crawler
    {
        private const int Timeout = 10;
        private int _i = 1;

        private readonly Context _context;
        private readonly ChromeDriver _driver;

        public Crawler(ChromeDriver driver, Context context)
        {
            _context = context;
            _driver = driver;
        }

        public void Login(string email, string password)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Timeout));

            _driver.Navigate().GoToUrl("https://www.linkedin.com/uas/login");

            _driver.FindElement(By.Id("session_key-login")).SendKeys(email);
            _driver.FindElement(By.Id("session_password-login")).SendKeys(password);
            _driver.FindElement(By.Id("btn-primary")).Click();

            // wait for element loading
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("mynetwork-tab-icon")));
        }

        public void AddFriendsFromOffers(int count)
        {
            
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Timeout));

            while (_i <= count)
            {
                // reload when list is empty
                if (_driver.FindElements(By.ClassName("mn-pymk-list__card")).Count <= 1)
                {
                    Console.WriteLine(DateTime.Now + " | Offers scanning...");
                    _driver.Navigate().GoToUrl("https://www.linkedin.com/mynetwork/");
                }

                try
                {
                    // wait for element loading
                    wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("mn-pymk-list__card")));
                }
                catch (Exception e)
                {
                    return;
                }

                var card = _driver.FindElement(By.ClassName("mn-pymk-list__card"));
                var cardId = card.FindElement(By.ClassName("mn-person-info")).GetAttribute("id");

                var friend = new Friend
                {
                    PageId = card.FindElement(By.ClassName("mn-person-info__link")).GetAttribute("href"),
                    Name = card.FindElement(By.ClassName("mn-person-info__name")).Text,
                    Position = card.FindElement(By.ClassName("mn-person-info__occupation")).Text,
                    IsMyFriend = false,
                    ConnectImpossible = false,
                    HisVisitorsInvited = false,
                    HisFriendsInvited = false,
                };

                if (!_context.Friends.Any(f => f.PageId == friend.PageId))
                {
                    // send request
                    card.FindElement(By.ClassName("button-secondary-small")).Click();

                    _context.Friends.Add(friend);
                    _context.SaveChanges();

                    Console.Write("\t" + friend.Name);
                    PrintRequestResults(friend, _i);

                    _i++;
                }
                else
                {
                    // close
                    card.FindElement(By.ClassName("mn-pymk-list__close-btn")).Click();
                }

                // waiting for unload element
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id(cardId)));
            }
        }

        public void AddFriendsByWalk(int count)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(Timeout));

            foreach (var myFriend in _context.Friends.Where(f => f.HisVisitorsInvited == false).ToList())
            {
                if (_i > count)
                {
                    return;
                }

                _driver.Navigate().GoToUrl(myFriend.PageId);
                Console.WriteLine(DateTime.Now + " | " + myFriend.Name + " visitors scanning...");

                var linkedFriends = new List<Friend>();

                // find linked friends
                foreach (var linkedFriendElement in _driver.FindElements(By.ClassName("pv-browsemap-section__member-container")))
                {
                    linkedFriends.Add(new Friend
                    {
                        PageId = linkedFriendElement.FindElement(By.ClassName("pv-browsemap-section__member")).GetAttribute("href"),
                        Name = linkedFriendElement.FindElement(By.ClassName("actor-name")).Text,
                        Position = linkedFriendElement.FindElement(By.ClassName("browsemap-headline")).Text,
                        IsMyFriend = false,
                        ConnectImpossible = false,
                        HisVisitorsInvited = false,
                        HisFriendsInvited = false,
                    });
                }

                // send request to linked friends
                foreach (var linkedFriend in linkedFriends)
                {
                    // only for new persons
                    if (!_context.Friends.Any(f => f.PageId == linkedFriend.PageId && f.ConnectImpossible == false && f.IsMyFriend == false))
                    {
                        _driver.Navigate().GoToUrl(linkedFriend.PageId);
                        Console.Write("\t" + linkedFriend.Name);

                        // only if button available
                        if (_driver.FindElements(By.ClassName("connect")).Count != 0)
                        {
                            _driver.FindElement(By.ClassName("connect")).Click();

                            // wait for element loading
                            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("button-primary-large")));

                            _driver.FindElement(By.ClassName("button-primary-large")).Click();

                            PrintRequestResults(linkedFriend, _i);

                            _i++;
                        }
                        else if (_driver.FindElements(By.ClassName("message")).Count != 0)
                        {
                            // mark confirmed friend
                            linkedFriend.IsMyFriend = true;
                            Console.WriteLine(" -> my friend!");
                        }
                        else
                        {
                            // can't send friend request
                            linkedFriend.ConnectImpossible = true;
                            Console.WriteLine(" -> connect impossible!");
                        }

                        _context.Friends.Add(linkedFriend);
                        _context.SaveChanges();
                    }
                }

                var currentFriend = _context.Friends.FirstOrDefault(f => f.Id == myFriend.Id);

                if (currentFriend != null)
                {
                    // current friend visitors invited flag set
                    currentFriend.HisVisitorsInvited = true;
                    _context.SaveChanges();
                }
            }
        }

        private void PrintRequestResults(Friend friend, int currentRequestNumber)
        {
            var totalInfo = "[" + currentRequestNumber + "-" + _context.Friends.Count() + "]";

            Console.WriteLine(" | " + friend.Position + " -> request sent! " + totalInfo);
        }
    }
}
