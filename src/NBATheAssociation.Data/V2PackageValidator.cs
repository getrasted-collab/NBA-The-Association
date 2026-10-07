using NBATheAssociation.Core;

namespace NBATheAssociation.Data;

public static class V2PackageValidator
{
    private static readonly string[] Categories = ["leagues", "ruleSets", "seasons", "franchises", "teamSeasons", "people", "players", "playerSeasonProfiles", "rosterMemberships", "games"];
    private static readonly HashSet<string> EntityTypes = new(["league", "ruleSet", "season", "franchise", "teamSeason", "person", "player", "game"], StringComparer.Ordinal);

    public static ValidationResult Validate(V2PackageDto package)
    {
        var issues = new List<ValidationIssue>();
        if (package.Manifest is null) Add("manifest.missing", "$.manifest", "manifest is required.");
        if (package.Data is null) Add("data.malformed", "$.data", "data is required.");
        if (package.Manifest is null || package.Data is null) return new(issues);
        var m = package.Manifest;
        var d = package.Data;
        var provenance = package.Provenance ?? MarkNull<ProvenanceRecordDto>("$.provenance");
        var mappings = package.ExternalIdMappings ?? MarkNull<ExternalIdMappingDto>("$.externalIdMappings");

        if (m.SchemaVersion != 2) Add("schema.unsupported", "$.manifest.schemaVersion", "Only schema version 2 is supported.");
        GuidValue(m.PackageId, "$.manifest.packageId");
        if (string.IsNullOrWhiteSpace(m.PackageVersion)) Add("manifest.package_version.missing", "$.manifest.packageVersion", "packageVersion is required.");
        if (m.CreatedAtUtc == default) Add("manifest.created_at.invalid", "$.manifest.createdAtUtc", "createdAtUtc is required.");

        var ids = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var leagues = List(d.Leagues, "$.data.leagues"); AddIds("league", leagues.Select(x => x.Id));
        var rules = List(d.RuleSets, "$.data.ruleSets"); AddIds("ruleSet", rules.Select(x => x.Id));
        var seasons = List(d.Seasons, "$.data.seasons"); AddIds("season", seasons.Select(x => x.Id));
        var franchises = List(d.Franchises, "$.data.franchises"); AddIds("franchise", franchises.Select(x => x.Id));
        var teams = List(d.TeamSeasons, "$.data.teamSeasons"); AddIds("teamSeason", teams.Select(x => x.Id));
        var people = List(d.People, "$.data.people"); AddIds("person", people.Select(x => x.Id));
        var players = List(d.Players, "$.data.players"); AddIds("player", players.Select(x => x.Id));
        var profiles = List(d.PlayerSeasonProfiles, "$.data.playerSeasonProfiles"); AddIds("playerSeasonProfile", profiles.Select(x => x.Id));
        var rosters = List(d.RosterMemberships, "$.data.rosterMemberships"); AddIds("rosterMembership", rosters.Select(x => x.Id));
        var games = List(d.Games, "$.data.games"); AddIds("game", games.Select(x => x.Id));

        var seasonById = seasons.GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var teamById = teams.GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var s in seasons)
        {
            Ref("league", s.LeagueId, $"$.data.seasons[{s.Id}].leagueId"); Ref("ruleSet", s.RuleSetId, $"$.data.seasons[{s.Id}].ruleSetId");
            if (s.EndsOn < s.StartsOn) Add("season.date_range.invalid", $"$.data.seasons[{s.Id}]", "Season end precedes start.");
        }
        foreach (var leagueSeasons in seasons.GroupBy(x => x.LeagueId, StringComparer.OrdinalIgnoreCase))
        {
            var ordered = leagueSeasons.OrderBy(x => x.StartsOn).ThenBy(x => x.EndsOn).ToArray();
            for (var i = 1; i < ordered.Length; i++)
                if (ordered[i].StartsOn <= ordered[i - 1].EndsOn)
                    Add("season.overlap", "$.data.seasons", "Seasons in the same League cannot have overlapping inclusive date ranges.");
        }
        foreach (var t in teams) { Ref("franchise", t.FranchiseId, $"$.data.teamSeasons[{t.Id}].franchiseId"); Ref("season", t.SeasonId, $"$.data.teamSeasons[{t.Id}].seasonId"); }
        foreach (var duplicate in teams.GroupBy(x => (x.FranchiseId.ToUpperInvariant(), x.SeasonId.ToUpperInvariant())).Where(x => x.Count() > 1))
            Add("team_season.identity.duplicate", "$.data.teamSeasons", "A franchise may have only one TeamSeason per season in V2.");
        foreach (var p in players) Ref("person", p.PersonId, $"$.data.players[{p.Id}].personId");
        foreach (var p in profiles)
        {
            Ref("player", p.PlayerId, $"$.data.playerSeasonProfiles[{p.Id}].playerId"); Ref("season", p.SeasonId, $"$.data.playerSeasonProfiles[{p.Id}].seasonId");
            HistoricalString(p.ListedPosition, $"$.data.playerSeasonProfiles[{p.Id}].listedPosition"); HistoricalInt(p.HeightInches, $"$.data.playerSeasonProfiles[{p.Id}].heightInches");
        }
        foreach (var duplicate in profiles.GroupBy(x => (x.PlayerId.ToUpperInvariant(), x.SeasonId.ToUpperInvariant())).Where(x => x.Count() > 1))
            Add("profile.identity.duplicate", "$.data.playerSeasonProfiles", "A player may have only one profile per season in V2.");
        foreach (var r in rosters)
        {
            Ref("player", r.PlayerId, $"$.data.rosterMemberships[{r.Id}].playerId"); Ref("teamSeason", r.TeamSeasonId, $"$.data.rosterMemberships[{r.Id}].teamSeasonId");
            if (r.EndsOn < r.StartsOn) Add("roster.date_range.invalid", $"$.data.rosterMemberships[{r.Id}]", "Roster end precedes start.");
            if (teamById.TryGetValue(r.TeamSeasonId, out var team) && seasonById.TryGetValue(team.SeasonId, out var season) &&
                (r.StartsOn < season.StartsOn || r.StartsOn > season.EndsOn || r.EndsOn is { } e && e > season.EndsOn))
                Add("roster.outside_season", $"$.data.rosterMemberships[{r.Id}]", "Roster membership is outside its season.");
        }
        foreach (var group in rosters.GroupBy(x => x.PlayerId, StringComparer.OrdinalIgnoreCase))
        {
            var entries = group.Select(r => (R: r, T: teamById.GetValueOrDefault(r.TeamSeasonId))).Where(x => x.T is not null)
                .GroupBy(x => x.T!.SeasonId, StringComparer.OrdinalIgnoreCase);
            foreach (var seasonGroup in entries)
            {
                var season = seasonById.GetValueOrDefault(seasonGroup.Key); if (season is null) continue;
                var ordered = seasonGroup.Select(x => (x.R.StartsOn, End: x.R.EndsOn ?? season.EndsOn)).OrderBy(x => x.StartsOn).ToArray();
                for (var i = 1; i < ordered.Length; i++) if (ordered[i].StartsOn <= ordered[i - 1].End) Add("roster.overlap", "$.data.rosterMemberships", "Player roster memberships overlap within a season.");
            }
        }
        foreach (var g in games)
        {
            Ref("season", g.SeasonId, $"$.data.games[{g.Id}].seasonId"); Ref("teamSeason", g.HomeTeamSeasonId, $"$.data.games[{g.Id}].homeTeamSeasonId"); Ref("teamSeason", g.AwayTeamSeasonId, $"$.data.games[{g.Id}].awayTeamSeasonId");
            if (!string.Equals(g.Status, "scheduled", StringComparison.OrdinalIgnoreCase)) Add("game.status.invalid", $"$.data.games[{g.Id}].status", "Only scheduled games are supported.");
            if (g.ScheduledStart == default) Add("game.scheduled_start.invalid", $"$.data.games[{g.Id}].scheduledStart", "A scheduled tipoff is required.");
            if (g.HomeTeamSeasonId == g.AwayTeamSeasonId) Add("game.same_participant", $"$.data.games[{g.Id}]", "Home and away must differ.");
            if (seasonById.TryGetValue(g.SeasonId, out var s))
            {
                if ((teamById.TryGetValue(g.HomeTeamSeasonId, out var h) && h.SeasonId != s.Id) || (teamById.TryGetValue(g.AwayTeamSeasonId, out var a) && a.SeasonId != s.Id)) Add("game.participant_wrong_season", $"$.data.games[{g.Id}]", "Participants must belong to game season.");
                var date = Game.GetScheduledDate(g.ScheduledStart); if (date < s.StartsOn || date > s.EndsOn) Add("game.outside_season", $"$.data.games[{g.Id}]", "Game is outside season.");
            }
        }
        foreach (var duplicate in games.GroupBy(g =>
        {
            var participants = new[] { g.HomeTeamSeasonId.ToUpperInvariant(), g.AwayTeamSeasonId.ToUpperInvariant() }.Order(StringComparer.Ordinal).ToArray();
            return (Season: g.SeasonId.ToUpperInvariant(), Instant: g.ScheduledStart.UtcTicks, First: participants[0], Second: participants[1]);
        }).Where(x => x.Count() > 1))
            Add("game.schedule_duplicate", "$.data.games", "Distinct Game IDs cannot represent the same matchup at the same instant.");

