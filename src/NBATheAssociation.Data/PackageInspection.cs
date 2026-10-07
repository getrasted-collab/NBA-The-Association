namespace NBATheAssociation.Data;

public sealed record PackageSummary(string PackageId, string PackageVersion, int SchemaVersion, string? EraLabel, IReadOnlyList<string> Seasons, IReadOnlyList<string> Sources, IReadOnlyDictionary<string, int> EntityCounts, bool IsValid);
public sealed record ExternalIdTrace(string SourceName, string EntityType, string ExternalIdentifier, string InternalId, ProvenanceRecordDto? Provenance);

public static class PackageInspector
{
    public static PackageSummary Inspect(V2PackageDto p)
    {
        var d = p.Data!; var m = p.Manifest!;
        var counts = new Dictionary<string, int>
        {
            ["leagues"] = d.Leagues?.Count ?? 0, ["ruleSets"] = d.RuleSets?.Count ?? 0, ["seasons"] = d.Seasons?.Count ?? 0,
            ["franchises"] = d.Franchises?.Count ?? 0, ["teamSeasons"] = d.TeamSeasons?.Count ?? 0, ["people"] = d.People?.Count ?? 0,
            ["players"] = d.Players?.Count ?? 0, ["playerSeasonProfiles"] = d.PlayerSeasonProfiles?.Count ?? 0,
            ["rosterMemberships"] = d.RosterMemberships?.Count ?? 0, ["games"] = d.Games?.Count ?? 0
        };
        return new(m.PackageId, m.PackageVersion, m.SchemaVersion, m.EraLabel, m.SeasonIds ?? [], (p.Provenance ?? []).Select(x => x.SourceName).Distinct().Order().ToArray(), counts, V2PackageValidator.Validate(p).IsValid);
    }

    public static ExternalIdTrace? TraceExternal(V2PackageDto p, string source, string entityType, string externalId)
    {
        var mapping = (p.ExternalIdMappings ?? []).FirstOrDefault(x => string.Equals(x.SourceName, source, StringComparison.OrdinalIgnoreCase) && x.EntityType == entityType && x.ExternalIdentifier == externalId);
        if (mapping is null) return null;
        var provenance = mapping.ProvenanceId is null ? null : (p.Provenance ?? []).FirstOrDefault(x => x.Id.Equals(mapping.ProvenanceId, StringComparison.OrdinalIgnoreCase));
        return new(mapping.SourceName, mapping.EntityType, mapping.ExternalIdentifier, mapping.InternalId, provenance);
    }
}

public sealed record PackageDiffResult(IReadOnlyList<string> ManifestChanges, IReadOnlyDictionary<string, IReadOnlyList<string>> AddedIds, IReadOnlyDictionary<string, IReadOnlyList<string>> RemovedIds, IReadOnlyList<string> MappingChanges)
{
    public bool HasChanges => ManifestChanges.Count + AddedIds.Sum(x => x.Value.Count) + RemovedIds.Sum(x => x.Value.Count) + MappingChanges.Count > 0;
}

public static class PackageDiffer
{
    public static PackageDiffResult Compare(V2PackageDto left, V2PackageDto right)
    {
        var manifest = new List<string>();
        if (left.Manifest!.PackageId != right.Manifest!.PackageId) manifest.Add("packageId");
        if (left.Manifest.SchemaVersion != right.Manifest.SchemaVersion) manifest.Add("schemaVersion");
        if (left.Manifest!.PackageVersion != right.Manifest!.PackageVersion) manifest.Add("packageVersion");
        if (!new HashSet<string>(left.Manifest.SeasonIds ?? [], StringComparer.OrdinalIgnoreCase).SetEquals(right.Manifest.SeasonIds ?? [])) manifest.Add("seasonCoverage");
        if (!new HashSet<string>(left.Manifest.ContainedCategories ?? [], StringComparer.Ordinal).SetEquals(right.Manifest.ContainedCategories ?? [])) manifest.Add("categories");
        var l = Sets(left.Data!); var r = Sets(right.Data!);
        l["provenance"] = (left.Provenance ?? []).Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        r["provenance"] = (right.Provenance ?? []).Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var added = l.Keys.ToDictionary(k => k, k => (IReadOnlyList<string>)r[k].Except(l[k], StringComparer.OrdinalIgnoreCase).Order().ToArray());
        var removed = l.Keys.ToDictionary(k => k, k => (IReadOnlyList<string>)l[k].Except(r[k], StringComparer.OrdinalIgnoreCase).Order().ToArray());
        var lm = (left.ExternalIdMappings ?? []).ToDictionary(Key, x => x.InternalId, StringComparer.Ordinal);
        var rm = (right.ExternalIdMappings ?? []).ToDictionary(Key, x => x.InternalId, StringComparer.Ordinal);
        var mappingChanges = lm.Keys.Union(rm.Keys).Where(k => !lm.TryGetValue(k, out var a) || !rm.TryGetValue(k, out var b) || !a.Equals(b, StringComparison.OrdinalIgnoreCase)).Order().ToArray();
        return new(manifest, added, removed, mappingChanges);
    }
    private static string Key(ExternalIdMappingDto x) => $"{x.SourceName.ToUpperInvariant()}|{x.EntityType}|{x.ExternalIdentifier}";
    private static Dictionary<string, HashSet<string>> Sets(V2PackageDataDto d) => new()
    {
        ["leagues"] = (d.Leagues ?? []).Select(x => x.Id).ToHashSet(), ["ruleSets"] = (d.RuleSets ?? []).Select(x => x.Id).ToHashSet(),
        ["seasons"] = (d.Seasons ?? []).Select(x => x.Id).ToHashSet(), ["franchises"] = (d.Franchises ?? []).Select(x => x.Id).ToHashSet(),
        ["teamSeasons"] = (d.TeamSeasons ?? []).Select(x => x.Id).ToHashSet(), ["people"] = (d.People ?? []).Select(x => x.Id).ToHashSet(),
        ["players"] = (d.Players ?? []).Select(x => x.Id).ToHashSet(), ["playerSeasonProfiles"] = (d.PlayerSeasonProfiles ?? []).Select(x => x.Id).ToHashSet(),
        ["rosterMemberships"] = (d.RosterMemberships ?? []).Select(x => x.Id).ToHashSet(), ["games"] = (d.Games ?? []).Select(x => x.Id).ToHashSet()
    };
}
