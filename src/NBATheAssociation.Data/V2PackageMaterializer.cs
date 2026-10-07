using NBATheAssociation.Core;

namespace NBATheAssociation.Data;

internal static class V2PackageMaterializer
{
    public static LeagueWorld Materialize(V2PackageDto p)
    {
        var d = p.Data!;
        return new(
            DataPackageId.Parse(p.Manifest!.PackageId),
            d.Leagues!.Select(x => new League(LeagueId.Parse(x.Id), x.Name)),
            d.RuleSets!.Select(x => new RuleSet(RuleSetId.Parse(x.Id), x.Name, x.ThreePointEnabled, x.RegulationPeriodCount, x.RegulationPeriodMinutes)),
            d.Seasons!.Select(x => new Season(SeasonId.Parse(x.Id), LeagueId.Parse(x.LeagueId), x.Label, x.StartsOn, x.EndsOn, RuleSetId.Parse(x.RuleSetId))),
            d.Franchises!.Select(x => new Franchise(FranchiseId.Parse(x.Id), x.InternalLabel)),
            d.TeamSeasons!.Select(x => new TeamSeason(TeamSeasonId.Parse(x.Id), FranchiseId.Parse(x.FranchiseId), SeasonId.Parse(x.SeasonId), x.Market, x.Name, x.Abbreviation, x.BrandingReference)),
            d.People!.Select(x => new Person(PersonId.Parse(x.Id), x.GivenName, x.FamilyName)),
            d.Players!.Select(x => new Player(PlayerId.Parse(x.Id), PersonId.Parse(x.PersonId))),
            d.PlayerSeasonProfiles!.Select(x => new PlayerSeasonProfile(PlayerSeasonProfileId.Parse(x.Id), PlayerId.Parse(x.PlayerId), SeasonId.Parse(x.SeasonId), H(x.ListedPosition!), H(x.HeightInches!))),
            d.RosterMemberships!.Select(x => new RosterMembership(RosterMembershipId.Parse(x.Id), PlayerId.Parse(x.PlayerId), TeamSeasonId.Parse(x.TeamSeasonId), x.StartsOn, x.EndsOn)),
            d.Games!.Select(x => new Game(GameId.Parse(x.Id), SeasonId.Parse(x.SeasonId), x.ScheduledStart, TeamSeasonId.Parse(x.HomeTeamSeasonId), TeamSeasonId.Parse(x.AwayTeamSeasonId), GameStatus.Scheduled)));
    }

    private static HistoricalValue<string> H(HistoricalStringValueDto x) => x.State switch
    {
        "known" => HistoricalValue<string>.Known(x.Value!), "unknown" => HistoricalValue<string>.Unknown(), "notApplicable" => HistoricalValue<string>.NotApplicable(),
        _ => throw new InvalidOperationException("Validated historical value has invalid state.")
    };
    private static HistoricalValue<int> H(HistoricalIntValueDto x) => x.State switch
    {
        "known" => HistoricalValue<int>.Known(x.Value!.Value), "unknown" => HistoricalValue<int>.Unknown(), "notApplicable" => HistoricalValue<int>.NotApplicable(),
        _ => throw new InvalidOperationException("Validated historical value has invalid state.")
    };
}
