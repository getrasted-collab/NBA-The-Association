using System.Text.Json;
using NBATheAssociation.Core;

namespace NBATheAssociation.Data;

public sealed record V2PackageLoadResult(V2PackageDto? Package, LeagueWorld? World, IReadOnlyList<ValidationIssue> Issues)
{
    public bool IsSuccess => World is not null && Issues.All(x => x.Severity != ValidationSeverity.Error);
}

public static class V2PackageSerializer
{
    public const int SupportedSchemaVersion = 2;
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        WriteIndented = true
    };

    public static V2PackageLoadResult Load(string json)
    {
        JsonDocument document;
        try { document = JsonDocument.Parse(json); }
        catch (JsonException ex) { return Failure("manifest.malformed", "$", ex.Message); }
        using (document)
        {
            if (!document.RootElement.TryGetProperty("manifest", out var manifest)) return Failure("manifest.missing", "$.manifest", "manifest is required.");
            if (manifest.ValueKind != JsonValueKind.Object || !manifest.TryGetProperty("schemaVersion", out var schema) || !schema.TryGetInt32(out var version))
                return Failure("manifest.malformed", "$.manifest.schemaVersion", "A numeric schemaVersion is required.");
            if (version != SupportedSchemaVersion)
                return Failure("schema.unsupported", "$.manifest.schemaVersion", $"Schema version {version} is unsupported; supported version is {SupportedSchemaVersion}.");
        }

        V2PackageDto? package;
        try { package = JsonSerializer.Deserialize<V2PackageDto>(json, Options); }
        catch (JsonException ex) { return Failure("manifest.malformed", "$", ex.Message); }
        if (package is null) return Failure("manifest.malformed", "$", "Package cannot be null.");
        var validation = V2PackageValidator.Validate(package);
        if (!validation.IsValid) return new(package, null, validation.Issues);
        return new(package, V2PackageMaterializer.Materialize(package), validation.Issues);
    }

    public static string Serialize(V2PackageDto package) => JsonSerializer.Serialize(V2PackageCanonicalizer.Canonicalize(package), Options);

    private static V2PackageLoadResult Failure(string code, string path, string message) =>
        new(null, null, [new ValidationIssue(code, ValidationSeverity.Error, path, message)]);
}

internal static class V2PackageCanonicalizer
{
    public static V2PackageDto Canonicalize(V2PackageDto p)
    {
        static string G(string value) => Guid.TryParse(value, out var id) ? id.ToString("D").ToLowerInvariant() : value;
        var d = p.Data;
        return p with
        {
            Manifest = p.Manifest is null ? null : p.Manifest with
            {
                PackageId = G(p.Manifest.PackageId),
                CreatedAtUtc = p.Manifest.CreatedAtUtc.ToUniversalTime(),
                SeasonIds = p.Manifest.SeasonIds?.Select(G).Order(StringComparer.Ordinal).ToList(),
                ProvenanceIds = p.Manifest.ProvenanceIds?.Select(G).Order(StringComparer.Ordinal).ToList(),
                ContainedCategories = p.Manifest.ContainedCategories?.Order(StringComparer.Ordinal).ToList()
            },
            Provenance = p.Provenance?.Select(x => x with { Id = G(x.Id), ImportedAtUtc = x.ImportedAtUtc.ToUniversalTime(), RetrievedAtUtc = x.RetrievedAtUtc?.ToUniversalTime() }).OrderBy(x => x.Id).ToList(),
            ExternalIdMappings = p.ExternalIdMappings?.Select(x => x with { InternalId = G(x.InternalId), ProvenanceId = x.ProvenanceId is null ? null : G(x.ProvenanceId) })
                .OrderBy(x => x.SourceName).ThenBy(x => x.EntityType).ThenBy(x => x.ExternalIdentifier).ToList(),
            Data = d is null ? null : d with
            {
                Leagues = d.Leagues?.Select(x => x with { Id = G(x.Id) }).OrderBy(x => x.Id).ToList(),
                RuleSets = d.RuleSets?.Select(x => x with { Id = G(x.Id) }).OrderBy(x => x.Id).ToList(),
                Seasons = d.Seasons?.Select(x => x with { Id = G(x.Id), LeagueId = G(x.LeagueId), RuleSetId = G(x.RuleSetId) }).OrderBy(x => x.Id).ToList(),
                Franchises = d.Franchises?.Select(x => x with { Id = G(x.Id) }).OrderBy(x => x.Id).ToList(),
                TeamSeasons = d.TeamSeasons?.Select(x => x with { Id = G(x.Id), FranchiseId = G(x.FranchiseId), SeasonId = G(x.SeasonId) }).OrderBy(x => x.Id).ToList(),
                People = d.People?.Select(x => x with { Id = G(x.Id) }).OrderBy(x => x.Id).ToList(),
                Players = d.Players?.Select(x => x with { Id = G(x.Id), PersonId = G(x.PersonId) }).OrderBy(x => x.Id).ToList(),
                PlayerSeasonProfiles = d.PlayerSeasonProfiles?.Select(x => x with { Id = G(x.Id), PlayerId = G(x.PlayerId), SeasonId = G(x.SeasonId) }).OrderBy(x => x.Id).ToList(),
                RosterMemberships = d.RosterMemberships?.Select(x => x with { Id = G(x.Id), PlayerId = G(x.PlayerId), TeamSeasonId = G(x.TeamSeasonId) }).OrderBy(x => x.Id).ToList(),
                Games = d.Games?.Select(x => x with { Id = G(x.Id), SeasonId = G(x.SeasonId), HomeTeamSeasonId = G(x.HomeTeamSeasonId), AwayTeamSeasonId = G(x.AwayTeamSeasonId) }).OrderBy(x => x.Id).ToList()
            }
        };
    }
}

