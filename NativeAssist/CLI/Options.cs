namespace NativeAssist.CLI;

using CommandLine;

public class Options
{
    [Option('o', "offline", Required = false, HelpText = "If set, the generator does not download latest natives.json from Internet.")]
    public bool Offline { get; set; }
}
