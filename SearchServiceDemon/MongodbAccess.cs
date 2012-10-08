using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;

namespace Crawler
{
    public class MongodbAccess
    {
        public string connect_string = "mongodb://localhost";
        public string database = "searchengine";
        private MongoServer mongo_server;
        private MongoDatabase mongo_database;

        public MongodbAccess()
        {
            mongo_server = MongoServer.Create(connect_string);
            mongo_database = mongo_server.GetDatabase(database);
        }

        public void InsertWebPage(WebPage page)
        {
            MongoCollection<WebPage> collection = mongo_database.GetCollection<WebPage>("webpages");
            SafeModeResult smr = collection.Insert<WebPage>(page);
        }

        public void InsertForwardIndexItem(ForwardIndexItem fitem)
        {
            MongoCollection<ForwardIndexItem> collection = mongo_database.GetCollection<ForwardIndexItem>("forwardindex");
            SafeModeResult smr = collection.Insert<ForwardIndexItem>(fitem);
        }

        public void InsertInvertedIndexItem(InvertedIndexItem iitem)
        {
            MongoCollection<InvertedIndexItem> collection = mongo_database.GetCollection<InvertedIndexItem>("invertedindex");
            SafeModeResult smr = collection.Insert<InvertedIndexItem>(iitem);
        }

        public void SaveWordDict(HashSet<string> words)
        {
            MongoCollection<WordIDF> collection = mongo_database.GetCollection<WordIDF>("words");
            foreach (string w in words)
            {
                WordIDF wi = new WordIDF();
                wi.word = w;
                wi.idf = 0;
                collection.Insert<WordIDF>(wi);
            }
        }

        public HashSet<string> GetCrawledURLs()
        {
            HashSet<string> ret = new HashSet<string>();
            MongoCollection<WebPage> collection = mongo_database.GetCollection<WebPage>("webpages");
            MongoCursor cursor = collection.FindAll();
            foreach (WebPage page in cursor)
            {
                ret.Add(page.url);
            }
            return ret;
        }

        public WebPage GetWebPageByURL(string url)
        {
            MongoCollection<WebPage> collection = mongo_database.GetCollection<WebPage>("webpages");
            IMongoQuery query = Query.EQ("url", url);
            WebPage ret = collection.FindOne(query);
            return ret;
        }

        public WebPage GetWebPageById(ObjectId id)
        {
            MongoCollection<WebPage> collection = mongo_database.GetCollection<WebPage>("webpages");
            IMongoQuery query = Query.EQ("_id", id);
            WebPage ret = collection.FindOne(query);
            return ret;
        }

        public HashSet<string> GetWordDict()
        {
            HashSet<string> ret = new HashSet<string>();
            MongoCollection<WordIDF> collection = mongo_database.GetCollection<WordIDF>("words");
            MongoCursor cursor = collection.FindAll();
            foreach (WordIDF wi in cursor)
            {
                ret.Add(wi.word);
            }
            return ret;
        }

        public Dictionary<string, double> GetWordIDF()
        {
            Dictionary<string, double> word_idf = new Dictionary<string, double>();
            MongoCollection<WordIDF> collection = mongo_database.GetCollection<WordIDF>("words");
            MongoCursor cursor = collection.FindAll();
            foreach (WordIDF wi in cursor)
            {
                word_idf[wi.word] = wi.idf;
            }
            return word_idf;
        }

        public Dictionary<ObjectId, Dictionary<string, int>> GetForwardIndex()
        {
            Dictionary<ObjectId, Dictionary<string, int>> ret = new Dictionary<ObjectId, Dictionary<string, int>>();
            MongoCollection<ForwardIndexItem> collection = mongo_database.GetCollection<ForwardIndexItem>("forwardindex");
            MongoCursor cursor = collection.FindAll();
            foreach (ForwardIndexItem fitem in cursor)
            {
                ret[fitem.webpage_id] = new Dictionary<string, int>();
                foreach (BsonElement e in fitem.words)
                {
                    ret[fitem.webpage_id][e.Name] = e.Value.AsInt32;
                }
            }
            return ret;
        }

        public ForwardIndexItem GetForwardIndexItemByDocId(ObjectId docId)
        {
            MongoCollection<ForwardIndexItem> collection = mongo_database.GetCollection<ForwardIndexItem>("forwardindex");
            IMongoQuery query = Query.EQ("webpage_id", docId);
            return collection.FindOne(query);
        }

        public List<ObjectId> GetDocIDByQuery(List<string> query_strings)
        {
            List<ObjectId> ret = new List<ObjectId>();
            HashSet<ObjectId> set = new HashSet<ObjectId>();
            HashSet<ObjectId> other = null;
            MongoCollection<InvertedIndexItem> collection = mongo_database.GetCollection<InvertedIndexItem>("invertedindex");
            foreach(string q in query_strings)
            {
                IMongoQuery query = Query.EQ("word", q);
                MongoCursor<InvertedIndexItem> cursor = collection.Find(query);
                foreach (InvertedIndexItem iitem in cursor)
                {
                    if (set.Count == 0)
                    {
                        foreach (ObjectId docId in iitem.webpage_ids)
                        {
                            set.Add(docId);
                        }
                    }
                    else
                    {
                        other = new HashSet<ObjectId>();
                        foreach (ObjectId docId in iitem.webpage_ids)
                        {
                            other.Add(docId);
                        }
                        set.IntersectWith(other);
                    }
                }
            }
            foreach (ObjectId docId in set)
            {
                ret.Add(docId);
            }
            return ret;
        }

        public void SetWebPageContent(string url, string content)
        {
            MongoCollection<WebPage> collection = mongo_database.GetCollection<WebPage>("webpages");
            IMongoQuery query = Query.EQ("url", url);
            IMongoUpdate update = Update.Set("content", content);
            collection.Update(query, update);
        }

        public void SetWordIDF(string word, double idf)
        {
            MongoCollection<WordIDF> collection = mongo_database.GetCollection<WordIDF>("words");
            IMongoQuery query = Query.EQ("word", word);
            IMongoUpdate update = Update.Set("idf", idf);
            collection.Update(query, update);
        }

        public void RemoveWebPageById(ObjectId id)
        {
            MongoCollection<WebPage> collection = mongo_database.GetCollection<WebPage>("webpages");
            IMongoQuery query = Query.EQ("_id", id);
            collection.Remove(query);
        }

        ~MongodbAccess()
        {
            mongo_server.Disconnect();
            mongo_database = null;
        }
    }
}
