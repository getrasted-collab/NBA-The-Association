using NBATheAssociation.Application;
using NBATheAssociation.Core;
using NBATheAssociation.Data;

namespace NBATheAssociation.Tests;

public class LeagueCalendarTests
{
    private static readonly SeasonId ShortSeasonId = SeasonId.Parse("32000000-0000-0000-0000-000000000001");
    private static readonly TeamSeasonId AuroraId = TeamSeasonId.Parse("52000000-0000-0000-0000-000000000001");

    [Test]
    public void Both_fixtures_validate_with_variable_league_sizes()
    {
        var shortResult = Load("league-calendar-short.json");
        var historical = Load("league-calendar-historical.json");
        var shortWorld = shortResult.World!;
        var historicalWorld = historical.World!;
        Assert.Multiple(() =>
        {
            Assert.That(shortResult.IsSuccess, Is.True, Issues(shortResult));
            Assert.That(historical.IsSuccess, Is.True, Issues(historical));
            Assert.That(new LeagueSession(shortWorld, ShortSeasonId).Participants, Has.Count.EqualTo(4));
            Assert.That(new LeagueSession(historicalWorld, SeasonId.Parse("33000000-0000-0000-0000-000000000001")).Participants, Has.Count.EqualTo(3));
            Assert.That(new LeagueSession(shortWorld, ShortSeasonId).Participants.All(x => shortWorld.Franchises.ContainsKey(x.FranchiseId)), Is.True);
        });
    }

    [Test]
    public void Scheduled_date_uses_stored_offset_civil_date_not_utc_date()
    {
        var game = ShortWorld().Games[GameId.Parse("a2000000-0000-0000-0000-000000000002")];
        Assert.Multiple(() =>
        {
            Assert.That(game.ScheduledDate, Is.EqualTo(new DateOnly(2035, 1, 1)));
            Assert.That(DateOnly.FromDateTime(game.ScheduledStart.UtcDateTime), Is.EqualTo(new DateOnly(2035, 1, 2)));
        });
    }

    [Test]
    public void Schedule_queries_use_strict_dates_and_deterministic_order()
    {
        var schedule = new ScheduleIndex(ShortWorld(), ShortSeasonId);
        var opening = schedule.GamesOn(new(2035, 1, 1));
        var upcoming = schedule.UpcomingGames(new(2035, 1, 1), 2);
        Assert.Multiple(() =>
        {
            Assert.That(schedule.AllGames, Has.Count.EqualTo(6));
            Assert.That(opening.Select(x => x.Id.ToString()), Is.EqualTo(new[]
            {
                "a2000000-0000-0000-0000-000000000001",
                "a2000000-0000-0000-0000-000000000002"
            }));
            Assert.That(schedule.GamesOn(new(2035, 1, 2)), Is.Empty);
            Assert.That(upcoming.Select(x => x.ScheduledDate), Is.EqualTo(new[] { new DateOnly(2035, 1, 3), new DateOnly(2035, 1, 4) }));
            Assert.That(schedule.NextGameAfter(new(2035, 1, 1))!.ScheduledDate, Is.EqualTo(new DateOnly(2035, 1, 3)));
            Assert.That(schedule.NextGameDayAfter(new(2035, 1, 1)), Is.EqualTo(new DateOnly(2035, 1, 3)));
        });
    }

