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
            .Where(file => file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Collect();

        context.RegisterSourceOutput(jsonFiles, (pCtx, files) =>
        {
            if (files.IsEmpty)
            {
                pCtx.Warning("No JSON files found", "No additional .json files were detected. Ensure ImSharpNative has been built.");
                return;
            }

            var typesInfo = new TypesInfo();
            var enums = new List<EnumDefinition>();
            var structs = new List<StructDefinition>();
            var functionsByLibrary = new Dictionary<string, (LibraryDescriptor Lib, List<FunctionDefinition> Funcs)>(StringComparer.OrdinalIgnoreCase);

            // 1. Load typedefs_dict.json from all libraries
            foreach (var file in files.Where(f => f.Path.EndsWith("typedefs_dict.json", StringComparison.OrdinalIgnoreCase)))
            {
                var lib = LibraryDescriptor.DetectFromPath(file.Path);
                JObject json = ParseJson(file);
                typesInfo.AddTypedefs(json, lib);
            }

            // 2. Load structs_and_enums.json from all libraries
            var seenEnums = new HashSet<string>(StringComparer.Ordinal);
            var seenStructs = new HashSet<string>(StringComparer.Ordinal);

            foreach (var file in files.Where(f => f.Path.EndsWith("structs_and_enums.json", StringComparison.OrdinalIgnoreCase)))
            {
                var lib = LibraryDescriptor.DetectFromPath(file.Path);
                JObject json = ParseJson(file);

                var enumUnderlyingTypes = new Dictionary<string, string>(StringComparer.Ordinal);
                if (json["enumtypes"] is JObject enumTypesObj)
                {
                    foreach (var kv in enumTypesObj)
                    {
                        if (kv.Value != null)
                        {
                            string resolvedUnderlying = typesInfo.ResolveType(kv.Value.ToString(), lib, allowOpaque: false);
                            enumUnderlyingTypes[kv.Key] = resolvedUnderlying;
                        }
                    }
                }

                if (json["enums"] is JObject enumsObj)
                {
                    foreach (var prop in enumsObj.Properties())
                    {
                        string keyNoUnderscore = prop.Name.TrimEnd('_');
                        enumUnderlyingTypes.TryGetValue(keyNoUnderscore, out var underlying);
                        var enumDef = new EnumDefinition(prop, lib, underlying);
                        string fullKey = $"{lib.Namespace}.{enumDef.FriendlyName}";
                        if (seenEnums.Add(fullKey))
                        {
                            enums.Add(enumDef);
                            typesInfo.RegisterEnum(enumDef.Name, enumDef.FriendlyName, lib);
                        }
                    }
                }

                if (json["structs"] is JObject structsObj)
                {
                    foreach (var prop in structsObj.Properties())
                    {
                        var structDef = new StructDefinition(prop, lib);
                        string fullKey = $"{lib.Namespace}.{structDef.FriendlyName}";
                        if (seenStructs.Add(fullKey))
                        {
                            structs.Add(structDef);
                            typesInfo.RegisterStruct(structDef.NativeName, structDef.FriendlyName, lib);
                        }
                    }
                }
            }

            // 3. Load definitions.json from all libraries
            foreach (var file in files.Where(f => f.Path.EndsWith("definitions.json", StringComparison.OrdinalIgnoreCase)))
            {
                var fileName = Path.GetFileName(file.Path);
                if (!fileName.Equals("definitions.json", StringComparison.OrdinalIgnoreCase))
                    continue;

                var lib = LibraryDescriptor.DetectFromPath(file.Path);
                if (!functionsByLibrary.TryGetValue(lib.Id, out var entry))
                {
                    entry = (lib, []);
                    functionsByLibrary[lib.Id] = entry;
                }

                JObject json = ParseJson(file);
                foreach (var prop in json.Properties())
                {
                    if (prop.Value is JArray overloads)
                    {
                        foreach (var ov in overloads.OfType<JObject>())
                        {
                            entry.Funcs.Add(new FunctionDefinition(ov, lib));
                        }
                    }
                }
            }

            // 4. Emit enums
            foreach (var @enum in enums)
            {
                string hint = $"Enum.{IdentifierHelper.SanitizeFileName(@enum.Library.Namespace)}.{IdentifierHelper.SanitizeFileName(@enum.FriendlyName)}.g.cs";
                pCtx.AddSource(hint, SourceText.From(@enum.GenerateSource(), Encoding.UTF8));
            }

            // 5. Emit structs
            foreach (var @struct in structs)
            {
                string hint = $"Struct.{IdentifierHelper.SanitizeFileName(@struct.Library.Namespace)}.{IdentifierHelper.SanitizeFileName(@struct.FriendlyName)}.g.cs";
                pCtx.AddSource(hint, SourceText.From(@struct.GenerateSource(typesInfo), Encoding.UTF8));
            }

            // 6. Emit native P/Invoke classes and idiomatic C# wrapper classes per library
            foreach (var kv in functionsByLibrary)
            {
                var (lib, funcs) = kv.Value;

                string nativeSrc = FunctionCodeGenerator.GenerateNativeClass(lib, funcs, typesInfo);
                pCtx.AddSource($"{IdentifierHelper.SanitizeFileName(lib.NativeClassName)}.g.cs",
                    SourceText.From(nativeSrc, Encoding.UTF8));

                string wrapperSrc = FunctionCodeGenerator.GenerateWrapperClass(lib, funcs, typesInfo);
                pCtx.AddSource($"{IdentifierHelper.SanitizeFileName(lib.ClassName)}.g.cs",
                    SourceText.From(wrapperSrc, Encoding.UTF8));
            }

            // 7. Emit opaque/forward-declared structs + safe *Ptr wrappers per namespace
            foreach (var kv in typesInfo.OpaqueStructsByNamespace)
            {
                if (kv.Value.Count == 0)
                    continue;

                using var writer = new CSharpWriter();
                writer.WriteAutoGeneratedHeader();
                writer.WriteLineIndented("using System;");
                writer.WriteLineIndented("using System.Runtime.CompilerServices;");
                writer.WriteLine();
                writer.WriteLineIndented($"namespace {kv.Key};");
                writer.WriteLine();
                foreach (var opaqueName in kv.Value.OrderBy(s => s, StringComparer.Ordinal))
                {
                    writer.WriteLineIndented($"public partial struct {opaqueName} {{ }}");
                    if (!opaqueName.EndsWith("Ptr", StringComparison.Ordinal))
                    {
                        StructDefinition.EmitSafePtrStruct(writer, opaqueName, null, null, null);
                    }
                    writer.WriteLine();
                }
                pCtx.AddSource($"OpaqueStructs.{IdentifierHelper.SanitizeFileName(kv.Key)}.g.cs",
                    SourceText.From(writer.ToString(), Encoding.UTF8));
            }
        });
    }

    private static JObject ParseJson(AdditionalText file)
    {
        string jsonContent = file.GetText()?.ToString() ?? "{}";
        using var reader = new JsonTextReader(new StringReader(jsonContent));
        return JObject.Load(reader);
    }
}

internal static class ContextExtensions
{
    public static void Hidden(this SourceProductionContext ctx, string title, string message)
    {
        ctx.Report(title, message, "Info", DiagnosticSeverity.Hidden);
    }

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
            "IMSHARP001", title, message,
            category, severity, true);
        ctx.ReportDiagnostic(Diagnostic.Create(description, Location.None));
    }
}
