using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ImSharpGenerator;

internal class TypesInfo
{
    private static readonly Regex ArraySuffixRegex = new(@"\[[^\]]*\]$", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> BuiltInTypes = new()
    {
        ["long long"] = "long",
        ["long long int"] = "long",
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
        ["unsigned long long int"] = "ulong",
        ["unsigned long"] = "ulong",
        ["unsigned int"] = "uint",
        ["unsigned short"] = "ushort",
        ["unsigned char"] = "byte",
        ["float"] = "float",
        ["double"] = "double",
        ["bool"] = "byte",
        ["void"] = "void",
        ["size_t"] = "nuint",
        ["ptrdiff_t"] = "nint",
        ["intptr_t"] = "nint",
        ["uintptr_t"] = "nuint",
        ["int8_t"] = "sbyte",
        ["uint8_t"] = "byte",
        ["int16_t"] = "short",
        ["uint16_t"] = "ushort",
        ["int32_t"] = "int",
        ["uint32_t"] = "uint",
        ["int64_t"] = "long",
        ["uint64_t"] = "ulong",
        ["va_list"] = "nint",
        ["FILE"] = "void",
        // imGuIZMO.quat vector/quaternion math types (identical memory layout to System.Numerics)
        ["vec3"] = "System.Numerics.Vector3",
        ["vec4"] = "System.Numerics.Vector4",
        ["Vec4"] = "System.Numerics.Vector4",
        ["quat"] = "System.Numerics.Quaternion",
        ["vgButtons"] = "int",
        ["vgModifiers"] = "int",
        // Handwritten ImGui container types
        ["ImBitArrayForNamedKeys"] = "ImBitArrayForNamedKeys",
        ["ImSmallStack__ImDrawFlags__3"] = "ImSmallStack__ImDrawFlags__3",
    };

    private static readonly HashSet<string> FixedBufferPrimitives =
    [
        "bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "char"
    ];

    // Global (core ImSharp namespace) types
    private readonly Dictionary<string, string> _coreTypes = new(BuiltInTypes);
    private readonly Dictionary<string, string> _coreTypedefs = new();
    private readonly HashSet<string> _coreStructs = new();
    private readonly HashSet<string> _coreEnums = new();

    // Library-scoped types keyed by LibraryDescriptor.Id
    private readonly Dictionary<string, Dictionary<string, string>> _scopedTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, string>> _scopedTypedefs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _scopedStructs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _scopedEnums = new(StringComparer.OrdinalIgnoreCase);

    // Opaque forward-declared structs keyed by namespace ("ImSharp", "ImSharp.NodeEditor", etc.)
    private readonly Dictionary<string, HashSet<string>> _opaqueStructsByNamespace = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, HashSet<string>> OpaqueStructsByNamespace => _opaqueStructsByNamespace;

    public static bool IsFixedBufferPrimitive(string csharpType) => FixedBufferPrimitives.Contains(csharpType);

    public bool IsEnum(string csharpType, LibraryDescriptor lib)
    {
        if (!lib.IsCoreNamespace && _scopedEnums.TryGetValue(lib.Id, out var scoped) && scoped.Contains(csharpType))
            return true;
        return _coreEnums.Contains(csharpType);
    }

    public bool IsStruct(string csharpType, LibraryDescriptor lib)
    {
        if (!lib.IsCoreNamespace && _scopedStructs.TryGetValue(lib.Id, out var scoped) && scoped.Contains(csharpType))
            return true;
        if (_coreStructs.Contains(csharpType))
            return true;
        if (_opaqueStructsByNamespace.TryGetValue(lib.Namespace, out var op1) && op1.Contains(csharpType))
            return true;
        if (_opaqueStructsByNamespace.TryGetValue("ImSharp", out var op2) && op2.Contains(csharpType))
            return true;
        return false;
    }

    public void RegisterEnum(string nativeName, string csharpName, LibraryDescriptor lib)
    {
        if (lib.IsCoreNamespace)
        {
            _coreTypes[nativeName] = csharpName;
            _coreTypes[csharpName] = csharpName;
            _coreEnums.Add(csharpName);
        }
        else
        {
            var dict = GetOrCreate(_scopedTypes, lib.Id);
            dict[nativeName] = csharpName;
            dict[csharpName] = csharpName;
            GetOrCreate(_scopedEnums, lib.Id).Add(csharpName);
        }
    }

    public void RegisterStruct(string nativeName, string csharpName, LibraryDescriptor lib)
    {
        if (lib.IsCoreNamespace)
        {
            _coreTypes[nativeName] = csharpName;
            _coreTypes[csharpName] = csharpName;
            _coreTypes[nativeName + "_c"] = csharpName;
            _coreStructs.Add(csharpName);
        }
        else
        {
            var dict = GetOrCreate(_scopedTypes, lib.Id);
            dict[nativeName] = csharpName;
            dict[csharpName] = csharpName;
            dict[nativeName + "_c"] = csharpName;
            GetOrCreate(_scopedStructs, lib.Id).Add(csharpName);
        }

        if (_opaqueStructsByNamespace.TryGetValue(lib.Namespace, out var ops))
            ops.Remove(csharpName);
    }

    public void AddTypedefs(JObject typesJson, LibraryDescriptor lib)
    {
        var target = lib.IsCoreNamespace ? _coreTypedefs : GetOrCreate(_scopedTypedefs, lib.Id);
        foreach (var kv in typesJson)
        {
            string key = kv.Key;
            string? value = kv.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (value == $"struct {key}" || value == $"enum {key}" || value == $"union {key}")
                continue;

            if (!target.ContainsKey(key))
                target[key] = value!;
        }
    }

    public string ResolveType(string rawType, LibraryDescriptor lib, bool allowOpaque = true)
    {
        if (TryResolveType(rawType, lib, out var resolved, allowOpaque, depth: 0) && resolved != null)
            return resolved;

        return "nint";
    }

    public bool TryResolveType(string rawType, LibraryDescriptor lib, out string? resolved, bool allowOpaque = true, int depth = 0)
    {
        resolved = null;
        if (depth > 16 || string.IsNullOrWhiteSpace(rawType))
            return false;

        string type = CleanType(rawType);

        // Function pointer types
        if (type.Contains("(*"))
        {
            resolved = "nint";
            return true;
        }

        // Convert C array parameter syntax to pointer
        int arrayPtrs = 0;
        while (ArraySuffixRegex.IsMatch(type))
        {
            type = ArraySuffixRegex.Replace(type, "").Trim();
            arrayPtrs++;
        }

        // Count trailing pointers / references
        int ptrCount = arrayPtrs;
        while (true)
        {
            type = StripQualifiers(type);
            if (type.EndsWith("*"))
            {
                ptrCount++;
                type = type.Substring(0, type.Length - 1).Trim();
            }
            else if (type.EndsWith("&"))
            {
                ptrCount++;
                type = type.Substring(0, type.Length - 1).Trim();
            }
            else
            {
                break;
            }
        }

        type = StripQualifiers(type);

        // Handle cimgui template instantiations
        if (TryResolveTemplateType(type, lib, out var templateResolved))
        {
            resolved = templateResolved + new string('*', ptrCount);
            return true;
        }

        // 1. Scoped lookup first (for sub-namespace libraries)
        if (!lib.IsCoreNamespace && _scopedTypes.TryGetValue(lib.Id, out var scopedMap) && scopedMap.TryGetValue(type, out var scopedKnown))
        {
            resolved = scopedKnown + new string('*', ptrCount);
            return true;
        }

        // 2. Core lookup (primitives, ImGui/ImPlot/ImPlot3D/ImNodes/ImGuizmo types)
        if (_coreTypes.TryGetValue(type, out var coreKnown))
        {
            resolved = coreKnown + new string('*', ptrCount);
            return true;
        }

        // 3. Scoped typedef lookup
        if (!lib.IsCoreNamespace && _scopedTypedefs.TryGetValue(lib.Id, out var scopedTd) && scopedTd.TryGetValue(type, out var scopedNext))
        {
            if (TryResolveType(scopedNext, lib, out var target, allowOpaque, depth + 1) && target != null)
            {
                resolved = target + new string('*', ptrCount);
                return true;
            }
        }

        // 4. Core typedef lookup
        if (_coreTypedefs.TryGetValue(type, out var coreNext))
        {
            if (TryResolveType(coreNext, lib, out var target, allowOpaque, depth + 1) && target != null)
            {
                resolved = target + new string('*', ptrCount);
                return true;
            }
        }

        // 5. Handle "_c" POD suffix (e.g. ImVec2_c, ImPlot3DPoint_c)
        if (type.EndsWith("_c", StringComparison.Ordinal))
        {
            string baseName = type.Substring(0, type.Length - 2);
            if (TryResolveType(baseName, lib, out var baseKnown, allowOpaque: false, depth + 1) && baseKnown != null)
            {
                resolved = baseKnown + new string('*', ptrCount);
                return true;
            }
        }

        // 6. Handle "Ptr" suffix typedefs like ImGuiWindowPtr -> ImGuiWindow*
        if (type.EndsWith("Ptr", StringComparison.Ordinal) && type.Length > 3)
        {
            string baseName = type.Substring(0, type.Length - 3);
            if (TryResolveType(baseName, lib, out var baseTarget, allowOpaque, depth + 1) && baseTarget != null)
            {
                resolved = baseTarget + new string('*', ptrCount + 1);
                return true;
            }
        }

        // 7. Opaque forward-declared struct referenced via pointer
        if (allowOpaque && ptrCount > 0 && IsValidCIdentifier(type))
        {
            bool existsInCore = _coreStructs.Contains(type) || _coreEnums.Contains(type);
            bool existsInScoped = !lib.IsCoreNamespace &&
                ((_scopedStructs.TryGetValue(lib.Id, out var ss) && ss.Contains(type)) ||
                 (_scopedEnums.TryGetValue(lib.Id, out var se) && se.Contains(type)));

            if (!existsInCore && !existsInScoped)
            {
                GetOrCreate(_opaqueStructsByNamespace, lib.Namespace).Add(type);
            }
            resolved = type + new string('*', ptrCount);
            return true;
        }

        return false;
    }

    private bool TryResolveTemplateType(string type, LibraryDescriptor lib, out string? resolved)
    {
        resolved = null;

        if (type.StartsWith("ImVector_", StringComparison.Ordinal))
        {
            string elemRaw = type.Substring("ImVector_".Length);
            resolved = ResolveContainerType("ImVector", elemRaw, lib, pointerFallback: "ImVectorPtr");
            return true;
        }

        if (type.StartsWith("ImSpan_", StringComparison.Ordinal))
        {
            string elemRaw = type.Substring("ImSpan_".Length);
            resolved = ResolveContainerType("ImSpan", elemRaw, lib, pointerFallback: "ImSpan<nint>");
            return true;
        }

        if (type.StartsWith("ImChunkStream_", StringComparison.Ordinal))
        {
            resolved = "ImChunkStream";
            return true;
        }

        if (type.StartsWith("ImPool_", StringComparison.Ordinal))
        {
            string elemRaw = type.Substring("ImPool_".Length);
            resolved = ResolveContainerType("ImPool", elemRaw, lib, pointerFallback: "ImPool<nint>");
            return true;
        }

        if (type.StartsWith("ImStableVector_", StringComparison.Ordinal))
        {
            string rest = type.Substring("ImStableVector_".Length);
            int sep = rest.IndexOf("__", StringComparison.Ordinal);
            string elemRaw = sep > 0 ? rest.Substring(0, sep) : rest;
            resolved = ResolveContainerType("ImStableVector", elemRaw, lib, pointerFallback: "ImStableVector<nint>");
            return true;
        }

        if (type.StartsWith("ImBitArray_", StringComparison.Ordinal))
        {
            resolved = "ImBitArrayForNamedKeys";
            return true;
        }

        return false;
    }

    private string ResolveContainerType(string containerName, string elemRaw, LibraryDescriptor lib, string pointerFallback)
    {
        if (elemRaw.EndsWith("Ptr", StringComparison.Ordinal) || elemRaw.Contains("*"))
            return pointerFallback;

        string normalizedElem = elemRaw.Replace("_", " ");
        if (BuiltInTypes.TryGetValue(normalizedElem, out var builtInElem))
        {
            return builtInElem == "void" ? pointerFallback : $"{containerName}<{builtInElem}>";
        }

        if (TryResolveType(elemRaw, lib, out var resolvedElem, allowOpaque: true, depth: 1) && resolvedElem != null)
        {
            if (resolvedElem.Contains("*") || resolvedElem == "void")
                return pointerFallback;
            return $"{containerName}<{resolvedElem}>";
        }

        return pointerFallback;
    }

    private static TVal GetOrCreate<TKey, TVal>(Dictionary<TKey, TVal> dict, TKey key)
        where TKey : notnull
        where TVal : new()
    {
        if (!dict.TryGetValue(key, out var val))
        {
            val = new TVal();
            dict[key] = val;
        }
        return val;
    }

    private static string CleanType(string rawType) =>
        rawType.Replace("\r", " ").Replace("\n", " ").Trim();

    private static string StripQualifiers(string type)
    {
        while (true)
        {
            type = type.Trim();
            if (type.StartsWith("const ", StringComparison.Ordinal))
                type = type.Substring(6);
            else if (type.StartsWith("volatile ", StringComparison.Ordinal))
                type = type.Substring(9);
            else if (type.StartsWith("struct ", StringComparison.Ordinal))
                type = type.Substring(7);
            else if (type.StartsWith("enum ", StringComparison.Ordinal))
                type = type.Substring(5);
            else if (type.EndsWith(" const", StringComparison.Ordinal))
                type = type.Substring(0, type.Length - 6);
            else if (type.EndsWith(" volatile", StringComparison.Ordinal))
                type = type.Substring(0, type.Length - 9);
            else
                break;
        }
        return type.Trim();
    }

    private static bool IsValidCIdentifier(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (!char.IsLetter(s[0]) && s[0] != '_') return false;
        for (int i = 1; i < s.Length; i++)
        {
            if (!char.IsLetterOrDigit(s[i]) && s[i] != '_')
                return false;
        }
        return true;
    }
}
