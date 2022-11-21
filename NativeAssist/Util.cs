namespace NativeAssist;

using NativeRefHelper.Models;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

internal static class Util
{
    public static Logger Logger { get; } = new LoggerConfiguration()
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .WriteTo.File("NativeAssist.log")
        .CreateLogger();

    private static readonly HashSet<string> _escapedWords = new()
    {
        "base",
        "override",
        "object",
        "string",
        "class",
        "struct",
        "event",
        "in",
        "out",
        "ref",
        "var"
    };

    public static string PascalCase(this string word)
    {
        return string.Join("", word.Split('_')
                     .Select(w => w.Trim())
                     .Where(w => w.Length > 0)
                     .Select(w => w[..1].ToUpper() + w[1..].ToLower()));
    }

    public static string WordToPascal(string word)
    {
        var x = word.ToLower();
        return x[..1].ToUpper() + x[1..].ToLower();
    }

    public static void OperateNative(NativeFunction func, string hash, StreamWriter target)
    {
        var popu = SecurityElement.Escape(func.Comment).ReplaceLineEndings("<br />");

        if (string.IsNullOrWhiteSpace(popu))
        {
            popu = "<i>No description available.</i>";
        }

        var hasUnkParam = func.Parameters.Any(x => x.Type == "Any");
        var warningText = hasUnkParam ? "There were parameter(s) that was labeled as <b>Any</b>, and these had been written as <c>int</c> by generator." : string.Empty;

        target.Write(@$"/// <summary>
/// {popu}
/// </summary>
/// <remarks>
/// <para>
/// <b>From Build:</b> {func.Build}<br />
/// <b>Native ID:</b> {hash}<br />{warningText}
/// </para>
/// </remarks>
public static ");
        var rType = GetReturnTypeTag(func.ReturnType);
        target.Write(rType);
        target.Write(' ');

        // If native does not have a name, warn
        if (string.IsNullOrWhiteSpace(func.Name))
        {
            Logger.Warning($"Native {hash} does not have a name");
        }

        var natName = func.Name;

        // If starts with underscore, remove underscore and warn user
        if (func.Name.StartsWith('_'))
        {
            Logger.Warning($"Native {hash} cames with a name \"{func.Name}\" that is does not match its hash");
            natName = func.Name[(func.Name.IndexOf('_') + 1)..];
        }

        // Use func name, or remove the number 0 from hash and use as name
        var declarationName = string.IsNullOrWhiteSpace(func.Name) ? hash[1..] : natName.PascalCase();

        // Get whether the native returns a value
        var returnsValue = func.ReturnType != "void" && !(func.ReturnType == "Any" && func.Comment.Contains("function returns nothing"));

        // Write function declaration
        target.Write(declarationName);
        target.Write('(');

        // Create lists
        var preStatements = new List<string>();
        var argPassStatements = new List<string>();
        var postStatements = new List<string>();

        // If true, process as unsafe
        var isUnsafe = false;
        
        // If true, this param is appended
        var notFirstParam = false;
        var retType = GetReturnTypeTag(func.ReturnType);

        var ptlNum = 0;

        foreach (var param in func.Parameters)
        {
            if (notFirstParam)
            {
                target.Write(", ");
            }

            notFirstParam = true;

            // Get parameter name (escape if needed)
            var pname = ProcessParamName(param.Name, declarationName);

            var x = false;
            switch (param.Type)
            {
                case "Vehicle":
                case "Object":
                case "Ped":
                case "Entity":
                case "Blip":
                case "Player":
                case "ScrHandle":
                case "Interior":
                case "Cam":
                case "FireId":
                    // Non-pointer script handle value, write as int
                    argPassStatements.Add(pname);
                    target.Write($"int /* {param.Type} */");
                    break;
                case "Vehicle*":
                case "Entity*":
                case "Ped*":
                case "Blip*":
                case "Object*":
                case "Player*":
                case "ScrHandle*":
                case "Interior*":
                case "Cam*":
                case "FireId*":
                    // Pointer script handle value, write as int*
                    x = true;
                    isUnsafe = true;
                    argPassStatements.Add($"&nativeAssistPointerVar{ptlNum}");
                    target.Write($"ref int /* {param.Type} */");
                    break;
                case "char*":
                case "const char*":
                    // String value
                    argPassStatements.Add(pname);
                    target.Write("string");
                    break;
                case "Any*":
                    // Pointer unknown type likely structure
                    argPassStatements.Add(pname);
                    target.Write("int /* bug: structure */");
                    break;
                default:
                    // Other types
                    argPassStatements.Add(pname);
                    target.Write(param.Type);
                    break;
                case "BOOL":
                    // Boolean value
                    argPassStatements.Add(pname);
                    target.Write("bool");
                    break;
                case "Hash":
                    // JOAAT Hash value
                    argPassStatements.Add(pname);
                    target.Write("uint");
                    break;
                case "BOOL*":
                    // Pointer boolean value
                    x = true;
                    isUnsafe = true;
                    argPassStatements.Add($"&nativeAssistPointerVar{ptlNum}");
                    target.Write($"ref bool");
                    break;
                case "Hash*":
                    // Pointer JOAAT hash value
                    x = true;
                    isUnsafe = true;
                    argPassStatements.Add($"&nativeAssistPointerVar{ptlNum}");
                    target.Write($"ref uint");
                    break;
                case "int*":
                    // Int pointer value
                    x = true;
                    isUnsafe = true;
                    argPassStatements.Add($"&nativeAssistPointerVar{ptlNum}");
                    target.Write($"ref int");
                    break;
                case "float*":
                    // Float pointer value
                    x = true;
                    isUnsafe = true;
                    argPassStatements.Add($"&nativeAssistPointerVar{ptlNum}");
                    target.Write($"ref float");
                    break;
                case "Vector3*":
                    // Pointer Vector3 value
                    x = true;
                    isUnsafe = true;
                    argPassStatements.Add($"&nativeAssistPointerVar{ptlNum}");
                    target.Write($"ref Vector3");
                    break;
                case "Any":
                    // Any value, write as int
                    argPassStatements.Add(pname);
                    target.Write("int /* bug: Any */");
                    break;
            }

            target.Write($" {pname}");

            if (x)
            {
                preStatements.Add($"var nativeAssistPointerVar{ptlNum} = {pname};");
                postStatements.Add($"{pname} = nativeAssistPointerVar{ptlNum};");
                ptlNum++;
            }
        }

        target.WriteLine(@")
{");

        if (returnsValue && postStatements.Count != 0)
        {
            preStatements.Add($"{retType} retVal;");
        }

        foreach (var ps in preStatements)
        {
            target.WriteLine(ps);
        }

        if (isUnsafe)
        {
            target.WriteLine("unsafe {");
        }

        if (returnsValue && postStatements.Count == 0)
        {
            target.Write("return ");
        }
        else if (returnsValue)
        {
            target.Write("retVal = ");
        }

        // Native call
        target.Write("Function.Call");

        if (returnsValue)
        {
            target.Write('<');
            target.Write(retType);
            target.Write('>');
        }

        // Write native hash conversion
        target.Write($"((Hash){hash}uL");

        foreach (var cc in argPassStatements)
        {
            target.Write(", ");
            target.Write(cc);
        }

        target.WriteLine(");");

        if (isUnsafe)
        {
            target.WriteLine("}");
        }

        foreach (var xs in postStatements)
        {
            target.WriteLine(xs);
        }

        if (returnsValue && postStatements.Count != 0)
        {
            target.WriteLine("return retVal;");
        }

        target.WriteLine(@"}
");
    }

    public static string ProcessParamName(string src, string nativeName)
    {
        foreach (var word in _escapedWords)
        {
            // If the specified name is C# keyword
            if (src == word)
            {
                // Warn the user
                Logger.Warning($"Native {nativeName} cames wtih a parameter with name {src} that must be escaped");
                // Escape the word
                return $"@{src}";
            }
        }

        return src;
    }

    public static string GetReturnTypeTag(string src)
    {
        return src switch
        {
            "Any*" => "IntPtr",
            "const char*" => "string",
            "BOOL" => "bool",
            "Ped" or "Entity" or "Blip" or "Vehicle" or "Object" or "ScrHandle" or "Interior"
            or "Cam" or "FireId" => "int",
            "Hash" => "uint",
            _ => src,
        };
    }
}
