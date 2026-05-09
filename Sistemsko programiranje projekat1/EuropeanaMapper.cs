using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
//using Newtonsoft.Json;
namespace Sistemsko_programiranje_projekat1
{
    internal class EuropeanaMapper
    {
        public string? apikey { get; set; }
        public bool success { get; set; }
        public int requestNumber { get; set; }
        public int itemsCount { get; set; }
        public List<Items> items { get; set; }
    }
}