    [Test]
    public void Team_schedule_contains_home_and_away_games_and_rejects_nonparticipant()
    {
        var schedule = new ScheduleIndex(ShortWorld(), ShortSeasonId);
        Assert.Multiple(() =>
        {
            Assert.That(schedule.GamesForTeam(AuroraId), Has.Count.EqualTo(3));
            Assert.That(schedule.GamesForTeam(AuroraId).All(x => x.HomeTeamSeasonId == AuroraId || x.AwayTeamSeasonId == AuroraId), Is.True);
            Assert.That(() => schedule.GamesForTeam(TeamSeasonId.Parse("53000000-0000-0000-0000-000000000001")), Throws.ArgumentException);
            Assert.That(() => schedule.UpcomingGames(new(2035, 1, 1), -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        });
    }

    [Test]
    public void Session_default_preseason_active_and_end_positions_are_explicit()
    {
        var world = ShortWorld();
        var opening = new LeagueSession(world, ShortSeasonId);
        var preseason = new LeagueSession(world, ShortSeasonId, new(2034, 12, 31));
        var end = new LeagueSession(world, ShortSeasonId, new(2035, 1, 12));
        Assert.Multiple(() =>
        {
            Assert.That(opening.CurrentDate, Is.EqualTo(new DateOnly(2035, 1, 1)));
            Assert.That(opening.ActiveSeason, Is.Not.Null);
            Assert.That(opening.DatePosition, Is.EqualTo(SeasonDatePosition.Active));
            Assert.That(preseason.ActiveSeason, Is.Null);
            Assert.That(preseason.DatePosition, Is.EqualTo(SeasonDatePosition.BeforeSeason));
            Assert.That(end.ActiveSeason, Is.Not.Null);
            Assert.That(end.DatePosition, Is.EqualTo(SeasonDatePosition.SeasonEnd));
        });
    }

    [Test]
    public void Advance_day_crosses_off_days_and_recognizes_opening_and_end()
    {
        var session = new LeagueSession(ShortWorld(), ShortSeasonId, new(2034, 12, 31));
        var opening = session.AdvanceDay();
        var offDay = session.AdvanceDay();
        session.AdvanceToDate(new(2035, 1, 11));
        var end = session.AdvanceDay();
        var blocked = session.AdvanceDay();
        Assert.Multiple(() =>
        {
            Assert.That(opening.StopReason, Is.EqualTo(LeagueAdvanceStopReason.SeasonStartReached));
            Assert.That(offDay.StopReason, Is.EqualTo(LeagueAdvanceStopReason.DayAdvanced));
            Assert.That(session.Schedule.GamesOn(offDay.CurrentDate), Is.Empty);
            Assert.That(end.StopReason, Is.EqualTo(LeagueAdvanceStopReason.SeasonEndReached));
            Assert.That(blocked.StopReason, Is.EqualTo(LeagueAdvanceStopReason.AlreadyAtSeasonEnd));
            Assert.That(blocked.DidAdvance, Is.False);
        });
    }

    [Test]
    public void Advance_to_date_rejects_backward_and_clamps_at_season_end()
    {
        var session = new LeagueSession(ShortWorld(), ShortSeasonId);
        var reached = session.AdvanceToDate(new(2035, 1, 5));
        var backward = session.AdvanceToDate(new(2035, 1, 4));
        var end = session.AdvanceToDate(new(2035, 2, 1));
        var alreadyEnded = session.AdvanceToDate(new(2035, 3, 1));
        Assert.Multiple(() =>
        {
            Assert.That(reached.StopReason, Is.EqualTo(LeagueAdvanceStopReason.RequestedDateReached));
            Assert.That(backward.StopReason, Is.EqualTo(LeagueAdvanceStopReason.BackwardTargetRejected));
            Assert.That(backward.CurrentDate, Is.EqualTo(new DateOnly(2035, 1, 5)));
            Assert.That(end.CurrentDate, Is.EqualTo(new DateOnly(2035, 1, 12)));
            Assert.That(end.StopReason, Is.EqualTo(LeagueAdvanceStopReason.SeasonEndReached));
            Assert.That(alreadyEnded.DidAdvance, Is.False);
            Assert.That(alreadyEnded.StopReason, Is.EqualTo(LeagueAdvanceStopReason.AlreadyAtSeasonEnd));
        });
    }

    [Test]
    public void Advance_to_next_game_day_skips_off_days_and_no_future_game_is_noop()
    {
        var session = new LeagueSession(ShortWorld(), ShortSeasonId);
        var next = session.AdvanceToNextGameDay();
        session.AdvanceToDate(new(2035, 1, 10));
        var none = session.AdvanceToNextGameDay();
        Assert.Multiple(() =>
        {
            Assert.That(next.CurrentDate, Is.EqualTo(new DateOnly(2035, 1, 3)));
            Assert.That(next.StopReason, Is.EqualTo(LeagueAdvanceStopReason.NextGameDayReached));
            Assert.That(none.DidAdvance, Is.False);
            Assert.That(none.StopReason, Is.EqualTo(LeagueAdvanceStopReason.NoFutureGameDay));
            Assert.That(none.CurrentDate, Is.EqualTo(new DateOnly(2035, 1, 10)));
        });
    }

    [Test]
    public void Sessions_are_independent_and_do_not_mutate_world_or_source_package()
    {
        var loaded = Load("league-calendar-short.json");
        var before = V2PackageSerializer.Serialize(loaded.Package!);
        var originalGame = loaded.World!.Games.Values.First();
        var first = new LeagueSession(loaded.World, ShortSeasonId);
        var second = new LeagueSession(loaded.World, ShortSeasonId);
        first.AdvanceToDate(new(2035, 1, 6));
        Assert.Multiple(() =>
        {
            Assert.That(second.CurrentDate, Is.EqualTo(new DateOnly(2035, 1, 1)));
            Assert.That(loaded.World.Games.Values.First(), Is.EqualTo(originalGame));
            Assert.That(V2PackageSerializer.Serialize(loaded.Package!), Is.EqualTo(before));
            Assert.That(first.World, Is.SameAs(second.World));
        });
    }

    [Test]
    public void Overlapping_seasons_fail_but_adjacent_and_gapped_seasons_pass()
    {
        var overlap = ValidPackage();
        AddSeason(overlap, "32000000-0000-0000-0000-000000000099", new(2035, 1, 10), new(2035, 2, 1));
        Code(V2PackageValidator.Validate(overlap), "season.overlap");

        var adjacent = ValidPackage();
        AddSeason(adjacent, "32000000-0000-0000-0000-000000000098", new(2035, 1, 13), new(2035, 2, 1));
        Assert.That(V2PackageValidator.Validate(adjacent).IsValid, Is.True);

        var gap = ValidPackage();
        AddSeason(gap, "32000000-0000-0000-0000-000000000097", new(2035, 2, 1), new(2035, 2, 12));
        Assert.That(V2PackageValidator.Validate(gap).IsValid, Is.True);
    }

    [Test]
    public void Duplicate_fixture_and_same_instant_team_conflict_fail()
    {
        var duplicate = ValidPackage();
        duplicate.Data!.Games!.Add(duplicate.Data.Games[0] with { Id = "a2000000-0000-0000-0000-000000000099", HomeTeamSeasonId = duplicate.Data.Games[0].AwayTeamSeasonId, AwayTeamSeasonId = duplicate.Data.Games[0].HomeTeamSeasonId });
        Code(V2PackageValidator.Validate(duplicate), "game.schedule_duplicate");

        var conflict = ValidPackage();
        conflict.Data!.Games!.Add(conflict.Data.Games[0] with
        {
            Id = "a2000000-0000-0000-0000-000000000098",
            HomeTeamSeasonId = "52000000-0000-0000-0000-000000000001",
            AwayTeamSeasonId = "52000000-0000-0000-0000-000000000003"
        });
        Code(V2PackageValidator.Validate(conflict), "game.team_time_conflict");
    }

    [Test]
    public void Same_day_different_time_and_consecutive_day_games_are_valid()
    {
        var package = ValidPackage();
        package.Data!.Games!.Add(package.Data.Games[0] with
        {
            Id = "a2000000-0000-0000-0000-000000000097",
            ScheduledStart = new DateTimeOffset(2035, 1, 1, 10, 0, 0, TimeSpan.FromHours(-5)),
            HomeTeamSeasonId = "52000000-0000-0000-0000-000000000001",
            AwayTeamSeasonId = "52000000-0000-0000-0000-000000000003"
        });
        Assert.That(V2PackageValidator.Validate(package).IsValid, Is.True);
        var auroraDates = package.Data.Games
            .Where(x => x.HomeTeamSeasonId == AuroraId.ToString() || x.AwayTeamSeasonId == AuroraId.ToString())
            .Select(x => Game.GetScheduledDate(x.ScheduledStart))
            .Distinct()
            .Order()
            .ToArray();
        Assert.That(auroraDates.Zip(auroraDates.Skip(1)).Any(x => x.Second == x.First.AddDays(1)), Is.True);
    }

    [Test]
    public void Default_scheduled_start_fails_structured_validation()
    {
        var package = ValidPackage();
        package.Data!.Games![0] = package.Data.Games[0] with { ScheduledStart = default };
        Code(V2PackageValidator.Validate(package), "game.scheduled_start.invalid");
    }

    [Test]
    public void Query_order_is_independent_of_source_collection_order()
    {
        var package = ValidPackage();
        var expected = new ScheduleIndex(V2PackageSerializer.Load(V2PackageSerializer.Serialize(package)).World!, ShortSeasonId).AllGames.Select(x => x.Id).ToArray();
        package.Data!.Games!.Reverse();
        var actual = new ScheduleIndex(V2PackageSerializer.Load(V2PackageSerializer.Serialize(package)).World!, ShortSeasonId).AllGames.Select(x => x.Id).ToArray();
        Assert.That(actual, Is.EqualTo(expected));
    }

    private static V2PackageLoadResult Load(string name) => V2PackageSerializer.Load(File.ReadAllText(FixturePath(name)));
    private static LeagueWorld ShortWorld() => Load("league-calendar-short.json").World!;
    private static V2PackageDto ValidPackage() => Load("league-calendar-short.json").Package!;
    private static string FixturePath(string name) => Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", name);
    private static string Issues(V2PackageLoadResult result) => string.Join(Environment.NewLine, result.Issues.Select(x => $"{x.Code}: {x.Message}"));
    private static void Code(ValidationResult result, string code) => Assert.That(result.Issues.Select(x => x.Code), Does.Contain(code));

    private static void AddSeason(V2PackageDto package, string id, DateOnly start, DateOnly end)
    {
        var original = package.Data!.Seasons![0];
        package.Data.Seasons.Add(original with { Id = id, Label = id, StartsOn = start, EndsOn = end });
        package.Manifest!.SeasonIds!.Add(id);
    }
}
