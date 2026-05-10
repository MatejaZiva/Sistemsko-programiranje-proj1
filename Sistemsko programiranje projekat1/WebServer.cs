using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sistemsko_programiranje_projekat1
{
    internal class WebServer
    {
        public HttpListener listener;
        public Cache cache;
        public Europeana api;
        public ConcurrentDictionary<string, SemaphoreSlim> queryE;

        public WebServer(AppSettings settings, string address)
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{settings.port}/");
            api = new Europeana(address, settings);
            cache = new Cache(settings.maxCacheSize);
            queryE = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        public void startTheServer()
        {
            try
            {
                listener.Start();
                Logger.Log("The server has started");
            }
            catch (Exception e)
            {
                Console.WriteLine("The server couldn't start " + e.Message);
            }

            while (true)
            {
                //ovde je main Thread i konstanto ceka za neku query i salje ga European-i 
                var context = listener.GetContext();
                ThreadPool.QueueUserWorkItem(handleRequest, context);
            }
        }


        public void handleRequest(Object? obj)
        {
            if (obj is not HttpListenerContext context)
                return;
            HttpListenerRequest request = context.Request;

            //kada se napravi klijent sa browser-a request on zbog nekog razloga na pravi dva requesta
            //i taj drugi request je ovaj dole string pa da bez potrebe pravi greske samo ce ga hardkodujem
            if (context.Request.Url?.ToString() == "http://localhost:8080/favicon.ico")
                return;

            var keys = request.QueryString.AllKeys;
            if (keys.Length == 0 || string.IsNullOrWhiteSpace(request.QueryString["query"]))
            {
                string page = CreateHtmlResponse(
                    "Europeana Search",
                    "<p>Use the <strong>query</strong> parameter to search the Europeana API.</p>"
                    + "<p>Example: <a href=\"?query=Lisa\">?query=Lisa</a></p>"
                    + "<form method=\"get\">"
                    + "<label>Search query: <input name=\"query\" value=\"\" /></label>"
                    + "<button type=\"submit\">Search</button>"
                    + "</form>"
                );
                sendDataToClient(page, context, 200);
                return;
            }

            string query = "?";
            foreach (var key in keys)
            {
                query += key;
                query += "=";
                query += request.QueryString.Get(key);
                query += "&";
            }

            query += $"wskey={api.apiKey}";

            String jsonDataAsString = String.Empty;
            EuropeanaMapper? mapper = null;


            //Odma proveri u kes da li postoji
            if (cache.checkForKey(query))
            {
                Logger.Log("The query was found in the cache");
                mapper = cache.getDataFromCache(query);
                string prettyJson = JsonSerializer.Serialize(mapper, new JsonSerializerOptions { WriteIndented = true });
                string responseHtml = RenderJsonPage(prettyJson);
                WebServer.sendDataToClient(responseHtml, context, 200);
                return;
            }

            //Posto nema u kes onda mora da vidi dal postoji semafor za njega

            SemaphoreSlim mainSem = queryE.GetOrAdd(query, (query) => new SemaphoreSlim(1));
            int codeSend;
            mainSem.Wait();
            
            try
            {
                if (!cache.checkForKey(query))
                {

                    Logger.Log("The query wasn't found in the cache");

                    var response = api.client.GetAsync(query).Result;

                    if (response.IsSuccessStatusCode == false)
                    {
                        codeSend = 500;
                        sendDataToClient("Failure", context, codeSend);
                        throw new Eexceptions("The GET method has failed", codeSend);
                    }

                    jsonDataAsString = response.Content.ReadAsStringAsync().Result;
                    mapper = JsonSerializer.Deserialize<EuropeanaMapper>(jsonDataAsString);
                    if (mapper == null)
                    {
                        throw new InvalidOperationException("Failed to deserialize Europeana response.");
                    }

                    if (mapper.itemsCount == 0)
                    {
                        Logger.Log($"The query: {query} is not valid");
                        codeSend = 404;
                    }
                    else
                    {
                        cache.addToCache(query, mapper);
                        codeSend = 200;
                    }

                }
                else
                {
                    Logger.Log("The query was found in the cache");
                    codeSend = 200;
                    mapper = cache.getDataFromCache(query);
                }

                string responseHtml;
                if (codeSend == 200)
                {
                    string prettyJson = JsonSerializer.Serialize(mapper, new JsonSerializerOptions { WriteIndented = true });
                    responseHtml = RenderJsonPage(prettyJson);
                }
                else if (codeSend == 404)
                {
                    responseHtml = CreateHtmlResponse("No results found", $"<p>The query returned no results.</p><p><strong>{WebUtility.HtmlEncode(request.QueryString["query"])}</strong></p>");
                }
                else
                {
                    responseHtml = CreateHtmlResponse("Failure", "<p>Failure while fetching from Europeana.</p>");
                }

                WebServer.sendDataToClient(responseHtml, context, codeSend);
            }
            catch (Eexceptions e)
            {
                Logger.Log($"An error has occured: {e.Message} {e.errorCode}");
            }
            catch (Exception e)
            {
                Logger.Log("Something went really wrong: " + e.Message);
            }
            finally
            {
                mainSem.Release();
            }

        }


        public static void sendDataToClient(string dataToSend,HttpListenerContext context,int statusCode)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(dataToSend);
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.StatusCode = statusCode;
            context.Response.OutputStream.Write(buffer,0,buffer.Length);
            context.Response.Close();
        }

        private static string RenderJsonPage(string json)
        {
            string escaped = WebUtility.HtmlEncode(json);
            return CreateHtmlResponse("Europeana result", $"<pre>{escaped}</pre>");
        }

        private static string CreateHtmlResponse(string title, string bodyHtml)
        {
            return "<!DOCTYPE html>\n"
                + "<html lang=\"en\">\n"
                + "<head>\n"
                + "    <meta charset=\"utf-8\">\n"
                + "    <title>" + WebUtility.HtmlEncode(title) + "</title>\n"
                + "    <style>\n"
                + "        body { font-family: Segoe UI, Arial, sans-serif; margin: 1rem; background: #f6f8fb; color: #1a1a1a; }\n"
                + "        pre { background: #ffffff; border: 1px solid #d1d5db; padding: 1rem; overflow: auto; white-space: pre-wrap; word-wrap: break-word; }\n"
                + "        a { color: #0366d6; text-decoration: none; }\n"
                + "        a:hover { text-decoration: underline; }\n"
                + "        button { margin-top: 0.5rem; padding: 0.5rem 1rem; }\n"
                + "    </style>\n"
                + "</head>\n"
                + "<body>\n"
                + "    <h1>" + WebUtility.HtmlEncode(title) + "</h1>\n"
                + bodyHtml + "\n"
                + "</body>\n"
                + "</html>";
        }
    }
}
