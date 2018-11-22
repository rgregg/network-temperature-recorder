using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp.MicrosoftGraph
{
    public class MicrosoftGraphConfig : ConfigurationFile
    {
        public MicrosoftGraphConfig() : base()
        {
            // Set some default values
            Office365TokenService = "https://login.microsoftonline.com/common";
            Office365ResourceUrl = "https://graph.microsoft.com";
        }

        public string CloudDataFilePath { get; set; }
        public string Office365UserName { get; set; }
        public string Office365Password { get; set; }
        public string Office365ClientId { get; set; }
        public string Office365TokenService { get; set; }
        public string Office365ResourceUrl { get; set; }
        public string Office365RedirectUri { get; set; }
    }
}
