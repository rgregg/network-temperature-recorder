using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp
{
    public abstract class IDataRecorder
    {
        public virtual Task InitalizeAsync()
        {
            return Task.FromResult<bool>(true);
        }

        public abstract Task RecordDataAsync(TemperatureData data);
    }
}
