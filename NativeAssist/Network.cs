namespace NativeAssist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class Network
{
#pragma warning disable S1075
    private const string _nativeAssistUrl = "https://github.com/alloc8or/gta5-nativedb-data/raw/master/natives.json";
    private static readonly HttpClient _httpClient = new();

    public static string GetVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        if (version == null)
        {
            return "unknown";
        }
        else
        {
            return version.ToString();
        }
    }

    public static async Task GetLatestJson()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("WithLithum/NativeAssist", GetVersion()));

        Util.Logger.Information("Downloading latest native file");
        Console.WriteLine();
        var x = 0L;
        try
        {
            using var s = File.Create("natives_latest.json");
            using var ns = await _httpClient.GetStreamAsync(_nativeAssistUrl);

            while (ns.CanRead)
            {
                var b = ns.ReadByte();

                if (b == -1) break;

                s.WriteByte((byte)b);
                x++;
                Console.Write($"\r{x} written, current byte {b}");
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Util.Logger.Error(ex, "Failed to get latest natives json");
        }
    }
}
