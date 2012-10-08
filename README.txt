This is a simple search engine for SEU news website(http://news.seu.edu.cn).

Crawler
    crawler the news pages and stroe to MongoDB.

Analysis
	wordsegment
	create forward index
	create inverted index
	calculate idf of each word

SearchServiceDemon
	a simple http server that processes every query.

WebApplication1
	the web interface for this search engine.