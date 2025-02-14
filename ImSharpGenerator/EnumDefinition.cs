using System.Text;
using Newtonsoft.Json.Linq;

namespace ImSharpGenerator;

public class EnumDefinition
{
    public bool Flags { get; set; }
    public string Name { get; set; }
    public IDictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

    public EnumDefinition(JProperty enumJson)
    {
        Name = enumJson.Name;
        Flags = Name.Contains("Flags");
        foreach (var field in enumJson.Values())
        {
            var name = field["name"]?.ToString();
            var value = field["calc_value"]?.ToString();
            if (name != null && value != null) Values.Add(name, value);
        }
    }

    public string GenerateSource()
    {
        StringBuilder sb = new();

        sb.AppendLine("namespace ImSharp");
        sb.AppendLine("{");

        if (Flags)
        {
            sb.AppendLine("    [System.Flags]");
        }

        sb.AppendLine($"    public enum {Name}");
        sb.AppendLine("    {");

        foreach (var keyValue in Values)
        {
            sb.AppendLine($"        {keyValue.Key} = {keyValue.Value},");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}