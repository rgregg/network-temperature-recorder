using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureRecorderConsoleApp
{
    public class TemperatureData
    {
        [JsonProperty("date_time_utc")]
        public DateTimeOffset InstanceDateTime { get; private set; }
        [JsonProperty("device")]
        public string DeviceIdentifier { get; private set; }
        [JsonProperty("temp_c")]
        public double TemperatureC { get; private set; }
        [JsonProperty("temp_f")]
        public double TemperatureF
        {
            get
            {
                return TemperatureC * 9.0 / 5.0 + 32.0;
            }
        }

        public TemperatureData(DateTimeOffset time, string device, double tempC)
        {
            this.InstanceDateTime = time;
            this.DeviceIdentifier = device;
            this.TemperatureC = tempC;
        }

        public override string ToString()
        {
            return string.Format(
                "{3}: Device {0} reports temperature: {1}C, {2}F",
                DeviceIdentifier,
                TemperatureC,
                TemperatureF,
                InstanceDateTime);
        }
    }
}
