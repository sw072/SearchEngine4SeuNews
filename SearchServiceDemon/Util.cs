﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Crawler
{
    class Util
    {
        static public void log(string fmt, params object[] paras)
        {
            Console.WriteLine(DateTime.Now.ToString() + " INFO: " + fmt, paras);
        }
    }

    public class WebPage
    {
        public ObjectId _id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string html { get; set; }
        public string content { get; set; }
        public DateTime published_time { get; set; }
    }

    public class WordIDF
    {
        public ObjectId _id { get; set; }
        public string word { get; set; }
        public double idf { get; set; }
    }

    public class ForwardIndexItem
    {
        public ObjectId _id { get; set; }
        public ObjectId webpage_id { get; set; }
        public BsonDocument words;
    }

    public class InvertedIndexItem
    {
        public ObjectId _id { get; set; }
        public string word { get; set; }
        public BsonArray webpage_ids { get; set; }
    }
}
