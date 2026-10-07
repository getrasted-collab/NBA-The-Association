namespace NBATheAssociation.Data;

public sealed record DataPackageDto
{
    public int SchemaVersion { get; init; }
    public string PackageId { get; init; } = "";
    public string PackageVersion { get; init; } = "";
    public PackageDataDto Data { get; init; } = new();
}

public sealed record PackageDataDto
{
    public List<LeagueDto> Leagues { get; init; } = [];
    public List<RuleSetDto> RuleSets { get; init; } = [];
    public List<SeasonDto> Seasons { get; init; } = [];
    public List<FranchiseDto> Franchises { get; init; } = [];
    public List<TeamSeasonDto> TeamSeasons { get; init; } = [];
    public List<PersonDto> People { get; init; } = [];
    public List<PlayerDto> Players { get; init; } = [];
    public List<PlayerSeasonProfileDto> PlayerSeasonProfiles { get; init; } = [];
    public List<RosterMembershipDto> RosterMemberships { get; init; } = [];
    public List<GameDto> Games { get; init; } = [];
}

public sealed record LeagueDto(string Id, string Name);
public sealed record RuleSetDto(string Id, string Name, bool ThreePointEnabled, int RegulationPeriodCount, int RegulationPeriodMinutes);
public sealed record SeasonDto(string Id, string LeagueId, string Label, DateOnly StartsOn, DateOnly EndsOn, string RuleSetId);
public sealed record FranchiseDto(string Id, string? InternalLabel);
public sealed record TeamSeasonDto(string Id, string FranchiseId, string SeasonId, string Market, string Name, string Abbreviation, string? BrandingReference);
public sealed record PersonDto(string Id, string GivenName, string FamilyName);
public sealed record PlayerDto(string Id, string PersonId);
public sealed record PlayerSeasonProfileDto(string Id, string PlayerId, string SeasonId, string? ListedPosition, int? HeightInches, int? ExampleSkillRating);
public sealed record RosterMembershipDto(string Id, string PlayerId, string TeamSeasonId, DateOnly StartsOn, DateOnly? EndsOn);
public sealed record GameDto(string Id, string SeasonId, DateTimeOffset ScheduledStart, string HomeTeamSeasonId, string AwayTeamSeasonId, string Status);

