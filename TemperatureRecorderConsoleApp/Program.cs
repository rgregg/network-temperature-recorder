using System;
using System.Linq;
using System.IO;

namespace TemperatureRecorderConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFile.ReadDefault();

            ITemperatureReader reader = GetTemperatureReader(config);
            IDataRecorder recorder = GetDataRecorder(config);

            var probes = reader.EnumerateDevices();
            if (probes.Length == 0)
            {
                Console.WriteLine("Unable to locate any devices. Aborting.");
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
                        Console.WriteLine("Exception occured: " + ex.Message);
                    }
                }
                System.Threading.Thread.Sleep(config.TemperaturePollingIntervalSeconds * 1000);
            }
        }

        private static IDataRecorder GetDataRecorder(ConfigurationFile config)
        {
            IDataRecorder recorder;
            switch (config.DataRecorder)
            {
                case "Console":
                    recorder = new ConsoleDataRecorder();
                    break;
                case "Office365":
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
                case "OneWire":
                    reader = new OneWireProbeReader();
                    break;
                case "Simulator":
                    reader = new ProbeSimulator();
                    break;
                default:
                    throw new NotSupportedException("TemperatureSource value not supported.");
            }

            return reader;
        }
    }
}
