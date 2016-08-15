namespace TemperatureRecorderConsoleApp
{
    using CommandLine;

    class CommandLineArgs
    {
        [Option("quiet", HelpText ="Don't print output to the console.")]
        public bool Quiet { get; set; }

    }
}
