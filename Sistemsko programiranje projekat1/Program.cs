using Sistemsko_programiranje_projekat1;

AppSettings set = new AppSettings();
WebServer srv = new WebServer(set, "https://api.europeana.eu/record/v2/search.json");

srv.startTheServer();

