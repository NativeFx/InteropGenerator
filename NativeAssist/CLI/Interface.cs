using System.Diagnostics;
using System.Text.Json;
using NativeAssist.Generators;
using NativeAssist.Properties;
using NativeRefHelper.Models;

namespace NativeAssist.CLI;

internal static class Interface
{
    internal static async Task<Dictionary<string, Dictionary<string, NativeFunction>>?> ParseFile()
    {
        if (File.Exists("natives_latest.json"))
        {
            // Use stream to parse file
            await using var stream = File.OpenRead("natives_latest.json");
            return await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, NativeFunction>>>(stream);
        }

        // Otherwise, use string
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, NativeFunction>>>(
            Resources.natives);
    }

    internal static async Task<int> Run(Options opt)
    {
        var l = Util.Logger;
        var version = Network.GetVersion();

        l.Information("Starting NativeAssist version {Version}", version);

        if (!opt.Offline)
        {
            await Network.GetLatestJson();
        }
        else
        {
            l.Information("Offline mode");
        }

        var natives = await ParseFile();

        if (natives == null)
        {
            l.Fatal("Native data parsing failed");
            return -1;
        }

        l.Information("Generating via Enhanced Classic Generator");

        var sw = new Stopwatch();
        sw.Start();
        using var generator = new EnhancedClassicGenerator(File.Create("Natives.cs"), natives, opt);

        generator.Initialise();
        generator.WriteHeader(version);
        generator.Run();

        sw.Stop();

        var elapsed = sw.Elapsed;
        var elapsedMs = sw.ElapsedMilliseconds;

        l.Information("Generator complete, {Elapsed} ({ElapsedMs}) elapsed", elapsed, elapsedMs);

        return 0;
    }
}
