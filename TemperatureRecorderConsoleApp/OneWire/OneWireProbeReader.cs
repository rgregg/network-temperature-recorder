using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TemperatureRecorderConsoleApp
{
    public class OneWireProbeReader : ITemperatureReader
    {
        public static string DevicesDirectory = "/sys/bus/w1/devices";

        public string[] EnumerateDevices()
        {
            var devicesDir = new DirectoryInfo(DevicesDirectory);
            if (!devicesDir.Exists)
            {
                Program.LogMessage(string.Format("Unable to locate device directory: {0}", devicesDir.FullName));
                return new string[0];
            }

            var knownDevices = devicesDir.EnumerateDirectories("28*");
            return (from d in knownDevices select d.Name).ToArray();
        }

        public TemperatureData GetValueFromDevice(string deviceId)
        {
            var deviceDir = new DirectoryInfo(Path.Combine(DevicesDirectory, deviceId));
            if (!deviceDir.Exists)
            {
                throw new Exception(string.Format("Unable to locate directory for device {0}", deviceId));
            }

            var sourceDataFile = deviceDir.GetFiles("w1_slave").FirstOrDefault();
            if (null == sourceDataFile)
            {
                throw new Exception(string.Format("Unable to file w1_slave for device {0}", deviceId));
            }

            string dataText = null;
            DateTimeOffset instance = DateTimeOffset.UtcNow;
            using (var sourceDataReader = sourceDataFile.OpenText())
            {
                dataText = sourceDataReader.ReadToEnd();
            }

            string temptext = dataText.Split(new string[] { "t=" }, StringSplitOptions.RemoveEmptyEntries)[1];
            double temp_C = double.Parse(temptext) / 1000;

            return new TemperatureData(instance, deviceId, temp_C);
        }

        /// <summary>
        /// Reads {measurements} values from the probe over {seconds} and returns the average temperature value recorded.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="seconds"></param>
        /// <param name="measurements"></param>
        /// <returns></returns>
        public TemperatureData GetAverageValueFromDevice(string deviceId, double measurements)
        {
            const int sleepTimer = 500;
            List<TemperatureData> recordedValues = new List<TemperatureData>();
            for (int i = 0; i < measurements; i++)
            {
		        var data = GetValueFromDevice(deviceId);
                if (null != data)
                    recordedValues.Add(data);
                System.Threading.Thread.Sleep(sleepTimer);
            }
            if (recordedValues.Count > 0)
            {
                double averageTemperature = 0;
                foreach (var data in recordedValues)
                {
                    averageTemperature += data.TemperatureC;
                }
                return new TemperatureData(DateTimeOffset.UtcNow, deviceId, averageTemperature / recordedValues.Count);
            }

            return null;
        }
    }


   
}
