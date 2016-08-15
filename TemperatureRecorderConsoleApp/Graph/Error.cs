using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp.Graph
{
    class ErrorResponse
    {
        [JsonProperty("error")]
        public ErrorMessage Error { get; set; }   
    }

    class ErrorMessage
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }


        [JsonProperty("innerError")]
        public InnerError InnerError { get; set; }
    }

    class InnerError
    {
        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("request-id")]
        public string RequestId {get;set;}
    }
}
