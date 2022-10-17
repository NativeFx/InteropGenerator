namespace NativeRefHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public struct NativeFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("jhash")]
    public string JenkinsHash { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    [JsonPropertyName("params")]
    public List<NativeParameter> Parameters { get; set; }

    [JsonPropertyName("return_type")]
    public string ReturnType { get; set; }

    [JsonPropertyName("build")]
    public string Build { get; set; }
}
