using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Sistemsko_programiranje_projekat1
{
    internal class AppSettings
    {
        public string apiKey { get; set; }
        public string port { get; set; }

        public int maxCacheSize { get; set; }
        public AppSettings(string port="8080")
        {
            bool optionalToAdd = false;
            bool reloadIfChangeHappens = true;
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",optionalToAdd,reloadIfChangeHappens).Build();
            
            this.apiKey = builder["apikey"];
            this.maxCacheSize = Convert.ToInt32(builder["maxCacheSize"]);
            this.port = port;
        }
    }
}
