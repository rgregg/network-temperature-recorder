using System;
using System.Linq;
using System.IO;

namespace TemperatureRecorderConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            int pauseBetweenReadings = 5000;
            if (args.Length > 0)
            {
                var newPauseValue = Int32.Parse(args[0]);
                if (newPauseValue > 0)
                    pauseBetweenReadings = newPauseValue * 1000;
            }

            var probes = OneWireProbeReader.EnumerateDevices();
            if (probes.Length == 0)
            {
                Console.WriteLine("Unable to locate any devices. Aborting.");
                return;
            }

            while (true)
            {
                foreach (var probe in probes)
                {
                    var data = OneWireProbeReader.GetAverageValueFromDevice(probe, 5, 10);
                    if (null != data)
                    {
                        Console.WriteLine(data.ToString());
                    }
                }
                System.Threading.Thread.Sleep(pauseBetweenReadings);
            }
        }

     
    }
}
