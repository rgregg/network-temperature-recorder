namespace TemperatureRecorderConsoleApp
{
    using System;
    using CommandLine;
    using System.Threading.Tasks;
    using Nito.AsyncEx.Synchronous;
    using Nito.AsyncEx;

    class Program
    {
        private static ConfigurationFile Config;
        private static LogFileWriter LogWriter;
        private static bool KeepRunning = true;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                Program.LogMessage("Exit request received...");
                e.Cancel = true;
                Program.KeepRunning = false;
                Environment.Exit(-1);
            };

            var task = MainAsync(args);
            try
            {
                task.WaitAndUnwrapException();
            }
            catch (Exception ex)
            {
                Program.LogMessage("An exception occured: " + ex.ToString());
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press return to exit.");
                Console.ReadLine();
            }
        }

        static async Task MainAsync(string[] args) 
        {
            var options = new CommandLineArgs();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Program.LogMessage("Unable to parse command line options. Exiting.");
                return;
            }

            if (!String.IsNullOrEmpty(options.ConfigurationFilePath))
            {
                Config = ConfigurationFile.ReadFromPath(options.ConfigurationFilePath);
            }
            else
            {
                Config = ConfigurationFile.ReadDefault();
            }
            Config.Quiet = options.Quiet;

            if (!string.IsNullOrEmpty(Config.LogFilePath))
            {
                LogWriter = new LogFileWriter(Config.LogFilePath);
            }

            ITemperatureReader reader = null;
            IDataRecorder recorder = null;
            try
            {
                reader = GetTemperatureReader(Config);
                recorder = await GetDataRecorderAsync(Config);
            } catch (Exception ex)
            {
                Program.LogMessage("Failed to create required dependencies: " + ex.Message + ". Exiting.");
                return;
            }

            var probes = reader.EnumerateDevices();
            if (probes.Length == 0)
            {
                Program.LogMessage("Unable to locate any devices. Exiting.");
                return;
            }

            Program.LogMessage("Collecting temperatures every " + Config.TemperaturePollingIntervalSeconds + " seconds.");

            while (Program.KeepRunning)
            {
                foreach (var probe in probes)
                {
                    try
                    {
                        var data = reader.GetAverageValueFromDevice(probe, 5);
                        if (null != data)
                        {
                            await recorder.RecordDataAsync(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.LogMessage("Exception occured: " + ex.Message);
                    }
                }
                System.Threading.Thread.Sleep(Config.TemperaturePollingIntervalSeconds * 1000);
            }
        }

        public static void LogMessage(string message)
        {
            if (null == Config || !Config.Quiet)
                Console.WriteLine(message);
            else
                System.Diagnostics.Debug.WriteLine(message);

            if (null != LogWriter)
            {
                LogWriter.LogMessage(message);
            }
        }

        private static async Task<IDataRecorder> GetDataRecorderAsync(ConfigurationFile config)
        {
            IDataRecorder recorder;
            switch (config.DataRecorder)
            {
                case ConfigurationFile.DataRecorderServices.Console:
                    recorder = new ConsoleDataRecorder();
                    break;
                case ConfigurationFile.DataRecorderServices.Office365:
                    recorder = new MicrosoftGraph.Office365DataRecorder((MicrosoftGraph.MicrosoftGraphConfig)config);
                    break;
                case ConfigurationFile.DataRecorderServices.GoogleCloudPubSub:
                    recorder = new GoogleCloud.GoogleCloudPubSubRecorder((GoogleCloud.GoogleCloudConfig)config);
                    break;
                default:
                    throw new NotSupportedException("DataRecorder value '" + config.DataRecorder + "' is not supported. Expected values are 'Console', 'Office365' or 'GoogleCloudPubSub'.");
            }
            await recorder.InitalizeAsync();

            return recorder;
        }

        private static ITemperatureReader GetTemperatureReader(ConfigurationFile config)
        {
            ITemperatureReader reader;
            switch (config.TemperatureSource)
            {
                case ConfigurationFile.TemperatureSources.OneWire:
                    reader = new OneWireProbeReader();
                    break;
                case ConfigurationFile.TemperatureSources.Simulator:
                    reader = new ProbeSimulator();
                    break;
                default:
                    throw new NotSupportedException("TemperatureSource value '" + config.TemperatureSource + "' is not supported. Expected values are 'OneWire' or 'Simulator'.");
            }

            return reader;
        }
    }
}