        foreach (var conflict in games
            .SelectMany(g => new[] { g.HomeTeamSeasonId, g.AwayTeamSeasonId }.Select(team => (Game: g, Team: team.ToUpperInvariant())))
            .GroupBy(x => (Season: x.Game.SeasonId.ToUpperInvariant(), Instant: x.Game.ScheduledStart.UtcTicks, x.Team))
            .Where(x => x.Select(v => v.Game.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1))
            Add("game.team_time_conflict", "$.data.games", "A TeamSeason cannot appear in multiple games at the same instant.");

        var provenanceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in provenance)
        {
            GuidValue(p.Id, "$.provenance[].id");
            if (!provenanceIds.Add(p.Id)) Add("provenance.id.duplicate", "$.provenance", "Duplicate provenance ID.");
            if (string.IsNullOrWhiteSpace(p.SourceName) || string.IsNullOrWhiteSpace(p.ImporterVersion) || p.ImportedAtUtc == default) Add("provenance.required_field.missing", $"$.provenance[{p.Id}]", "SourceName, ImporterVersion, and ImportedAtUtc are required.");
        }
        foreach (var id in m.ProvenanceIds ?? []) if (!provenanceIds.Contains(id)) Add("provenance.reference.invalid", "$.manifest.provenanceIds", "Manifest provenance reference does not exist.");
        foreach (var id in m.SeasonIds ?? []) if (!ids["season"].Contains(id)) Add("manifest.season_reference.invalid", "$.manifest.seasonIds", "Manifest season reference does not exist.");
        if (!new HashSet<string>(m.SeasonIds ?? [], StringComparer.OrdinalIgnoreCase).SetEquals(ids["season"]))
            Add("manifest.season_reference.invalid", "$.manifest.seasonIds", "Manifest season coverage must match contained seasons.");

