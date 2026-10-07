using System.Collections.ObjectModel;
using NBATheAssociation.Core;

namespace NBATheAssociation.Application;

public sealed class ScheduleIndex
{
    private readonly IReadOnlyList<Game> _allGames;
    private readonly IReadOnlyDictionary<DateOnly, IReadOnlyList<Game>> _gamesByDate;
    private readonly IReadOnlyDictionary<TeamSeasonId, IReadOnlyList<Game>> _gamesByTeam;
    private readonly HashSet<TeamSeasonId> _participants;

    public ScheduleIndex(LeagueWorld world, SeasonId seasonId)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!world.Seasons.ContainsKey(seasonId))
            throw new ArgumentException("Season does not exist in the league world.", nameof(seasonId));

        SeasonId = seasonId;
        _participants = world.TeamSeasons.Values
            .Where(x => x.SeasonId == seasonId)
            .Select(x => x.Id)
            .ToHashSet();

        _allGames = ReadOnly(world.Games.Values
            .Where(x => x.SeasonId == seasonId)
            .OrderBy(x => x.ScheduledStart.UtcDateTime)
            .ThenBy(x => x.Id.Value));

        _gamesByDate = new ReadOnlyDictionary<DateOnly, IReadOnlyList<Game>>(
            _allGames.GroupBy(x => x.ScheduledDate)
                .ToDictionary(x => x.Key, x => ReadOnly(x)));

        _gamesByTeam = new ReadOnlyDictionary<TeamSeasonId, IReadOnlyList<Game>>(
            _participants.ToDictionary(
                x => x,
                x => ReadOnly(_allGames.Where(g => g.HomeTeamSeasonId == x || g.AwayTeamSeasonId == x))));
    }

    public SeasonId SeasonId { get; }
    public IReadOnlyList<Game> AllGames => _allGames;

    public IReadOnlyList<Game> GamesOn(DateOnly date) =>
        _gamesByDate.TryGetValue(date, out var games) ? games : Array.Empty<Game>();

    public IReadOnlyList<Game> UpcomingGames(DateOnly afterDate, int? limit = null)
    {
        if (limit < 0) throw new ArgumentOutOfRangeException(nameof(limit), "Limit cannot be negative.");
        var games = _allGames.Where(x => x.ScheduledDate > afterDate);
        if (limit is { } count) games = games.Take(count);
        return ReadOnly(games);
    }

    public IReadOnlyList<Game> GamesForTeam(TeamSeasonId teamSeasonId)
    {
        if (!_participants.Contains(teamSeasonId))
            throw new ArgumentException("TeamSeason does not participate in this schedule's season.", nameof(teamSeasonId));
        return _gamesByTeam[teamSeasonId];
    }

    public Game? NextGameAfter(DateOnly date) =>
        _allGames.FirstOrDefault(x => x.ScheduledDate > date);

    public DateOnly? NextGameDayAfter(DateOnly date) =>
        _allGames.Where(x => x.ScheduledDate > date)
            .Select(x => (DateOnly?)x.ScheduledDate)
            .Min();

    private static IReadOnlyList<Game> ReadOnly(IEnumerable<Game> games) =>
        Array.AsReadOnly(games.ToArray());
}
