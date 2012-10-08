using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Crawler
{
    class Program
    {
        public static string domain = "http://news.seu.edu.cn";
        public static string[] seed_category_urls_fmt = { "http://news.seu.edu.cn/s/146/t/1399/p/1/c/6595/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/1/c/2100/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/1/c/6767/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/1/c/2103/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/1/c/2108/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/1/c/2145/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/6553/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/6552/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/2102/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/6554/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/6555/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/6550/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/2148/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/6551/i/{0}/list.htm",
                                                        "http://news.seu.edu.cn/s/146/t/1399/p/2/c/6548/i/{0}/list.htm"};
        public static int[] seed_page_count = { 2, 161, 15, 104, 10, 6, 3, 4, 8, 3, 3, 2, 4, 5, 4 };

        static string GetWebHTML(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                return sr.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        static HashSet<string> GetNewsURLsFromHTML(string html)
        {
            HashSet<string> ret = new HashSet<string>();
            string url;
            string regex = @"<tr><td><a href='(?<url>[:\\/.\w\d]+)' target='_blank' style";
            Regex re = new Regex(regex);
            MatchCollection mc = re.Matches(html);
            foreach (Match m in mc)
            {
                url = m.Groups["url"].Value;
                if (!url.Contains("info")) continue;
                if (url.StartsWith("/"))
                {
                    url = domain + url;
                }
                ret.Add(url);
            }
            return ret;
        }

        static HashSet<string> GetURLsByCategory(int idx)
        {
            HashSet<string> ret = new HashSet<string>();
            string url, html = "";   //GetWebHTML(seed_category_urls[idx]);
            int page_count = seed_page_count[idx];
            //get urls of each page
            for (int page_idx = 0; page_idx < page_count; page_idx++)
            {
                url = String.Format(seed_category_urls_fmt[idx], page_idx + 1);
                html = GetWebHTML(url);
                if (html == null) continue;
                ret.UnionWith(GetNewsURLsFromHTML(html));
            }
            return ret;
        }

        static HashSet<string> GetWaitingURLs(HashSet<string> crawled_urls)
        {
            HashSet<string> ret = new HashSet<string>();
            for(int idx = 0; idx < seed_category_urls_fmt.Length; idx++)
            {
                ret.UnionWith(GetURLsByCategory(idx));
            }
            ret.ExceptWith(crawled_urls);
            return ret;
        }

        static WebPage GetWebPage(string url)
        {
            string html = GetWebHTML(url);
            if (html == null) return null;
            int title_start_pos = html.IndexOf("<title>") + "<title>".Length;
            int title_end_pos = html.IndexOf("</title>");
            int pub_time_start_pos = html.IndexOf(">发布时间:") + ">发布时间:".Length;
            //int content_start_pos = html.IndexOf("<font size='3'>") + "<td valign='top'><div><font size='3'>".Length;
            //int content_end_pos = html.IndexOf("</font></div></td>");
            WebPage page = new WebPage();
            page.url = url;
            page.html = html;
            page.title = html.Substring(title_start_pos, title_end_pos - title_start_pos);
            try
            {
                page.published_time = DateTime.Parse(html.Substring(pub_time_start_pos, 10));
            }
            catch
            {
                //Console.WriteLine("{0} {1}", url, html.Substring(pub_time_start_pos, 10));
                page.published_time = DateTime.Now;
            }
            page.content = "";
            return page;
        }

        static void Main(string[] args)
        {
            MongodbAccess mongo = new MongodbAccess();
            HashSet<string> crawled_urls = mongo.GetCrawledURLs();
            Util.log("{0} urls have been crawled.", crawled_urls.Count);

            Random r = new Random(99);

            HashSet<string> urls = GetWaitingURLs(crawled_urls);
            foreach (string url in urls)
            {
                WebPage page = GetWebPage(url);
                if (page == null) continue;
                mongo.InsertWebPage(page);
                Util.log("crawl {0} done.", url);
                if (r.Next(100) > 80)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }

            /*
            HashSet<string> urls = GetWaitingURLs(null);
            Console.WriteLine(urls.Count);
            foreach (string url in urls)
            {
                WebPage page = GetWebPage(url);
                if (page == null) continue;
                Console.WriteLine("{0} {1}", page.title, page.published_time);
            }
             * */
        }
    }
}
