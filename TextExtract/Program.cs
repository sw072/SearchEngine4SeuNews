using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Crawler;

namespace TextExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            MongodbAccess mongo = new MongodbAccess();
            HashSet<string> crawled_urls = mongo.GetCrawledURLs();
            int count = 0;
            foreach(string url in crawled_urls)
            {
                WebPage page = mongo.GetWebPageByURL(url);
                if (page == null) continue;
                TextExtract te = new TextExtract(page.html, true);
                mongo.SetWebPageContent(url, page.title + te.content);
                count++;
                Util.log("{0} {1}", count, url);
            }
            Util.log("processed {0} urls.", count);
        }
    }
}
