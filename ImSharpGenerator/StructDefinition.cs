using Newtonsoft.Json.Linq;

namespace ImSharpGenerator;

public class StructDefinition : ISourceGenerator
{
    public string Name { get; }
    public List<StructField> Fields { get; } = [];

    public StructDefinition(JProperty structJson)
    {
        Name = structJson.Name;
        
        foreach (var field in structJson.Values())
        {
            var name = field["name"]?.ToString();
            var type = field["type"]?.ToString();
            var size = field["size"]?.ToObject<int>() ?? -1;
            var template = field["template_type"]?.ToString();
            if (name != null && type != null)
            {
                Fields.Add(new StructField(name, type, size, template));
            }
        }
    }
    
    public StructDefinition(string name, params StructField[] fields)
    {
        Name = name;
        Fields.AddRange(fields);
    }

    public string GenerateSource()
    {
        throw new NotImplementedException();
    }

    public class StructField
    {
        public string Name { get; }
        public string Type { get; }
        public int Size { get; }
        public string? TemplateType { get; }

        public StructField(string name, string type, int size, string? templateType = null)
        {
            Name = name;
            Type = type;
            Size = size;
            TemplateType = templateType;
        }

        public override string ToString()
        {
            return $"{Type} {Name}[{Size}] <{TemplateType}>";
        }
    }
}