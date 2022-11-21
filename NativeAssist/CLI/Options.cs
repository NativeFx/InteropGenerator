﻿namespace NativeAssist.CLI;

using CommandLine;

public class Options
{
    [Option('o', "offline", Required = false, HelpText = "If set, the generator does not download latest natives.json from Internet.")]
    public bool Offline { get; set; }

    [Option("handle-type", Default = "int", Required = false, HelpText = "The type for the handles of Entities, &c. for use in natives.")]
    public string HandleType { get; set; } = "int";
}
