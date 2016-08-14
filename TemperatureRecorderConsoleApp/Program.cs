using System;
using System.Linq;
using System.IO;

namespace TemperatureRecorderConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo devicesDir = new DirectoryInfo("/sys/bus/w1/devices");
            if (!devicesDir.Exists)
            {
                Console.WriteLine($"Unable to locate device directory: {devicesDir.FullName}");
                return;
            }

            while (true)
            {
                ReadProbeData(devicesDir);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private static void ReadProbeData(DirectoryInfo devicesDir)
        {
            foreach (var deviceDir in devicesDir.EnumerateDirectories("28*"))
            {
                try
                {
                    var sourceDataFile = deviceDir.GetFiles("w1_slave").FirstOrDefault();
                    if (null == sourceDataFile)
                    {
                        Console.WriteLine($"Unable to file w1_slave for device {deviceDir.Name}");
                        continue;
                    }

                    string dataText = null;
                    using (var sourceDataReader = sourceDataFile.OpenText())
                    {
                        dataText = sourceDataReader.ReadToEnd();
                    }

                    string temptext = dataText.Split(new string[] { "t=" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    double temp_C = double.Parse(temptext) / 1000;
                    double temp_F = temp_C * 9.0 / 5.0 + 32.0;
                    Console.WriteLine($"Device {deviceDir.Name} reports temperature: {temp_C}C, {temp_F}F");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading data from probe {deviceDir.Name}: {ex.Message}");
                }
            }
        }
    }
}
