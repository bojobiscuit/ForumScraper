using System;
using HtmlAgilityPack;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ForumScraper
{
    class Program
    {
        private static string url;

        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            var pages = new List<Page>();
            var posts = new List<Post>();

            // Online Pages
            url = "http://simulationhockey.com/showthread.php?tid=94401";
            pages = GetPagesOnline(url);

            // Local Files (testing)
            // pages.Add(GetLocalPage("localFiles/ac254-1.html"));
            // pages.Add(GetLocalPage("localFiles/ac254-2.html"));
            // pages.Add(GetLocalPage("localFiles/ac254-3.html"));

            foreach (var page in pages)
                foreach (var postDiv in page.PostDivs)
                    posts.Add(GetPost(postDiv));

            // Remove the OP, sort by name.
            posts = posts.Skip(1).OrderBy(a => a.Username).ToList();

            CreateOutput(posts);
        }

        private static List<Page> GetPagesOnline(string url)
        {
            List<Page> pages = new List<Page>();
            int pageNumber = 1;
            string initialAnchor = null;
            while (true)
            {
                if (pageNumber > 1)
                    url += "&page=" + pageNumber;
                var htmlDocument = new HtmlWeb().Load(url);
                var page = new Page(htmlDocument);
                var anchor = page.GetInitialAnchor();

                // When you get to a page after the limit, it resets to the first page in the thread.
                // We check the first anchor link containing a post id to see if it matches the first page.
                if (initialAnchor == anchor)
                    break;

                if (initialAnchor == null)
                    initialAnchor = anchor;

                pages.Add(page);

                Console.WriteLine("Got Page " + pageNumber);
                pageNumber++;
            }

            return pages;
        }

        private static void CreateOutput(List<Post> posts)
        {
            Console.WriteLine();
            Console.Write("Writing output...");
            string filePath = Path.Combine(Environment.CurrentDirectory, "outputFormat.html");
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(filePath);
            var table = htmlDocument.DocumentNode.Descendants().First(a => a.Id == "outputTable");

            foreach (var post in posts)
            {
                HtmlNode tableRow = HtmlNode.CreateNode("<tr></tr>");
                table.ChildNodes.Add(tableRow);

                bool isMultiple = posts.Count(a => a.Username == post.Username) > 1;

                HtmlNode tableCell = HtmlNode.CreateNode("<td></td>");
                tableCell.InnerHtml = post.Username;
                tableRow.ChildNodes.Add(tableCell);
                if (isMultiple)
                    tableCell.AddClass("yellow");

                tableCell = HtmlNode.CreateNode("<td></td>");
                tableCell.InnerHtml = WordCount(post.Message).ToString();
                tableRow.ChildNodes.Add(tableCell);

                tableCell = HtmlNode.CreateNode("<td></td>");
                foreach (var imageLink in post.ImageLinks)
                    tableCell.InnerHtml += "<a href=\"" + imageLink + "\" target=\"_blank\"><img src=\"" + imageLink + "\" /></a><br />";
                tableCell.InnerHtml += post.Message;
                tableRow.ChildNodes.Add(tableCell);
            }

            var threadLink = htmlDocument.DocumentNode.Descendants().First(a => a.Id == "threadLink");
            threadLink.InnerHtml = url;
            threadLink.SetAttributeValue("href", url);

            var scanDate = htmlDocument.DocumentNode.Descendants().First(a => a.Id == "scanDate");
            scanDate.InnerHtml = DateTime.Now.ToString() + " " + TimeZoneInfo.Local.ToString();

            Console.Write(" Done");
            Console.WriteLine();

            htmlDocument.Save("output/threadData.html");
        }

        private static int WordCount(string whole_text)
        {
            char[] delimiters = new char[] { ' ', '\r', '\n' };
            var splits = whole_text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            var cleanSplits = splits.Where(a => Regex.IsMatch(a, ".*[a-zA-Z].*")).Select(a => a.Trim());
            return cleanSplits.Count();
        }

        private static Page GetLocalPage(string fileName)
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);
            var htmlDocument = new HtmlDocument();
            htmlDocument.Load(filePath);
            return new Page(htmlDocument);
        }

        private static Post GetPost(HtmlNode postDiv)
        {
            var post = new Post();

            var postLeft = postDiv.Descendants().First(a => a.Id == "one");
            var postRight = postDiv.Descendants().First(a => a.Id == "two");
            var profileArea = postLeft.Descendants("span").First(a => a.HasClass("profile-username"));
            var nameArea = profileArea.Descendants("a").First();
            var messageArea = postRight.Descendants().First(a => a.HasClass("post_body"));
            var signature = messageArea.Descendants("div").FirstOrDefault(a => a.HasClass("hide"));

            // removing signatures from post
            if (signature != null)
                signature.InnerHtml = "";

            var imageLinks = messageArea.Descendants("img").Select(a => a.GetAttributeValue("src", "none"));

            post.Username = nameArea.InnerText.Trim();
            post.Message = messageArea.InnerText.Trim();
            post.Message = RemoveImagePlaceholders(post.Message);
            post.ImageLinks = imageLinks.ToList();

            return post;
        }

        private static string RemoveImagePlaceholders(string message)
        {
            int startIndex = message.IndexOf("[Image: ");
            while (startIndex >= 0)
            {
                int endIndex = message.IndexOf("]", startIndex);
                var imagePlaceHolder = message.Substring(startIndex, endIndex - startIndex + 1);
                message = message.Replace(imagePlaceHolder, "");
                startIndex = message.IndexOf("[Image: ");
            }
            return message;
        }
    }
}