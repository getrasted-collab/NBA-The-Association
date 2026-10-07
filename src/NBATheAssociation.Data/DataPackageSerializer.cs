using System.Text.Json;
using NBATheAssociation.Core;

namespace NBATheAssociation.Data;

public sealed record PackageLoadResult(DataPackageDto? Package, LeagueWorld? World, IReadOnlyList<ValidationIssue> Issues)
{
    public bool IsSuccess => World is not null && Issues.All(x => x.Severity != ValidationSeverity.Error);
}

public static class DataPackageSerializer
{
    public const int SupportedSchemaVersion = 1;
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        WriteIndented = true
    };

    public static PackageLoadResult Load(string json)
    {
        JsonDocument document;
        try { document = JsonDocument.Parse(json); }
        catch (JsonException ex) { return Failure("schema.malformed", "$", ex.Message); }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("schemaVersion", out var schema))
                return Failure("schema.missing", "$.schemaVersion", "schemaVersion is required.");
            if (!schema.TryGetInt32(out var version) || version <= 0)
                return Failure("schema.malformed", "$.schemaVersion", "schemaVersion must be a positive integer.");
            if (version != SupportedSchemaVersion)
                return Failure("schema.unsupported", "$.schemaVersion", $"Schema version {version} is unsupported; supported version is {SupportedSchemaVersion}.");
        }

        DataPackageDto? package;
        try { package = JsonSerializer.Deserialize<DataPackageDto>(json, Options); }
        catch (JsonException ex) { return Failure("schema.malformed", "$", ex.Message); }
        if (package is null) return Failure("schema.malformed", "$", "Package cannot be null.");

        var validation = DataPackageValidator.Validate(package);
        if (!validation.IsValid) return new(package, null, validation.Issues);
        return new(package, DataPackageMaterializer.Materialize(package), validation.Issues);
    }

    public static string Serialize(DataPackageDto package) => JsonSerializer.Serialize(package, Options);

    private static PackageLoadResult Failure(string code, string path, string message) =>
        new(null, null, [new ValidationIssue(code, ValidationSeverity.Error, path, message)]);
}

