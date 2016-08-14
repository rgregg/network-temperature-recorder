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
                Console.WriteLine(string.Format("Unable to locate device directory: {0}", devicesDir.FullName));
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
                        Console.WriteLine(string.Format("Unable to file w1_slave for device {0}", deviceDir.Name));
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
                    Console.WriteLine(string.Format("Device {0} reports temperature: {1}C, {2}F", deviceDir.Name, temp_C, temp_F));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error reading data from probe {0}: {1}", deviceDir.Name, ex.Message));
                }
            }
        }
    }
}
