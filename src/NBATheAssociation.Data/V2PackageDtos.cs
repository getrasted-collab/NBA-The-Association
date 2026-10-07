namespace NBATheAssociation.Data;

public sealed record V2PackageDto
{
    public V2ManifestDto? Manifest { get; init; }
    public List<ProvenanceRecordDto>? Provenance { get; init; } = [];
    public List<ExternalIdMappingDto>? ExternalIdMappings { get; init; } = [];
    public V2PackageDataDto? Data { get; init; }
}

public sealed record V2ManifestDto(
    int SchemaVersion,
    string PackageId,
    string PackageVersion,
    DateTimeOffset CreatedAtUtc,
    string? EraLabel,
    List<string>? SeasonIds,
    List<string>? ProvenanceIds,
    List<string>? ContainedCategories);

public sealed record ProvenanceRecordDto(
    string Id,
    string SourceName,
    string? SourceIdentifier,
    DateTimeOffset? RetrievedAtUtc,
    DateTimeOffset ImportedAtUtc,
    string ImporterVersion,
    string? TransformationVersion,
    string? UsageNotes);

public sealed record ExternalIdMappingDto(string SourceName, string EntityType, string ExternalIdentifier, string InternalId, string? ProvenanceId);

public sealed record HistoricalStringValueDto(string State, string? Value = null);
public sealed record HistoricalIntValueDto(string State, int? Value = null);

public sealed record V2PackageDataDto
{
    public List<LeagueDto>? Leagues { get; init; } = [];
    public List<RuleSetDto>? RuleSets { get; init; } = [];
    public List<SeasonDto>? Seasons { get; init; } = [];
    public List<FranchiseDto>? Franchises { get; init; } = [];
    public List<TeamSeasonDto>? TeamSeasons { get; init; } = [];
    public List<PersonDto>? People { get; init; } = [];
    public List<PlayerDto>? Players { get; init; } = [];
    public List<V2PlayerSeasonProfileDto>? PlayerSeasonProfiles { get; init; } = [];
    public List<RosterMembershipDto>? RosterMemberships { get; init; } = [];
    public List<GameDto>? Games { get; init; } = [];
}

public sealed record V2PlayerSeasonProfileDto(
    string Id,
    string PlayerId,
    string SeasonId,
    HistoricalStringValueDto? ListedPosition,
    HistoricalIntValueDto? HeightInches);
