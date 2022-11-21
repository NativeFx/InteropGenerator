namespace NativeAssist.CLI;
using CommandLine.Text;

using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Options
{
    [Option('o', "offline", Required = false, HelpText = "If set, the generator does not download latest natives.json from Internet.")]
    public bool Offline { get; set; }
}
