using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Crawler;
using PanGu;
using MongoDB.Bson;

namespace SearchServiceDemon
{
    class DocIdResult:IComparable
    {
        public ObjectId docId { get; set; }
        public double sum_tfidf { get; set; }

        public int CompareTo(object obj)
        {
            DocIdResult other = obj as DocIdResult;
            double result = sum_tfidf - other.sum_tfidf;
            if (result > 0) return -1;
            else if (result < 0) return 1;
            return 0;
        }
    }

    class DocUrlAbstractResult
    {
        public string title { get; set; }
        public string url { get; set; }
        public string abst { get; set; }
    }

    class Program
    {
        private static Dictionary<ObjectId, Dictionary<string, int>> forward_index;
        private static Dictionary<string, double> word_idf;

        static void HTTPServerStart()
        {
            using (HttpListener listerner = new HttpListener())
            {
                listerner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;//指定身份验证  Anonymous匿名访问
                listerner.Prefixes.Add("http://localhost:8081/");
                listerner.Start();
                Util.log("{0}", "WebServer Start Successed.");
                while (true)
                {
                    //等待请求连接
                    //没有请求则GetContext处于阻塞状态
                    HttpListenerContext ctx = listerner.GetContext();
                    ctx.Response.StatusCode = 200;//设置返回给客服端http状态代码
                    string query_string = ctx.Request.QueryString["query"];

                    //query_string = "上海交通";
                    if (query_string != null)
                    {
                        Util.log("query: {0}", query_string);
                    }
                    else continue;
                    string[] query = query_string.Split('+');
                    string html_code = ProcessQuery(query);

                    //使用Writer输出http响应代码
                    using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream))
                    {
                        writer.WriteLine(html_code);
                    }
                }
            }
        }

        static string ProcessQuery(string[] query)
        {
            /* 对query进行分词 */
            List<string> query_words = new List<string>();
            PanGu.Segment.Init();
            Segment seg = new Segment();
            foreach (string q in query)
            {
                ICollection<WordInfo> words = seg.DoSegment(q);
                foreach (WordInfo wi in words)
                {
                    if (!query_words.Contains(wi.Word))
                        query_words.Add(wi.Word);
                }
            }
            /* 检索出符合条件的docIds */
            MongodbAccess mongo = new MongodbAccess();
            List<ObjectId> docIds = mongo.GetDocIDByQuery(query_words);
            docIds = SortResult(docIds, query_words);
            List<DocUrlAbstractResult> result = GetResult(docIds);
            /*
            foreach (DocUrlAbstractResult duar in result)
            {
                Console.WriteLine("{0}\n\t{1}", duar.title, duar.url);
            }
             * */
            StringBuilder strbuilder = new StringBuilder();
            foreach (DocUrlAbstractResult duar in result)
            {
                strbuilder.Append(String.Format(
                    "<li><div><span><a href='{2}' target='_blank' class='link'>{0}<a></span><br/><span class='abstract'>{1}</span><br/><span class='url'>{2}</span></div></li>",
                    duar.title, duar.abst, duar.url));
            }
            if (strbuilder.Length == 0)
            {
                strbuilder.Append("No pages mathch the query.");
            }
            return "<ol>" + strbuilder.ToString() + "</ol>";
        }

        static List<DocUrlAbstractResult> GetResult(List<ObjectId> docIds)
        {
            List<DocUrlAbstractResult> ret = new List<DocUrlAbstractResult>();
            MongodbAccess mongo = new MongodbAccess();
            foreach (ObjectId docId in docIds)
            {
                WebPage page = mongo.GetWebPageById(docId);
                DocUrlAbstractResult duar = new DocUrlAbstractResult();
                duar.title = page.title;
                duar.url = page.url;
                duar.abst = GetAbstract(docId);
                ret.Add(duar);
            }
            return ret;
        }

        static string GetAbstract(ObjectId docId)
        {
            MongodbAccess mongo = new MongodbAccess();
            WebPage page = mongo.GetWebPageById(docId);
            return page.title.Length < page.content.Length ?
                page.content.Substring(page.title.Length, (page.content.Length - page.title.Length) > 150 ? 150 : (page.content.Length - page.title.Length)) : "";
        }

        static List<ObjectId> SortResult(List<ObjectId> docIds, List<string> query_words)
        {
            List<ObjectId> ret = new List<ObjectId>();
            List<DocIdResult> result = new List<DocIdResult>();
            foreach (ObjectId docId in docIds)
            {
                DocIdResult dr = new DocIdResult();
                dr.docId = docId;
                dr.sum_tfidf = GetSumOfTFIDF(docId, query_words);
                result.Add(dr);
            }
            result.Sort();
            int count = 0, limit = 200;
            foreach (DocIdResult dr in result)
            {
                ret.Add(dr.docId);
                count++;
                if (count > limit) break;
            }
            return ret;
        }

        static double GetSumOfTFIDF(ObjectId docId, List<string> query_words)
        {
            double ret = 0;
            Dictionary<string, int> forward = forward_index[docId];
            foreach (string w in query_words)
            {
                if (forward.ContainsKey(w))
                {
                    ret += forward[w] * word_idf[w];
                }
            }
            return ret;
        }

        static void ForwardIndexAndWordIDFInit()
        {
            MongodbAccess mongo = new MongodbAccess();
            forward_index = mongo.GetForwardIndex();
            word_idf = mongo.GetWordIDF();
        }

        static void Main(string[] args)
        {
            ForwardIndexAndWordIDFInit();
            HTTPServerStart();
        }
    }
}
