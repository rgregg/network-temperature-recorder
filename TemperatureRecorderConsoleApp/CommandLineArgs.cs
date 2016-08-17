namespace TemperatureRecorderConsoleApp
{
    using CommandLine;

    class CommandLineArgs
    {
        [Option("quiet", HelpText ="Don't print output to the console.")]
        public bool Quiet { get; set; }

        [Option("config", HelpText = "Path to configuration file.")]
        public string ConfigurationFilePath { get; set; }
    }
}
