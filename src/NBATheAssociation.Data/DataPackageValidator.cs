namespace NBATheAssociation.Data;

public static class DataPackageValidator
{
    public static ValidationResult Validate(DataPackageDto package)
    {
        var issues = new List<ValidationIssue>();
        var ids = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        AddIds("league", package.Data.Leagues.Select(x => x.Id));
        AddIds("ruleSet", package.Data.RuleSets.Select(x => x.Id));
        AddIds("season", package.Data.Seasons.Select(x => x.Id));
        AddIds("franchise", package.Data.Franchises.Select(x => x.Id));
        AddIds("teamSeason", package.Data.TeamSeasons.Select(x => x.Id));
        AddIds("person", package.Data.People.Select(x => x.Id));
        AddIds("player", package.Data.Players.Select(x => x.Id));
        AddIds("playerSeasonProfile", package.Data.PlayerSeasonProfiles.Select(x => x.Id));
        AddIds("rosterMembership", package.Data.RosterMemberships.Select(x => x.Id));
        AddIds("game", package.Data.Games.Select(x => x.Id));

        ValidateGuid(package.PackageId, "$.packageId", "dataPackage");
        if (string.IsNullOrWhiteSpace(package.PackageVersion))
            Add("manifest.package_version.missing", "$.packageVersion", "Package version is required.");

        foreach (var season in package.Data.Seasons)
        {
            Ref("league", season.LeagueId, $"$.data.seasons[{season.Id}].leagueId", season.Id);
            Ref("ruleSet", season.RuleSetId, $"$.data.seasons[{season.Id}].ruleSetId", season.Id);
            if (season.EndsOn < season.StartsOn) Add("season.date_range.invalid", $"$.data.seasons[{season.Id}]", "Season end date precedes start date.", "season", season.Id);
        }
        foreach (var team in package.Data.TeamSeasons)
        {
            Ref("franchise", team.FranchiseId, $"$.data.teamSeasons[{team.Id}].franchiseId", team.Id);
            Ref("season", team.SeasonId, $"$.data.teamSeasons[{team.Id}].seasonId", team.Id);
        }
        foreach (var player in package.Data.Players) Ref("person", player.PersonId, $"$.data.players[{player.Id}].personId", player.Id);
        foreach (var profile in package.Data.PlayerSeasonProfiles)
        {
            Ref("player", profile.PlayerId, $"$.data.playerSeasonProfiles[{profile.Id}].playerId", profile.Id);
            Ref("season", profile.SeasonId, $"$.data.playerSeasonProfiles[{profile.Id}].seasonId", profile.Id);
        }
        foreach (var membership in package.Data.RosterMemberships)
        {
            Ref("player", membership.PlayerId, $"$.data.rosterMemberships[{membership.Id}].playerId", membership.Id);
            Ref("teamSeason", membership.TeamSeasonId, $"$.data.rosterMemberships[{membership.Id}].teamSeasonId", membership.Id);
            if (membership.EndsOn < membership.StartsOn) Add("roster.date_range.invalid", $"$.data.rosterMemberships[{membership.Id}]", "Roster membership end date precedes start date.", "rosterMembership", membership.Id);
            var team = package.Data.TeamSeasons.FirstOrDefault(x => x.Id == membership.TeamSeasonId);
            var season = team is null ? null : package.Data.Seasons.FirstOrDefault(x => x.Id == team.SeasonId);
            if (season is not null && (membership.StartsOn < season.StartsOn || membership.StartsOn > season.EndsOn || membership.EndsOn is { } end && end > season.EndsOn))
                Add("roster.outside_season", $"$.data.rosterMemberships[{membership.Id}]", "Roster membership falls outside its team season.", "rosterMembership", membership.Id);
        }
        foreach (var game in package.Data.Games)
        {
            Ref("season", game.SeasonId, $"$.data.games[{game.Id}].seasonId", game.Id);
            Ref("teamSeason", game.HomeTeamSeasonId, $"$.data.games[{game.Id}].homeTeamSeasonId", game.Id);
            Ref("teamSeason", game.AwayTeamSeasonId, $"$.data.games[{game.Id}].awayTeamSeasonId", game.Id);
            if (!string.Equals(game.Status, "scheduled", StringComparison.OrdinalIgnoreCase))
                Add("game.status.invalid", $"$.data.games[{game.Id}].status", "Only scheduled games are supported by V1.", "game", game.Id);
            if (game.HomeTeamSeasonId == game.AwayTeamSeasonId) Add("game.same_participant", $"$.data.games[{game.Id}]", "Home and away participants must differ.", "game", game.Id);
            var season = package.Data.Seasons.FirstOrDefault(x => x.Id == game.SeasonId);
            var home = package.Data.TeamSeasons.FirstOrDefault(x => x.Id == game.HomeTeamSeasonId);
            var away = package.Data.TeamSeasons.FirstOrDefault(x => x.Id == game.AwayTeamSeasonId);
            if (season is not null && ((home is not null && home.SeasonId != season.Id) || (away is not null && away.SeasonId != season.Id)))
                Add("game.participant_wrong_season", $"$.data.games[{game.Id}]", "Game participants must belong to the game's season.", "game", game.Id);
            if (season is not null && (DateOnly.FromDateTime(game.ScheduledStart.Date) < season.StartsOn || DateOnly.FromDateTime(game.ScheduledStart.Date) > season.EndsOn))
                Add("game.outside_season", $"$.data.games[{game.Id}].scheduledStart", "Game date falls outside its season.", "game", game.Id);
        }

        return new ValidationResult(issues);

        void AddIds(string type, IEnumerable<string> values)
        {
            var set = ids[type] = new(StringComparer.OrdinalIgnoreCase);
            var index = 0;
            foreach (var value in values)
            {
                ValidateGuid(value, $"$.data.{type}[{index}].id", type);
                if (!set.Add(value)) Add("id.duplicate", $"$.data.{type}[{index}].id", $"Duplicate {type} ID.", type, value);
                index++;
            }
        }
        void ValidateGuid(string value, string path, string type)
        {
            if (!Guid.TryParseExact(value, "D", out var parsed) || parsed == Guid.Empty) Add("id.empty", path, "ID must be a non-empty canonical GUID.", type, value);
        }
        void Ref(string type, string value, string path, string owner) { if (!ids.TryGetValue(type, out var set) || !set.Contains(value)) Add("reference.missing", path, $"Referenced {type} does not exist.", null, owner); }
        void Add(string code, string path, string message, string? type = null, string? id = null) => issues.Add(new(code, ValidationSeverity.Error, path, message, type, id));
    }
}
