namespace TemperatureRecorderConsoleApp
{
    using System;
    using CommandLine;

    class Program
    {
        private static ConfigurationFile Config;
        private static LogFileWriter LogWriter;

        static void Main(string[] args)
        {
            var options = new CommandLineArgs();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Program.LogMessage("Unable to parse command line options.");
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

            ITemperatureReader reader = GetTemperatureReader(Config);
            IDataRecorder recorder = GetDataRecorder(Config);

            var probes = reader.EnumerateDevices();
            if (probes.Length == 0)
            {
                Program.LogMessage("Unable to locate any devices. Aborting.");
                return;
            }

            while (true)
            {
                foreach (var probe in probes)
                {
                    try
                    {
                        var data = reader.GetAverageValueFromDevice(probe, 5);
                        if (null != data)
                        {
                            recorder.RecordDataAsync(data);
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

        private static IDataRecorder GetDataRecorder(ConfigurationFile config)
        {
            IDataRecorder recorder;
            switch (config.DataRecorder)
            {
                case ConfigurationFile.DataRecorderServices.Console:
                    recorder = new ConsoleDataRecorder();
                    break;
                case ConfigurationFile.DataRecorderServices.Office365:
                    recorder = new Office365DataRecorder(config);
                    break;
                default:
                    throw new NotSupportedException("DataRecorder value not supported.");
            }

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
                    throw new NotSupportedException("TemperatureSource value not supported.");
            }

            return reader;
        }
    }
}
