using NBATheAssociation.Core;

namespace NBATheAssociation.Data;

internal static class DataPackageMaterializer
{
    public static LeagueWorld Materialize(DataPackageDto p) => new(
        DataPackageId.Parse(p.PackageId),
        p.Data.Leagues.Select(x => new League(LeagueId.Parse(x.Id), x.Name)),
        p.Data.RuleSets.Select(x => new RuleSet(RuleSetId.Parse(x.Id), x.Name, x.ThreePointEnabled, x.RegulationPeriodCount, x.RegulationPeriodMinutes)),
        p.Data.Seasons.Select(x => new Season(SeasonId.Parse(x.Id), LeagueId.Parse(x.LeagueId), x.Label, x.StartsOn, x.EndsOn, RuleSetId.Parse(x.RuleSetId))),
        p.Data.Franchises.Select(x => new Franchise(FranchiseId.Parse(x.Id), x.InternalLabel)),
        p.Data.TeamSeasons.Select(x => new TeamSeason(TeamSeasonId.Parse(x.Id), FranchiseId.Parse(x.FranchiseId), SeasonId.Parse(x.SeasonId), x.Market, x.Name, x.Abbreviation, x.BrandingReference)),
        p.Data.People.Select(x => new Person(PersonId.Parse(x.Id), x.GivenName, x.FamilyName)),
        p.Data.Players.Select(x => new Player(PlayerId.Parse(x.Id), PersonId.Parse(x.PersonId))),
        p.Data.PlayerSeasonProfiles.Select(x => new PlayerSeasonProfile(PlayerSeasonProfileId.Parse(x.Id), PlayerId.Parse(x.PlayerId), SeasonId.Parse(x.SeasonId), x.ListedPosition, x.HeightInches, x.ExampleSkillRating)),
        p.Data.RosterMemberships.Select(x => new RosterMembership(RosterMembershipId.Parse(x.Id), PlayerId.Parse(x.PlayerId), TeamSeasonId.Parse(x.TeamSeasonId), x.StartsOn, x.EndsOn)),
        p.Data.Games.Select(x => new Game(GameId.Parse(x.Id), SeasonId.Parse(x.SeasonId), x.ScheduledStart, TeamSeasonId.Parse(x.HomeTeamSeasonId), TeamSeasonId.Parse(x.AwayTeamSeasonId), GameStatus.Scheduled)));
}

