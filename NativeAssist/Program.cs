// See https://aka.ms/new-console-template for more information
using CommandLine;
using NativeAssist;
using NativeAssist.CLI;
using NativeRefHelper.Models;
using System.Diagnostics;
using System.Text.Json;
var l = Util.Logger;
var result = Parser.Default.ParseArguments<Options>(args);

if (result?.Value == null)
{
    return 0;
}

l.Information("Starting new NativeAssist");

return await Interface.Run(result.Value);