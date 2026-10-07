namespace NBATheAssociation.Application;

public enum LeagueAdvanceStopReason
{
    DayAdvanced,
    RequestedDateReached,
    SeasonStartReached,
    NextGameDayReached,
    SeasonEndReached,
    AlreadyAtRequestedDate,
    AlreadyAtSeasonEnd,
    NoFutureGameDay,
    BackwardTargetRejected
}

public sealed record LeagueAdvanceResult(
    DateOnly PreviousDate,
    DateOnly CurrentDate,
    bool DidAdvance,
    LeagueAdvanceStopReason StopReason);
