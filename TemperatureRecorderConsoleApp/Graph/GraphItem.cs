using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp.Graph
{
    class GraphItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }
    }
}
