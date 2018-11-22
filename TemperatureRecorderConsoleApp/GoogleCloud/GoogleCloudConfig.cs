using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp.GoogleCloud
{
    class GoogleCloudConfig : ConfigurationFile
    {
        public string AuthorizationToken { get; set; }
        public string GoogleCloudProjectName { get; set; }
        public string PubSubTopicName { get; set; }
    }
}
