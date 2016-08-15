using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace TemperatureRecorderConsoleApp
{
    public class ConfigurationFile
    {


        /// <summary>
        /// Number of seconds > 0 to poll for the temperature
        /// </summary>
        public int TemperaturePollingIntervalSeconds { get; set; }

        public string DataRecorder { get; set; }
        public string TemperatureSource { get; set; }

        public string CloudDataFilePath { get; set; }
        public string Office365UserName { get; set; }
        public string Office365Password { get; set; }
        public string Office365ClientId { get; set; }
        public string Office365TokenService { get; set; }
        public string Office365ResourceUrl { get; set; }
        public string Office365RedirectUri { get; set; }


        public ConfigurationFile()
        {
            DataRecorder = "Console";
            TemperatureSource = "OneWire";
            TemperaturePollingIntervalSeconds = 60;
        }

        /// <summary>
        /// Loads the default configuration file from ~/.iotTempRecorder.rc
        /// </summary>
        /// <returns></returns>
        public static ConfigurationFile ReadDefault()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var file = new FileInfo(Path.Combine(path, ".iotTempRecorder.rc"));
            if (!file.Exists)
            {
                Console.WriteLine("Couldn't find configuration file: " + file.FullName);
                return new ConfigurationFile();
            }

            using (var reader = file.OpenText())
            {
                var config = JsonConvert.DeserializeObject<ConfigurationFile>(reader.ReadToEnd());
                return config;
            }
        }

    }
}
