namespace NativeAssist.Generators;

using NativeAssist.CLI;
using NativeAssist.Properties;
using NativeRefHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal partial class EnhancedClassicGenerator : IDisposable
{
    private readonly StreamWriter writer;
    private readonly Options _options;
    private readonly Dictionary<string, Dictionary<string, NativeFunction>> natives;

    public EnhancedClassicGenerator(Stream target, Dictionary<string, Dictionary<string, NativeFunction>> natives,
        Options options)
    {
        _options = options;
        this.writer = new StreamWriter(target);
        this.natives = natives;
    }

    public void Initialise()
    {
        Util.Logger.Information("Initialising enhanced classic generator");
        var escapeds = Resources.EClassicEscapedWords.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var split in escapeds)
        {
            _escapedWords.Add(split);
        }

        var aliases = Resources.EClassicScrHandleAliases.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var split in aliases)
        {
            _scrHandleAliases.Add(split);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        writer.Dispose();
    }

    ~EnhancedClassicGenerator()
    {
        Dispose(false);
    }

    public void Run()
    {
        foreach (var ns in natives)
        {
            var nsKey = ns.Key;
            Util.Logger.Information("Processing namespace {NsKey}", nsKey);
            writer.WriteLine($"#region {ns.Key}");

            foreach (var native in ns.Value)
            {
                this.OperateNative(native.Value, native.Key, nsKey);
            }

            writer.WriteLine($"#endregion");
        }

        writer.WriteLine("}");
    }

    public void WriteHeader(string version)
    {
        writer.WriteLine(Resources.FileHeader.Replace("$version$", version)
            .Replace("$ns$", _options.Namespace)
            .Replace("$class$", _options.ClassName));
    }
}
