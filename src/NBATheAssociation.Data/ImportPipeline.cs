using System.Text.Json;

namespace NBATheAssociation.Data;

public sealed record ImportStageResult<T>(T? Value, IReadOnlyList<ValidationIssue> Issues)
{
    public bool IsSuccess => Value is not null && Issues.All(x => x.Severity != ValidationSeverity.Error);
}

public interface IRawPackageParser<T> { ImportStageResult<IReadOnlyList<T>> Parse(string raw); }
public interface IPackageNormalizer<T> { ImportStageResult<V2PackageDto> Normalize(IReadOnlyList<T> records); }

public sealed record PackageImportResult(V2PackageDto? Package, string? CanonicalJson, IReadOnlyList<ValidationIssue> Issues)
{
    public bool IsSuccess => Package is not null && CanonicalJson is not null && Issues.All(x => x.Severity != ValidationSeverity.Error);
}

public static class PackageImportPipeline
{
    public static PackageImportResult Run<T>(string raw, IRawPackageParser<T> parser, IPackageNormalizer<T> normalizer)
    {
        var parsed = parser.Parse(raw); if (!parsed.IsSuccess) return new(null, null, parsed.Issues);
        var normalized = normalizer.Normalize(parsed.Value!); if (!normalized.IsSuccess) return new(null, null, normalized.Issues);
        var validation = V2PackageValidator.Validate(normalized.Value!);
        var issues = normalized.Issues.Concat(validation.Issues).ToArray();
        return validation.IsValid ? new(normalized.Value, V2PackageSerializer.Serialize(normalized.Value!), issues) : new(null, null, issues);
    }
}

public sealed record SyntheticRawRecord(string Kind, Dictionary<string, string?> Fields);

public sealed class SyntheticRawJsonParser : IRawPackageParser<SyntheticRawRecord>
{
    public ImportStageResult<IReadOnlyList<SyntheticRawRecord>> Parse(string raw)
    {
        try
        {
            var value = JsonSerializer.Deserialize<List<SyntheticRawRecord>>(raw, V2PackageSerializer.Options);
            return value is null ? Fail("import.parse", "Raw record array is required.") : new(value, []);
        }
        catch (JsonException ex) { return Fail("import.parse", ex.Message); }
    }
    private static ImportStageResult<IReadOnlyList<SyntheticRawRecord>> Fail(string code, string message) => new(null, [new(code, ValidationSeverity.Error, "$", message)]);
}

// Synthetic-only adapter used to prove stage boundaries; it intentionally supports a tiny vocabulary.
public sealed class SyntheticPackageNormalizer(DateTimeOffset createdAtUtc) : IPackageNormalizer<SyntheticRawRecord>
{
    public ImportStageResult<V2PackageDto> Normalize(IReadOnlyList<SyntheticRawRecord> records)
    {
        var template = records.FirstOrDefault(x => x.Kind == "canonicalPackageJson")?.Fields.GetValueOrDefault("json");
        if (string.IsNullOrWhiteSpace(template))
            return new(null, [new("import.normalize", ValidationSeverity.Error, "$", "Synthetic input requires one canonicalPackageJson record.")]);
        try
        {
            var package = JsonSerializer.Deserialize<V2PackageDto>(template, V2PackageSerializer.Options);
            if (package?.Manifest is null) return new(null, [new("import.normalize", ValidationSeverity.Error, "$", "Embedded package is malformed.")]);
            return new(package with { Manifest = package.Manifest with { CreatedAtUtc = createdAtUtc.ToUniversalTime() } }, []);
        }
        catch (JsonException ex) { return new(null, [new("import.normalize", ValidationSeverity.Error, "$", ex.Message)]); }
    }
}

