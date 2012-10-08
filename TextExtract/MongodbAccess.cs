using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using Crawler;

namespace TextExtract
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

        public void SetWebPageContent(string url, string content)
        {
            MongoCollection<WebPage> collection = mongo_database.GetCollection<WebPage>("webpages");
            IMongoQuery query = Query.EQ("url", url);
            IMongoUpdate update = Update.Set("content", content);
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
