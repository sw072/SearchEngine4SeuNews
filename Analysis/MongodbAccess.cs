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
