using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PanGu;
using Crawler;
using MongoDB.Bson;

namespace Analysis
{
    class Program
    {
        static Dictionary<string, int> ContetnWordSegment(string content)
        {
            Dictionary<string, int> ret = new Dictionary<string, int>();
            Segment seg = new Segment();
            ICollection<WordInfo> words = seg.DoSegment(content);
            foreach (WordInfo w in words)
            {
                if (!ret.ContainsKey(w.Word))
                {
                    ret[w.Word] = 1;
                }
                else ret[w.Word]++;
            }
            return ret;
        }

        static bool _check(string w)
        {
            if (w.Contains('.') || w.Contains('\\')
                || w.Contains('|') || w.Contains('/')
                || w.Contains('_') || w.Contains('$'))
                return false;
            return true;
        }

        static void CreateForwordIndex()
        {
            PanGu.Segment.Init();
            MongodbAccess mongo = new MongodbAccess();
            HashSet<string> words = new HashSet<string>();
            HashSet<string> crawled_urls = mongo.GetCrawledURLs();
            foreach (string url in crawled_urls)
            {
                WebPage page = mongo.GetWebPageByURL(url);
                if (page == null) continue;
                Dictionary<string, int> dict = ContetnWordSegment(page.content);
                ForwardIndexItem forwarditem = new ForwardIndexItem();
                forwarditem.webpage_id = page._id;
                forwarditem.words = new MongoDB.Bson.BsonDocument();
                foreach (string word in dict.Keys)
                {
                    if (!_check(word)) continue;
                    forwarditem.words.Add(new MongoDB.Bson.BsonElement(word, dict[word]));
                    if (!words.Contains(word)) words.Add(word);
                }
                mongo.InsertForwardIndexItem(forwarditem);
            }
            Util.log("total {0} words.", words.Count);
            mongo.SaveWordDict(words);
        }

        static void CreateInvertedIndexAndIDF()
        {
            MongodbAccess mongo = new MongodbAccess();
            Dictionary<ObjectId, Dictionary<string, int>> forward_index = mongo.GetForwardIndex();
            Util.log("{0} items in forward index.", forward_index.Count);
            HashSet<string> words = mongo.GetWordDict();
            Util.log("{0} items in word dict.", words.Count);

            foreach (string w in words)
            {
                InvertedIndexItem inverteditem = new InvertedIndexItem();
                inverteditem.word = w;
                inverteditem.webpage_ids = new BsonArray();
                foreach (ObjectId webpageid in forward_index.Keys)
                {
                    if (forward_index[webpageid].ContainsKey(w))
                    {
                        inverteditem.webpage_ids.Add(webpageid);
                    }
                }
                mongo.InsertInvertedIndexItem(inverteditem);
                double idf = Math.Log10(forward_index.Count * 1.0 / inverteditem.webpage_ids.Count);
                mongo.SetWordIDF(w, idf);
            }

        }

        static void Main(string[] args)
        {
            //CreateForwordIndex();
            CreateInvertedIndexAndIDF();
        }
    }
}
