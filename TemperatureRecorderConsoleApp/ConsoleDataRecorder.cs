using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp
{
    class ConsoleDataRecorder : IDataRecorder
    {
        public Task RecordDataAsync(TemperatureData data)
        {
            Console.WriteLine(data.ToString());
            return Task.FromResult(true);
        }
    }
}
