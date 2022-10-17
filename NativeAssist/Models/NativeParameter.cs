namespace NativeRefHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public struct NativeParameter
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
