using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp.GoogleCloud
{
    public class GoogleCloudConfig : ConfigurationFile
    {
        public string AuthorizationTokenPath { get; set; }
        public string GoogleCloudProjectId { get; set; }
        public string PubSubTopicName { get; set; }
    }
}
