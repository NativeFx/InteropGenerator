namespace NativeAssist.Generators;

using NativeAssist.Properties;
using NativeRefHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class EnhancedClassicGenerator : IDisposable
{
    private readonly StreamWriter writer;
    private readonly Dictionary<string, Dictionary<string, NativeFunction>> natives;

    public EnhancedClassicGenerator(Stream target, Dictionary<string, Dictionary<string, NativeFunction>> natives)
    {
        this.writer = new StreamWriter(target);
        this.natives = natives;
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
            Util.Logger.Information($"Processing namespace {ns.Key}");
            writer.WriteLine($"#region {ns.Key}");

            foreach (var native in ns.Value)
            {
                Util.OperateNative(native.Value, native.Key, writer);
            }

            writer.WriteLine($"#endregion");
        }

        writer.WriteLine("}");
    }

    public void WriteHeader(string version)
    {
        writer.WriteLine(Resources.FileHeader.Replace("$version$", version));
    }
}