        var actualCategories = new HashSet<string>(StringComparer.Ordinal);
        var counts = new[] { leagues.Count, rules.Count, seasons.Count, franchises.Count, teams.Count, people.Count, players.Count, profiles.Count, rosters.Count, games.Count };
        for (var i = 0; i < Categories.Length; i++) if (counts[i] > 0) actualCategories.Add(Categories[i]);
        var declared = new HashSet<string>(m.ContainedCategories ?? [], StringComparer.Ordinal);
        if (!actualCategories.SetEquals(declared)) Add("manifest.category_mismatch", "$.manifest.containedCategories", "Declared categories do not match contained data.");

        var externalKeys = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var x in mappings)
        {
            if (!EntityTypes.Contains(x.EntityType)) { Add("external_mapping.entity_type.unsupported", "$.externalIdMappings", "Unsupported entity type."); continue; }
            var key = $"{x.SourceName.ToUpperInvariant()}\u001f{x.EntityType}\u001f{x.ExternalIdentifier}";
            if (externalKeys.TryGetValue(key, out var target)) Add(target == x.InternalId ? "external_mapping.duplicate" : "external_mapping.conflict", "$.externalIdMappings", "Duplicate or conflicting external mapping."); else externalKeys[key] = x.InternalId;
            if (!ids[x.EntityType].Contains(x.InternalId)) Add("external_mapping.internal_reference.invalid", "$.externalIdMappings", "Internal mapping target does not exist.");
            if (x.ProvenanceId is not null && !provenanceIds.Contains(x.ProvenanceId)) Add("external_mapping.provenance_reference.invalid", "$.externalIdMappings", "Mapping provenance does not exist.");
        }
        foreach (var duplicate in mappings.GroupBy(x => (x.SourceName.ToUpperInvariant(), x.EntityType, x.InternalId)).Where(x => x.Select(v => v.ExternalIdentifier).Distinct().Count() > 1))
            issues.Add(new("external_mapping.multiple_ids_for_entity", ValidationSeverity.Warning, "$.externalIdMappings", "One source has multiple IDs for one entity."));

        return new(issues);

        List<T> MarkNull<T>(string path) { Add("data.malformed", path, "Collection cannot be null."); return []; }
        List<T> List<T>(List<T>? list, string path) => list ?? MarkNull<T>(path);
        void AddIds(string type, IEnumerable<string> values) { var set = ids[type] = new(StringComparer.OrdinalIgnoreCase); foreach (var v in values) { GuidValue(v, $"$.data.{type}.id"); if (!set.Add(v)) Add("id.duplicate", $"$.data.{type}.id", $"Duplicate {type} ID."); } }
        void GuidValue(string value, string path) { if (!Guid.TryParseExact(value, "D", out var id) || id == Guid.Empty) Add("id.empty", path, "A non-empty canonical GUID is required."); }
        void Ref(string type, string value, string path) { if (!ids[type].Contains(value)) Add("reference.missing", path, $"Referenced {type} does not exist."); }
        void HistoricalString(HistoricalStringValueDto? value, string path) => HistoricalCore(value?.State, value?.Value, path);
        void HistoricalInt(HistoricalIntValueDto? value, string path) => HistoricalCore(value?.State, value?.Value, path);
        void HistoricalCore(string? state, object? value, string path)
        {
            if (state is not ("known" or "unknown" or "notApplicable")) { Add("historical_value.state.invalid", path, "Historical value state is invalid."); return; }
            if (state == "known" && value is null) Add("historical_value.known_value.missing", path, "Known historical value requires a value.");
            if (state != "known" && value is not null) Add("historical_value.unexpected_value", path, "Unknown/notApplicable cannot carry a value.");
        }
        void Add(string code, string path, string message) => issues.Add(new(code, ValidationSeverity.Error, path, message));
    }
}
