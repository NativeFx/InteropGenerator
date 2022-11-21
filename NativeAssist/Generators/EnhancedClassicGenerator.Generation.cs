namespace NativeAssist.Generators;

using NativeRefHelper.Models;
using Serilog.Core;
using System.Security;

partial class EnhancedClassicGenerator
{
    private readonly HashSet<string> _escapedWords = new();
    private readonly HashSet<string> _scrHandleAliases = new();
    private const string _pointerVarName = "nativeAssistPointerVar";

    private string ProcessParamName(string src, string nativeName)
    {
        foreach (var word in _escapedWords)
        {
            // If the specified name is C# keyword
            if (src == word)
            {
                // Warn the user
                Util.Logger.Warning($"Native {nativeName} cames with a parameter with name {src} that must be escaped");
                // Escape the word
                return $"@{src}";
            }
        }

        return src;
    }

    private string GetReturnTypeTag(string src)
    {
        if (_scrHandleAliases.Contains(src))
        {
            return "int";
        }

        return src switch
        {
            "Any*" => "IntPtr",
            "const char*" => "string",
            "BOOL" => "bool",
            "Hash" => "uint",
            _ => src,
        };
    }

    private void OperateNative(NativeFunction func, string hash, string ns)
    {
        var popu = SecurityElement.Escape(func.Comment).ReplaceLineEndings("<br />");

        if (string.IsNullOrWhiteSpace(popu))
        {
            popu = "<i>No description available.</i>";
        }

        var hasUnkParam = func.Parameters.Any(x => x.Type == "Any");
        var warningText = hasUnkParam ? "There were parameter(s) that was labeled as <b>Any</b>, and these had been written as <c>int</c> by generator." : string.Empty;

        writer.Write(@$"/// <summary>
/// {popu}
/// </summary>
/// <remarks>
/// <para>
/// <b>In Namespace:</b> {ns}<br />
/// <b>From Build:</b> {func.Build}<br />
/// <b>Native ID:</b> {hash}<br />{warningText}
/// </para>
/// </remarks>
public static ");
                var rType = GetReturnTypeTag(func.ReturnType);
                writer.Write(rType);
                writer.Write(' ');

                // If native does not have a name, warn
                if (string.IsNullOrWhiteSpace(func.Name))
                {
                    Util.Logger.Warning("Native {Hash} does not have a name", hash);
                }

                var natName = func.Name;

                // If starts with underscore, remove underscore and warn user
                if (func.Name.StartsWith('_'))
                {
                    Util.Logger.Warning("Native {Hash} cames with a name \"{NatName}\" that is does not match its hash", hash, natName);
                    natName = func.Name[(func.Name.IndexOf('_') + 1)..];
                }

                // Use func name, or remove the number 0 from hash and use as name
                var declarationName = string.IsNullOrWhiteSpace(func.Name) ? hash[1..] : natName.PascalCase();

                // Get whether the native returns a value
                var returnsValue = func.ReturnType != "void" && !(func.ReturnType == "Any" && func.Comment.Contains("function returns nothing"));

                // Write function declaration
                writer.Write(declarationName);
                writer.Write('(');

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
                        writer.Write(", ");
                    }

                    notFirstParam = true;

                    // Get parameter name (escape if needed)
                    var pname = ProcessParamName(param.Name, declarationName);

                    var addPointerVars = false;
                    switch (param.Type)
                    {
                        default:
                            // Other types
                            argPassStatements.Add(pname);
                            writer.Write(param.Type);
                            break;
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
                            writer.Write($"int /* {param.Type} */");
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
                            addPointerVars = true;
                            isUnsafe = true;
                            argPassStatements.Add($"&{_pointerVarName}{ptlNum}");
                            writer.Write($"ref int /* {param.Type} */");
                            break;
                        case "char*":
                        case "const char*":
                            // String value
                            argPassStatements.Add(pname);
                            writer.Write("string");
                            break;
                        case "Any*":
                            // Pointer unknown type likely structure
                            argPassStatements.Add(pname);
                            writer.Write("int /* bug: structure */");
                            break;
                        case "BOOL":
                            // Boolean value
                            argPassStatements.Add(pname);
                            writer.Write("bool");
                            break;
                        case "Hash":
                            // JOAAT Hash value
                            argPassStatements.Add(pname);
                            writer.Write("uint");
                            break;
                        case "BOOL*":
                            // Pointer boolean value
                            addPointerVars = true;
                            isUnsafe = true;
                            argPassStatements.Add($"&{_pointerVarName}{ptlNum}");
                            writer.Write($"ref bool");
                            break;
                        case "Hash*":
                            // Pointer JOAAT hash value
                            addPointerVars = true;
                            isUnsafe = true;
                            argPassStatements.Add($"&{_pointerVarName}{ptlNum}");
                            writer.Write($"ref uint");
                            break;
                        case "int*":
                            // Int pointer value
                            addPointerVars = true;
                            isUnsafe = true;
                            argPassStatements.Add($"&{_pointerVarName}{ptlNum}");
                            writer.Write($"ref int");
                            break;
                        case "float*":
                            // Float pointer value
                            addPointerVars = true;
                            isUnsafe = true;
                            argPassStatements.Add($"&{_pointerVarName}{ptlNum}");
                            writer.Write($"ref float");
                            break;
                        case "Vector3*":
                            // Pointer Vector3 value
                            addPointerVars = true;
                            isUnsafe = true;
                            argPassStatements.Add($"&{_pointerVarName}{ptlNum}");
                            writer.Write($"ref Vector3");
                            break;
                        case "Any":
                            // Any value, write as int
                            argPassStatements.Add(pname);
                            writer.Write("int /* bug: Any */");
                            break;
                    }

                    writer.Write($" {pname}");

                    if (addPointerVars)
                    {
                        preStatements.Add($"var nativeAssistPointerVar{ptlNum} = {pname};");
                        postStatements.Add($"{pname} = nativeAssistPointerVar{ptlNum};");
                        ptlNum++;
                    }
                }

                writer.WriteLine("){");

                if (returnsValue && postStatements.Count != 0)
                {
                     preStatements.Add($"{retType} retVal;");
                }

                        foreach (var ps in preStatements)
                        {
                            writer.WriteLine(ps);
                        }

                        if (isUnsafe)
                        {
                            writer.WriteLine("unsafe {");
                        }

                        if (returnsValue && postStatements.Count == 0)
                        {
                            writer.Write("return ");
                        }
                        else if (returnsValue)
                        {
                            writer.Write("retVal = ");
                        }

                        // Native call
                        writer.Write("Function.Call");

                        if (returnsValue)
                        {
                            writer.Write('<');
                            writer.Write(retType);
                            writer.Write('>');
                        }

                        // Write native hash conversion
                        writer.Write($"((Hash){hash}uL");

                        foreach (var cc in argPassStatements)
                        {
                            writer.Write(", ");
                            writer.Write(cc);
                        }

                        writer.WriteLine(");");

                        if (isUnsafe)
                        {
                            writer.WriteLine("}");
                        }

                        foreach (var xs in postStatements)
                        {
                            writer.WriteLine(xs);
                        }

                        if (returnsValue && postStatements.Count != 0)
                        {
                            writer.WriteLine("return retVal;");
                        }

                        writer.WriteLine(@"}
");
    }
}