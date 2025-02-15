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
        using var writer = new CSharpWriter();

        writer.WriteLineIndented("namespace ImSharp");
        using (writer.WriteBlock())
        {
            if (Flags)
            {
                writer.WriteLineIndented("[Flags]");
            }
            
            writer.WriteLineIndented($"public enum {Name}");
            using (writer.WriteBlock())
            {
                foreach (var keyValue in Values)
                {
                    writer.WriteLineIndented($"{keyValue.Key} = {keyValue.Value},");
                }
            }
        }

        return writer.ToString();
    }
}