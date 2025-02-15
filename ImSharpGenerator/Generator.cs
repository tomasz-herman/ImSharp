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

            foreach (var file in files)
            {
                productionContext.Info("JSON file detected", $"File: {file.Path}");

                if (file.Path.EndsWith("structs_and_enums.json", StringComparison.OrdinalIgnoreCase))
                {
                    string jsonContent = file.GetText()?.ToString() ?? "{}";
                    using JsonTextReader reader = new JsonTextReader(new StringReader(jsonContent));
                    JObject jsonTypes = JObject.Load(reader);
                    var enums = jsonTypes["enums"]?
                        .Select(jt => new EnumDefinition((JProperty)jt))
                        .Where(x => x != null).ToArray();
                    if (enums is null or { Length: 0 })
                    {
                        productionContext.Report("No enums found", "No enums were detected.");
                        continue;
                    }

                    foreach (var @enum in enums)
                    {
                        productionContext.AddSource($"{@enum.FriendlyName}.g.cs", SourceText.From(@enum.GenerateSource(), Encoding.UTF8));
                    }
                }
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