using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImSharpGenerator;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var jsonFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).Collect();

        context.RegisterSourceOutput(jsonFiles, (productionContext, files) =>
        {
            if (files.IsEmpty)
            {
                productionContext.Warning("No JSON files found", "No additional .json files were detected.");
            }

            List<EnumDefinition> enums = [];
            List<StructDefinition> structs = [];
            TypesInfo typesInfo = new TypesInfo();
            foreach (var file in files.Where(file => file.Path.EndsWith("typedefs_dict.json", StringComparison.OrdinalIgnoreCase)))
            {
                productionContext.Info("JSON file detected", $"File: {file.Path}");
                
                string jsonContent = file.GetText()?.ToString() ?? "{}";
                using JsonTextReader reader = new JsonTextReader(new StringReader(jsonContent));
                JObject jsonTypes = JObject.Load(reader);
                typesInfo.AddTypes(jsonTypes);
            }
            
            foreach (var file in files.Where(file => file.Path.EndsWith("structs_and_enums.json", StringComparison.OrdinalIgnoreCase)))
            {
                productionContext.Info("JSON file detected", $"File: {file.Path}");
                
                string jsonContent = file.GetText()?.ToString() ?? "{}";
                using JsonTextReader reader = new JsonTextReader(new StringReader(jsonContent));
                JObject jsonTypes = JObject.Load(reader);
                enums.AddRange(jsonTypes["enums"]?.Select(jt => new EnumDefinition((JProperty)jt)) ?? []);
                structs.AddRange(jsonTypes["structs"]?.Select(jt => new StructDefinition((JProperty)jt)) ?? []);
            }

            foreach (var @enum in enums)
            {
                productionContext.AddSource($"{@enum.FriendlyName}.g.cs", SourceText.From(@enum.GenerateSource(), Encoding.UTF8));
            }
            
            foreach (var @struct in structs)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"// {@struct.Name}({@struct.Fields.Count}):");
                foreach (var field in @struct.Fields)
                {
                    typesInfo.TryResolveType(field.Type, out var resolved);
                    builder.AppendLine($"// - [{resolved}] {field}");
                }
                productionContext.AddSource($"{@struct.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
            }
        });
    }
}

internal static class ContextExtensions
{
    public static void Info(this SourceProductionContext ctx, string title, string message)
    {
        ctx.Report(title, message, "Info", DiagnosticSeverity.Info);
    }
    
    public static void Warning(this SourceProductionContext ctx, string title, string message)
    {
        ctx.Report(title, message, "Warning");
    }
    
    public static void Error(this SourceProductionContext ctx, string title, string message)
    {
        ctx.Report(title, message, "Error", DiagnosticSeverity.Error);
    }
    
    public static void Report(this SourceProductionContext ctx, string title, string message, string category = "Debug", DiagnosticSeverity severity = DiagnosticSeverity.Warning)
    {
        var description = new DiagnosticDescriptor(
            "ImSharpGenerator", title, message,
            category, severity, true);
        ctx.ReportDiagnostic(Diagnostic.Create(description, Location.None));
    }
}