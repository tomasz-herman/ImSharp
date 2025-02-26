using Newtonsoft.Json.Linq;

namespace ImSharpGenerator;

public class TypesInfo
{
    private Dictionary<string, string> WellKnownTypes { get; } = new()
    {
        ["long long"] = "long",
        ["long"] = "long",
        ["int"] = "int",
        ["short"] = "short",
        ["char"] = "byte",
        ["signed long long"] = "long",
        ["signed long"] = "long",
        ["signed int"] = "int",
        ["signed short"] = "short",
        ["signed char"] = "sbyte",
        ["unsigned long long"] = "ulong",
        ["unsigned long"] = "ulong",
        ["unsigned int"] = "uint",
        ["unsigned short"] = "ushort",
        ["unsigned char"] = "byte",
        ["float"] = "float",
        ["double"] = "double",
        ["bool"] = "byte",
        ["void"] = "void",
        ["ImBitArrayForNamedKeys"] = "ImBitArrayForNamedKeys"
    };

    public Dictionary<string, string> Types { get; } = [];

    public void RegisterType(string name, string friendlyName)
    {
        WellKnownTypes.Add(name, friendlyName);
    }

    public void AddTypes(JObject typesJson)
    {
        foreach (var type in typesJson)
        {
            string key = type.Key;
            string? value = type.Value?.ToString();
            if (value is not null)
            {
                if (!Types.ContainsKey(key))
                    Types.Add(key, value);
            }
        }
    }

    public bool TryResolveType(string type, out string? resolved)
    {
        int ptrCount = 0;
        while (type.EndsWith("*"))
        {
            ptrCount++;
            type = type.Substring(0, type.Length - 1);
        }

        resolved = null;

        if (WellKnownTypes.TryGetValue(type, out var knownType))
        {
            resolved = knownType + new string('*', ptrCount);
            return true;
        }

        if (Types.TryGetValue(type, out var nextType))
        {
            if (TryResolveType(nextType, out resolved))
            {
                resolved += new string('*', ptrCount);
                return true;
            }
        }

        return false;
    }
}