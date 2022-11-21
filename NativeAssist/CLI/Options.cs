namespace NativeAssist.CLI;

using CommandLine;

public class Options
{
    [Option('o', "offline", Required = false, HelpText = "If set, the generator does not download latest natives.json from Internet.")]
    public bool Offline { get; set; }

    [Option("handle-type", Default = "int", Required = false, HelpText = "The type for the handles of Entities, &c. for use in natives.")]
    public string HandleType { get; set; } = "int";

    [Option("hash-type", Default = "uint", Required = false, HelpText = "The type for hashes.")]
    public string HashType { get; set; } = "uint";

    [Option('n', "namespace", Default = "NativeFx.Interop", Required = false, HelpText = "The name of the namespace for the generated file.")]
    public string Namespace { get; set; } = "NativeFx.Interop";

    [Option('c', "class-name", Default = "Natives", Required = false, HelpText = "The class of the generated class.")]
    public string ClassName { get; set; } = "Natives";

    [Option('f', "file-name", Default = "Natives.cs", Required = false, HelpText = "The file name.")]
    public string FileName { get; set; } = "Natives.cs";
}
