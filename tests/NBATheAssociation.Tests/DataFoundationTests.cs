using System.Collections;
using NBATheAssociation.Core;
using NBATheAssociation.Data;

namespace NBATheAssociation.Tests;

public class DataFoundationTests
{
    private static string FixtureJson => File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "synthetic-world.json"));

    [Test]
    public void Strong_ids_are_distinct_and_round_trip()
    {
        var guid = Guid.Parse("70000000-0000-0000-0000-000000000001");
        var playerId = new PlayerId(guid);
        Assert.Multiple(() =>
        {
            Assert.That(PlayerId.Parse(playerId.ToString()), Is.EqualTo(playerId));
            Assert.That(typeof(PlayerId), Is.Not.EqualTo(typeof(FranchiseId)));
            Assert.That(typeof(Player).GetConstructors().Single().GetParameters()[0].ParameterType, Is.EqualTo(typeof(PlayerId)));
            Assert.That(() => new PlayerId(Guid.Empty), Throws.ArgumentException);
        });
    }

    [Test]
    public void Complete_fixture_validates_and_preserves_identity_continuity()
    {
        var result = DataPackageSerializer.Load(FixtureJson);
        Assert.That(result.IsSuccess, Is.True, Issues(result));
        var world = result.World!;
        var player = PlayerId.Parse("70000000-0000-0000-0000-000000000001");
        var memberships = world.RosterMemberships.Values.Where(x => x.PlayerId == player).ToArray();
        var franchise = FranchiseId.Parse("40000000-0000-0000-0000-000000000001");
        var teams = world.TeamSeasons.Values.Where(x => x.FranchiseId == franchise).OrderBy(x => x.SeasonId.ToString()).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(memberships, Has.Length.EqualTo(2));
            Assert.That(memberships.Select(x => x.TeamSeasonId).Distinct().ToArray(), Has.Length.EqualTo(2));
            Assert.That(teams, Has.Length.EqualTo(2));
            Assert.That(teams.Select(x => x.Id).Distinct().ToArray(), Has.Length.EqualTo(2));
            Assert.That(teams.Select(x => x.Market), Is.EquivalentTo(new[] { "Harbor City", "Summit City" }));
            Assert.That(teams.Select(x => x.Name), Is.EquivalentTo(new[] { "Comets", "Stallions" }));
        });
    }

    [Test]
    public void Duplicate_ids_fail_validation()
    {
        var package = ValidPackage();
        package.Data.Leagues.Add(package.Data.Leagues[0]);
        AssertCode(DataPackageValidator.Validate(package), "id.duplicate");
    }

    [Test]
    public void Missing_references_fail_validation()
    {
        var package = ValidPackage();
        package.Data.Players[0] = package.Data.Players[0] with { PersonId = "ffffffff-ffff-ffff-ffff-ffffffffffff" };
        AssertCode(DataPackageValidator.Validate(package), "reference.missing");
    }

    [TestCase("1985-01-02", "1985-01-01", "roster.date_range.invalid")]
    [TestCase("1983-01-01", null, "roster.outside_season")]
    public void Invalid_roster_ranges_fail(string start, string? end, string expectedCode)
    {
        var package = ValidPackage();
        package.Data.RosterMemberships[0] = package.Data.RosterMemberships[0] with
        {
            StartsOn = DateOnly.Parse(start),
            EndsOn = end is null ? null : DateOnly.Parse(end)
        };
        AssertCode(DataPackageValidator.Validate(package), expectedCode);
    }

    [Test]
    public void Json_round_trip_preserves_valid_data_and_unknown_values()
    {
        var first = DataPackageSerializer.Load(FixtureJson);
        var json = DataPackageSerializer.Serialize(first.Package!);
        var second = DataPackageSerializer.Load(json);
        Assert.Multiple(() =>
        {
            Assert.That(second.IsSuccess, Is.True, Issues(second));
            Assert.That(second.World!.PackageId, Is.EqualTo(first.World!.PackageId));
            Assert.That(second.World.Players.Keys, Is.EquivalentTo(first.World.Players.Keys));
            Assert.That(second.World.PlayerSeasonProfiles.Values.Any(x => x.HeightInches.State == HistoricalValueState.Unknown), Is.True);
        });
    }

    [Test]
    public void Schema_version_one_loads_and_newer_version_fails_clearly()
    {
        Assert.That(DataPackageSerializer.Load(FixtureJson).IsSuccess, Is.True);
        var newer = FixtureJson.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 999", StringComparison.Ordinal);
        var result = DataPackageSerializer.Load(newer);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.World, Is.Null);
            Assert.That(result.Issues.Select(x => x.Code), Does.Contain("schema.unsupported"));
        });
    }

    [Test]
    public void Materialization_does_not_mutate_or_share_source_state()
    {
        var first = DataPackageSerializer.Load(FixtureJson);
        var before = DataPackageSerializer.Serialize(first.Package!);
        _ = first.World!.Players.Values.ToArray();
        var after = DataPackageSerializer.Serialize(first.Package!);
        var second = DataPackageSerializer.Load(FixtureJson);

        Assert.Multiple(() =>
        {
            Assert.That(after, Is.EqualTo(before));
            Assert.That(second.World, Is.Not.SameAs(first.World));
            Assert.That(second.World!.Players, Is.Not.SameAs(first.World.Players));
            Assert.That(() => ((IDictionary)first.World.Players).Add(new object(), new object()), Throws.Exception);
        });
    }

    private static DataPackageDto ValidPackage() => DataPackageSerializer.Load(FixtureJson).Package!;
    private static void AssertCode(ValidationResult result, string code) => Assert.That(result.Issues.Select(x => x.Code), Does.Contain(code));
    private static string Issues(PackageLoadResult result) => string.Join(Environment.NewLine, result.Issues.Select(x => $"{x.Code}: {x.Message}"));
}
