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
        /// Frequency a data point is collected
        /// </summary>
        public int TemperaturePollingIntervalSeconds { get; set; }

        /// <summary>
        /// Path to write out a log file
        /// </summary>
        public string LogFilePath { get; set; }

        /// <summary>
        /// Data recorder that should be used by the program
        /// </summary>
        public DataRecorderServices DataRecorder { get; set; }

        /// <summary>
        /// Temperature source that should be used by the program
        /// </summary>
        public TemperatureSources TemperatureSource { get; set; }

        /// <summary>
        /// Don't write to the console if Quiet == true.
        /// </summary>
        [JsonIgnore]
        public bool Quiet { get; set; }

        public ConfigurationFile()
        {
            DataRecorder = DataRecorderServices.Console;
            TemperatureSource = TemperatureSources.Simulator;
            TemperaturePollingIntervalSeconds = 60;
            LogFilePath = "temperature-recorder.log";
        }

	    public static ConfigurationFile ReadFromPath(string path)
        {
            if (!File.Exists(path))
            {
                Program.LogMessage("Couldn't find configuration file: '" + path + "'. Using default configuration.");
                return new ConfigurationFile();
            }

            using (var reader = File.OpenText(path))
            {
                var configData = reader.ReadToEnd();
                var config = JsonConvert.DeserializeObject<ConfigurationFile>(configData);
                switch(config.DataRecorder)
                {
                    case DataRecorderServices.Office365:
                        Program.LogMessage("Parsing configuration file for Microsoft Graph");
                        config = JsonConvert.DeserializeObject<MicrosoftGraph.MicrosoftGraphConfig>(configData);
                        break;
                    case DataRecorderServices.GoogleCloudPubSub:
                        Program.LogMessage("Parsing configuration file for Google Cloud Pub/Sub");
                        config = JsonConvert.DeserializeObject<GoogleCloud.GoogleCloudConfig>(configData);
                        break;
                }
                return config;
            }
        }

        /// <summary>
        /// Loads the default configuration file from ~/.iotTempRecorder.rc
        /// </summary>
        /// <returns></returns>
        public static ConfigurationFile ReadDefault()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return ReadFromPath(Path.Combine(path, "temperature-recorder.config"));
        }

        public enum TemperatureSources
        {
            Simulator = 0,
            OneWire = 1,
        }

        public enum DataRecorderServices
        {
            Console = 0,
            Office365 = 1,
            GoogleCloudPubSub = 2,
        }

    }
}
