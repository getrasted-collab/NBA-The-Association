namespace NBATheAssociation.Data;

public sealed record MigrationReport(int SourceSchemaVersion, int TargetSchemaVersion, IReadOnlyList<string> Notes, int EntityCount);
public sealed record MigrationResult(V2PackageDto? Package, MigrationReport Report, IReadOnlyList<ValidationIssue> Issues)
{
    public bool IsSuccess => Package is not null && Issues.All(x => x.Severity != ValidationSeverity.Error);
}

public static class V1ToV2PackageMigration
{
    public static MigrationResult Migrate(DataPackageDto source, DateTimeOffset createdAtUtc, IReadOnlyList<ProvenanceRecordDto> provenance)
    {
        var notes = new List<string>();
        if (source.Data.PlayerSeasonProfiles.Any(x => x.ExampleSkillRating is not null))
            notes.Add("V1 ExampleSkillRating was an architecture placeholder and is intentionally omitted from V2 runtime data.");

        var d = source.Data;
        var categories = new List<string>();
        void Cat(string name, int count) { if (count > 0) categories.Add(name); }
        Cat("leagues", d.Leagues.Count); Cat("ruleSets", d.RuleSets.Count); Cat("seasons", d.Seasons.Count); Cat("franchises", d.Franchises.Count);
        Cat("teamSeasons", d.TeamSeasons.Count); Cat("people", d.People.Count); Cat("players", d.Players.Count); Cat("playerSeasonProfiles", d.PlayerSeasonProfiles.Count);
        Cat("rosterMemberships", d.RosterMemberships.Count); Cat("games", d.Games.Count);

        var package = new V2PackageDto
        {
            Manifest = new(2, source.PackageId, source.PackageVersion, createdAtUtc.ToUniversalTime(), "Migrated V1 synthetic package",
                d.Seasons.Select(x => x.Id).ToList(), provenance.Select(x => x.Id).ToList(), categories),
            Provenance = provenance.ToList(),
            ExternalIdMappings = [],
            Data = new()
            {
                Leagues = d.Leagues.ToList(), RuleSets = d.RuleSets.ToList(), Seasons = d.Seasons.ToList(), Franchises = d.Franchises.ToList(),
                TeamSeasons = d.TeamSeasons.ToList(), People = d.People.ToList(), Players = d.Players.ToList(),
                PlayerSeasonProfiles = d.PlayerSeasonProfiles.Select(x => new V2PlayerSeasonProfileDto(x.Id, x.PlayerId, x.SeasonId,
                    x.ListedPosition is null ? new HistoricalStringValueDto("unknown") : new HistoricalStringValueDto("known", x.ListedPosition),
                    x.HeightInches is null ? new HistoricalIntValueDto("unknown") : new HistoricalIntValueDto("known", x.HeightInches.Value))).ToList(),
                RosterMemberships = d.RosterMemberships.ToList(), Games = d.Games.ToList()
            }
        };
        var validation = V2PackageValidator.Validate(package);
        var count = categories.Sum(c => c switch
        {
            "leagues" => d.Leagues.Count, "ruleSets" => d.RuleSets.Count, "seasons" => d.Seasons.Count, "franchises" => d.Franchises.Count,
            "teamSeasons" => d.TeamSeasons.Count, "people" => d.People.Count, "players" => d.Players.Count, "playerSeasonProfiles" => d.PlayerSeasonProfiles.Count,
            "rosterMemberships" => d.RosterMemberships.Count, "games" => d.Games.Count, _ => 0
        });
        return new(validation.IsValid ? package : null, new(1, 2, notes, count), validation.Issues);
    }
}
