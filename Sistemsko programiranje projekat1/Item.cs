using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sistemsko_programiranje_projekat1
{
    internal class Items
    {
        public string? id { get; set; }
        public int? completeness { get; set; }
        public string? link { get; set; }
        public List<string>? dcDescription { get; set; }
        public List<string>? title { get; set; }
        public List<string>? dataProvider { get; set; }
    }


}
