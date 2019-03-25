using System;
using System.Collections.Generic;

namespace ForumScraper
{
    public class Post
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public string PostId { get; set; }
        public string DateText { get; set; }
        public IEnumerable<string> ImageLinks { get; set; }


        public override string ToString()
        {
            return Username;
        }
    }
}
