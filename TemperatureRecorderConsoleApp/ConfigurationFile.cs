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

        public DataRecorderServices DataRecorder { get; set; }
        public TemperatureSources TemperatureSource { get; set; }
        public string CloudDataFilePath { get; set; }
        public string Office365UserName { get; set; }
        public string Office365Password { get; set; }
        public string Office365ClientId { get; set; }
        public string Office365TokenService { get; set; }
        public string Office365ResourceUrl { get; set; }
        public string Office365RedirectUri { get; set; }
        public string LogFilePath { get; set; }

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
            Office365TokenService = "https://login.microsoftonline.com/common";
            Office365ResourceUrl = "https://graph.microsoft.com";
            LogFilePath = "~/.iotTempRecorder/output.txt";
        }

	public static ConfigurationFile ReadFromPath(string path)
        {
            var file = new FileInfo(path);
            return ReadFromFileInfo(file);   
        }

        private static ConfigurationFile ReadFromFileInfo(FileInfo file)
        {
            if (!file.Exists)
            {
                Program.LogMessage("Couldn't find configuration file: " + file.FullName);
                return new ConfigurationFile();
            }

            using (var reader = file.OpenText())
            {
                var config = JsonConvert.DeserializeObject<ConfigurationFile>(reader.ReadToEnd());
                return config;
            }

        }

        /// <summary>
        /// Loads the default configuration file from ~/.iotTempRecorder.rc
        /// </summary>
        /// <returns></returns>
        public static ConfigurationFile ReadDefault()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var file = new FileInfo(Path.Combine(path, ".iotTempRecorder.rc"));
            return ReadFromFileInfo(file);
        }

        public enum TemperatureSources
        {
            Simulator = 0,
            OneWire = 1
        }

        public enum DataRecorderServices
        {
            Console = 0,
            Office365 = 1
        }

    }
}
