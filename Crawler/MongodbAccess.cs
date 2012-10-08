using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;

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

        ~MongodbAccess()
        {
            mongo_server.Disconnect();
            mongo_database = null;
        }
    }
}
