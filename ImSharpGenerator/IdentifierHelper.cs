using System.Text;

namespace ImSharpGenerator;

internal sealed class LibraryDescriptor
{
    public string Id { get; }
    public string Namespace { get; }
    public string ClassName { get; }
    public string NativeClassName => $"{ClassName}Native";
    public string PrefixStrip { get; }
    public bool IsCoreNamespace => Namespace == "ImSharp";

    public LibraryDescriptor(string id, string @namespace, string className, string prefixStrip)
    {
        Id = id;
        Namespace = @namespace;
        ClassName = className;
        PrefixStrip = prefixStrip;
    }

    private static readonly Dictionary<string, LibraryDescriptor> KnownLibraries = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cimgui"]          = new("cimgui",          "ImSharp",               "ImGui",           "ig"),
        ["cimplot"]         = new("cimplot",         "ImSharp",               "ImPlot",          "ImPlot_"),
        ["cimplot3d"]       = new("cimplot3d",       "ImSharp",               "ImPlot3D",        "ImPlot3D_"),
        ["cimnodes"]        = new("cimnodes",        "ImSharp",               "ImNodes",         "imnodes_"),
        ["cimnodes_r"]      = new("cimnodes_r",      "ImSharp.NodesR",        "ImNodesR",        "ImNodes_"),
        ["cimnodes_editor"] = new("cimnodes_editor", "ImSharp.NodeEditor",    "ImNodeEditor",    "ax_NodeEditor_"),
        ["cimguizmo"]       = new("cimguizmo",       "ImSharp",               "ImGuizmo",        "ImGuizmo_"),
        ["cimguizmo_quat"]  = new("cimguizmo_quat",  "ImSharp.GuizmoQuat",    "ImGuizmoQuat",    "ig"),
        ["cimcte"]          = new("cimcte",          "ImSharp.ColorTextEdit", "ImColorTextEdit", "TextEditor_"),
    };

    public static LibraryDescriptor DetectFromPath(string filePath)
    {
        var normalized = filePath.Replace('\\', '/');
        var segments = normalized.Split('/');

        // Look for "<lib>-src" segment under _deps/ or direct directory name matching a known library
        foreach (var seg in segments)
        {
            string candidate = seg.EndsWith("-src", StringComparison.OrdinalIgnoreCase)
                ? seg.Substring(0, seg.Length - 4)
                : seg;

            if (KnownLibraries.TryGetValue(candidate, out var desc))
                return desc;
        }

        // Extensible fallback: automatically support any future cimgui-family repo under _deps/<name>-src
        foreach (var seg in segments)
        {
            if (seg.EndsWith("-src", StringComparison.OrdinalIgnoreCase))
            {
                string id = seg.Substring(0, seg.Length - 4).ToLowerInvariant();
                string pascal = ToPascalCase(id.StartsWith("cim", StringComparison.Ordinal) ? id.Substring(1) : id);
                return new LibraryDescriptor(id, $"ImSharp.{pascal}", pascal, $"{pascal}_");
            }
        }

        return KnownLibraries["cimgui"];
    }

    private static string ToPascalCase(string raw)
    {
        var sb = new StringBuilder();
        bool upperNext = true;
        foreach (char c in raw)
        {
            if (c == '_' || c == '-')
            {
                upperNext = true;
                continue;
            }
            sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
            upperNext = false;
        }
        return sb.Length > 0 ? sb.ToString() : "Custom";
    }
}

internal static class IdentifierHelper
{
    private static readonly HashSet<string> CSharpKeywords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
        "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
        "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual",
        "void", "volatile", "while"
    ];

    public static string EscapeIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "_unnamed";

        int bracketIdx = name.IndexOf('[');
        if (bracketIdx >= 0)
            name = name.Substring(0, bracketIdx);

        // Strip any stray non-identifier characters (e.g. trailing '=' from upstream parser quirks)
        var cleaned = new StringBuilder(name.Length);
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                cleaned.Append(c);
        }
        name = cleaned.ToString();
        if (string.IsNullOrEmpty(name))
            return "_unnamed";

        if (char.IsDigit(name[0]))
            name = "_" + name;

        return CSharpKeywords.Contains(name) ? "@" + name : name;
    }

    public static string SanitizeFileName(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (char c in name)
        {
            sb.Append(char.IsLetterOrDigit(c) || c == '_' || c == '.' ? c : '_');
        }
        return sb.ToString();
    }
}
