using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp
{
    interface ITemperatureReader
    {
        string[] EnumerateDevices();
        TemperatureData GetValueFromDevice(string deviceId);
        TemperatureData GetAverageValueFromDevice(string deviceId, double measurements);

    }
}
