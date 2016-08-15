using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp
{
    class ProbeSimulator : ITemperatureReader
    {
        Random r = new Random();
        public string[] EnumerateDevices()
        {
            return new string[] { "device1" };
        }

        public TemperatureData GetAverageValueFromDevice(string deviceId, double measurements)
        {
            return new TemperatureData(DateTimeOffset.Now, deviceId, r.Next(0, 80) / 2.0);
        }

        public TemperatureData GetValueFromDevice(string deviceId)
        {
            return new TemperatureData(DateTimeOffset.Now, deviceId, r.Next(0, 80) / 2.0);
        }
    }
}
