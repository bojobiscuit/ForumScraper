using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ForumScraper
{
    public class Page
    {

        public Page(HtmlDocument htmlDocument)
        {
            _htmlDocument = htmlDocument;
            _postArea = _htmlDocument.DocumentNode.Descendants().First(a => a.Id == "posts");
            PostDivs = _postArea.Descendants().Where(a => a.HasClass("posts2"));
        }

        public string GetInitialAnchor()
        {
            return _postArea.Descendants("a").First().Id;
        }

        public IEnumerable<HtmlNode> PostDivs { get; private set; }

        private HtmlDocument _htmlDocument;
        private HtmlNode _postArea;
    }
}
