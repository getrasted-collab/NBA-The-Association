using NBATheAssociation.Core;

namespace NBATheAssociation.Application;

public enum SeasonDatePosition
{
    BeforeSeason,
    Active,
    SeasonEnd
}

public sealed class LeagueSession
{
    private readonly IReadOnlyList<TeamSeason> _participants;

    public LeagueSession(LeagueWorld world, SeasonId seasonId)
        : this(world, seasonId, GetSeason(world, seasonId).StartsOn)
    {
    }

    public LeagueSession(LeagueWorld world, SeasonId seasonId, DateOnly initialDate)
    {
        ArgumentNullException.ThrowIfNull(world);
        var season = GetSeason(world, seasonId);
        if (!world.Leagues.ContainsKey(season.LeagueId))
            throw new ArgumentException("Season's League does not exist in the league world.", nameof(seasonId));
        if (initialDate > season.EndsOn)
            throw new ArgumentOutOfRangeException(nameof(initialDate), "Initial date cannot be after season end.");

        World = world;
        TargetSeason = season;
        LeagueId = season.LeagueId;
        CurrentDate = initialDate;
        Schedule = new ScheduleIndex(world, seasonId);
        _participants = Array.AsReadOnly(world.TeamSeasons.Values
            .Where(x => x.SeasonId == seasonId)
            .OrderBy(x => x.Id.Value)
            .ToArray());
    }

    public LeagueWorld World { get; }
    public LeagueId LeagueId { get; }
    public Season TargetSeason { get; }
    public SeasonId TargetSeasonId => TargetSeason.Id;
    public DateOnly CurrentDate { get; private set; }
    public ScheduleIndex Schedule { get; }
    public IReadOnlyList<TeamSeason> Participants => _participants;
    public Season? ActiveSeason => CurrentDate < TargetSeason.StartsOn ? null : TargetSeason;
    public SeasonDatePosition DatePosition => CurrentDate < TargetSeason.StartsOn
        ? SeasonDatePosition.BeforeSeason
        : CurrentDate == TargetSeason.EndsOn
            ? SeasonDatePosition.SeasonEnd
            : SeasonDatePosition.Active;

    public LeagueAdvanceResult AdvanceDay()
    {
        if (CurrentDate == TargetSeason.EndsOn)
            return NoChange(LeagueAdvanceStopReason.AlreadyAtSeasonEnd);

        var previous = CurrentDate;
        CurrentDate = CurrentDate.AddDays(1);
        var reason = CurrentDate == TargetSeason.StartsOn && previous < TargetSeason.StartsOn
            ? LeagueAdvanceStopReason.SeasonStartReached
            : CurrentDate == TargetSeason.EndsOn
                ? LeagueAdvanceStopReason.SeasonEndReached
                : LeagueAdvanceStopReason.DayAdvanced;
        return new(previous, CurrentDate, true, reason);
    }

    public LeagueAdvanceResult AdvanceToDate(DateOnly target)
    {
        if (target < CurrentDate) return NoChange(LeagueAdvanceStopReason.BackwardTargetRejected);
        if (target == CurrentDate) return NoChange(LeagueAdvanceStopReason.AlreadyAtRequestedDate);
        if (CurrentDate == TargetSeason.EndsOn) return NoChange(LeagueAdvanceStopReason.AlreadyAtSeasonEnd);

        var previous = CurrentDate;
        var next = target > TargetSeason.EndsOn ? TargetSeason.EndsOn : target;
        CurrentDate = next;
        var reason = next == TargetSeason.StartsOn && previous < TargetSeason.StartsOn
            ? LeagueAdvanceStopReason.SeasonStartReached
            : next == TargetSeason.EndsOn
                ? LeagueAdvanceStopReason.SeasonEndReached
                : LeagueAdvanceStopReason.RequestedDateReached;
        return new(previous, CurrentDate, true, reason);
    }

    public LeagueAdvanceResult AdvanceToNextGameDay()
    {
        var next = Schedule.NextGameDayAfter(CurrentDate);
        if (next is null || next > TargetSeason.EndsOn)
            return NoChange(LeagueAdvanceStopReason.NoFutureGameDay);

        var previous = CurrentDate;
        CurrentDate = next.Value;
        var reason = CurrentDate == TargetSeason.StartsOn && previous < TargetSeason.StartsOn
            ? LeagueAdvanceStopReason.SeasonStartReached
            : LeagueAdvanceStopReason.NextGameDayReached;
        return new(previous, CurrentDate, true, reason);
    }

    private LeagueAdvanceResult NoChange(LeagueAdvanceStopReason reason) =>
        new(CurrentDate, CurrentDate, false, reason);

    private static Season GetSeason(LeagueWorld world, SeasonId seasonId)
    {
        ArgumentNullException.ThrowIfNull(world);
        return world.Seasons.TryGetValue(seasonId, out var season)
            ? season
            : throw new ArgumentException("Season does not exist in the league world.", nameof(seasonId));
    }
}
