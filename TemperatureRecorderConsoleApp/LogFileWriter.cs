namespace TemperatureRecorderConsoleApp
{
    using System;
    using System.IO;

    public class LogFileWriter
    {
        private TextWriter LogWriter;

        public LogFileWriter(string outputPath)
        {
            LogWriter = new StreamWriter(outputPath, true) { AutoFlush = true };
        }

        public void LogMessage(string message)
        {
            try
            {
                LogWriter.WriteLine(DateTimeOffset.Now.ToString() + " - " + message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error writing to log file: " + ex.ToString());
            }
        }
    }
}

