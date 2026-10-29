using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Test.SchemaContract;

// C14 schema-first support. The committed .spec/OpenApi/*.json documents ARE the contract for the web and
// community domains: the controllers were written to match them, so the risk this guards is silent DRIFT — a DTO
// property renamed, added or dropped without the contract moving with it.
//
// The check is deliberately BIDIRECTIONAL. Contract-implies-CLR alone would let a new DTO property ship without
// ever appearing in the contract; CLR-implies-contract alone would let a contract keep describing a property the
// DTO no longer has. Both directions together are what make the document trustworthy.
internal static class OpenApiContract
{
    // The .spec folder lives under the WebApi project, not the test one, so walk up from the test assembly until
    // the folder itself appears. Anchoring on the target rather than on a marker file matters here: this repo has
    // TWO .slnx files (root and API/), so "walk up to the nearest .slnx" stops one level short and looks in API/API.
    private static readonly string SpecFolder =
        Path.Combine("API", "CertificateOfOrigins.WebApi", ".spec", "OpenApi");

    internal static JsonDocument Load(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, SpecFolder)))
        {
            dir = dir.Parent;
        }

        Assert.That(dir, Is.Not.Null,
            $"could not locate {SpecFolder} walking up from {AppContext.BaseDirectory}");

        var path = Path.Combine(dir!.FullName, SpecFolder, fileName);
        Assert.That(File.Exists(path), Is.True, $"contract document is missing: {path}");
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    // The property names the contract declares for one schema, in the order the document lists them.
    internal static IReadOnlyList<string> SchemaProperties(JsonDocument doc, string schemaName)
    {
        var schemas = doc.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.That(schemas.TryGetProperty(schemaName, out var schema), Is.True,
            $"the contract does not describe a schema named '{schemaName}'");

        if (!schema.TryGetProperty("properties", out var properties))
        {
            return [];
        }

        return properties.EnumerateObject().Select(p => p.Name).ToList();
    }

    // The property names the CLR type serialises, under the same camelCase policy the API uses on the wire.
    internal static IReadOnlyList<string> ClrProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() is null)
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                         ?? JsonNamingPolicy.CamelCase.ConvertName(p.Name))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();
    }

    // One schema against one CLR type, both directions, with a message that names the drifting properties rather
    // than just reporting that two sets differ.
    internal static void AssertMatches(JsonDocument doc, string schemaName, Type type)
    {
        var contract = SchemaProperties(doc, schemaName).OrderBy(n => n, StringComparer.Ordinal).ToList();
        var clr = ClrProperties(type);

        var missingFromContract = clr.Except(contract, StringComparer.Ordinal).ToList();
        var missingFromClr = contract.Except(clr, StringComparer.Ordinal).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(missingFromContract, Is.Empty,
                $"{type.Name} has properties the contract does not declare: {string.Join(", ", missingFromContract)}. "
                + "Either add them to the contract, or mark them [JsonIgnore] if they are not part of the wire shape.");
            Assert.That(missingFromClr, Is.Empty,
                $"the contract declares properties {type.Name} does not have: {string.Join(", ", missingFromClr)}. "
                + "The DTO changed without the contract following it.");
        });
    }
}
