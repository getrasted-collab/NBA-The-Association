using System.Collections.ObjectModel;

namespace NBATheAssociation.Core;

public sealed class LeagueWorld
{
    public LeagueWorld(
        DataPackageId packageId,
        IEnumerable<League> leagues,
        IEnumerable<RuleSet> ruleSets,
        IEnumerable<Season> seasons,
        IEnumerable<Franchise> franchises,
        IEnumerable<TeamSeason> teamSeasons,
        IEnumerable<Person> people,
        IEnumerable<Player> players,
        IEnumerable<PlayerSeasonProfile> profiles,
        IEnumerable<RosterMembership> memberships,
        IEnumerable<Game> games)
    {
        PackageId = packageId;
        Leagues = Freeze(leagues, x => x.Id);
        RuleSets = Freeze(ruleSets, x => x.Id);
        Seasons = Freeze(seasons, x => x.Id);
        Franchises = Freeze(franchises, x => x.Id);
        TeamSeasons = Freeze(teamSeasons, x => x.Id);
        People = Freeze(people, x => x.Id);
        Players = Freeze(players, x => x.Id);
        PlayerSeasonProfiles = Freeze(profiles, x => x.Id);
        RosterMemberships = Freeze(memberships, x => x.Id);
        Games = Freeze(games, x => x.Id);
    }

    public DataPackageId PackageId { get; }
    public IReadOnlyDictionary<LeagueId, League> Leagues { get; }
    public IReadOnlyDictionary<RuleSetId, RuleSet> RuleSets { get; }
    public IReadOnlyDictionary<SeasonId, Season> Seasons { get; }
    public IReadOnlyDictionary<FranchiseId, Franchise> Franchises { get; }
    public IReadOnlyDictionary<TeamSeasonId, TeamSeason> TeamSeasons { get; }
    public IReadOnlyDictionary<PersonId, Person> People { get; }
    public IReadOnlyDictionary<PlayerId, Player> Players { get; }
    public IReadOnlyDictionary<PlayerSeasonProfileId, PlayerSeasonProfile> PlayerSeasonProfiles { get; }
    public IReadOnlyDictionary<RosterMembershipId, RosterMembership> RosterMemberships { get; }
    public IReadOnlyDictionary<GameId, Game> Games { get; }

    private static IReadOnlyDictionary<TKey, TValue> Freeze<TKey, TValue>(IEnumerable<TValue> source, Func<TValue, TKey> key)
        where TKey : notnull => new ReadOnlyDictionary<TKey, TValue>(source.ToDictionary(key));
}

