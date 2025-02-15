using Newtonsoft.Json.Linq;

namespace ImSharpGenerator;

public class TypesInfo
{
    private static Dictionary<string, string> KnownTypes { get; } = new()
    {
        ["int"] = "int",
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
        ["void"] = "void"
    };

    public Dictionary<string, string> Types { get; } = [];

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
        resolved = null;
        if (KnownTypes.TryGetValue(type, out var knownType))
        {
            resolved = knownType;
            return true;
        }
        if (Types.TryGetValue(type, out var nextType)) return TryResolveType(nextType, out resolved);
        return false;
    }
}