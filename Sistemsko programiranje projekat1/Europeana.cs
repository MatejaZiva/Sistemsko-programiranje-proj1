using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
namespace Sistemsko_programiranje_projekat1
{
    internal class Europeana
    {
        public HttpClient client;
        public string apiKey { get; set;}
        public Europeana(string address, AppSettings settings)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(address);
            apiKey = settings.apiKey;
        }


    }
}


