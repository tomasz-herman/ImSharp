using Newtonsoft.Json.Linq;

namespace ImSharpGenerator;

public class StructDefinition : ISourceGenerator
{
    public string Name { get; }

    public StructDefinition(JProperty structJson)
    {
        Name = structJson.Name;
    }

    public string GenerateSource()
    {
        throw new NotImplementedException();
    }
}