using System.Text.Json;
using NBATheAssociation.Core;
using NBATheAssociation.Data;

namespace NBATheAssociation.Tests;

public class DataFoundationV2Tests
{
    private static string Fixture(string name) => File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", name));
    private static V2PackageDto Valid() => V2PackageSerializer.Load(Fixture("v2-cross-era.json")).Package!;
    private static DataPackageDto V1() => DataPackageSerializer.Load(Fixture("synthetic-world.json")).Package!;

    [Test]
    public void Both_cross_era_fixtures_validate_and_preserve_historical_states()
    {
        var cross = V2PackageSerializer.Load(Fixture("v2-cross-era.json"));
        var sparse = V2PackageSerializer.Load(Fixture("v2-sparse-historical.json"));
        var sparseRoundTrip = V2PackageSerializer.Load(V2PackageSerializer.Serialize(sparse.Package!));
        Assert.Multiple(() =>
        {
            Assert.That(cross.IsSuccess, Is.True, Join(cross.Issues));
            Assert.That(sparse.IsSuccess, Is.True, Join(sparse.Issues));
            Assert.That(cross.World!.PlayerSeasonProfiles.Values.Any(x => x.ListedPosition.State == HistoricalValueState.Known), Is.True);
            Assert.That(cross.World.PlayerSeasonProfiles.Values.Any(x => x.ListedPosition.State == HistoricalValueState.Unknown), Is.True);
            Assert.That(sparseRoundTrip.World!.PlayerSeasonProfiles.Values.Single().ListedPosition.State, Is.EqualTo(HistoricalValueState.NotApplicable));
            Assert.That(sparseRoundTrip.World.PlayerSeasonProfiles.Values.Single().HeightInches.State, Is.EqualTo(HistoricalValueState.Unknown));
        });
    }

    [Test]
    public void Historical_value_enforces_access_and_round_trips_distinct_states()
    {
        var known = HistoricalValue<int>.Known(75);
        var unknown = HistoricalValue<int>.Unknown();
        var notApplicable = HistoricalValue<int>.NotApplicable();
        Assert.Multiple(() =>
        {
            Assert.That(known.Value, Is.EqualTo(75));
            Assert.That(unknown.State, Is.Not.EqualTo(notApplicable.State));
            Assert.That(() => _ = unknown.Value, Throws.InvalidOperationException);
            Assert.That(() => HistoricalValue<string>.Known(null!), Throws.ArgumentNullException);
        });

        var package = Valid();
        package.Data!.PlayerSeasonProfiles![0] = package.Data.PlayerSeasonProfiles[0] with { HeightInches = new("unknown", 75) };
        Code(V2PackageValidator.Validate(package), "historical_value.unexpected_value");
    }

    [Test]
    public void Manifest_provenance_and_category_failures_are_structured()
    {
        var missingVersion = Valid();
        missingVersion = missingVersion with { Manifest = missingVersion.Manifest! with { PackageVersion = "" } };
        Code(V2PackageValidator.Validate(missingVersion), "manifest.package_version.missing");

        var missingProvenance = Valid();
        missingProvenance = missingProvenance with { Manifest = missingProvenance.Manifest! with { ProvenanceIds = ["ffffffff-ffff-ffff-ffff-ffffffffffff"] } };
        Code(V2PackageValidator.Validate(missingProvenance), "provenance.reference.invalid");

        var categories = Valid();
        categories = categories with { Manifest = categories.Manifest! with { ContainedCategories = ["players"] } };
        Code(V2PackageValidator.Validate(categories), "manifest.category_mismatch");

        var malformed = V2PackageSerializer.Load("{\"manifest\":null,\"data\":null}");
        Assert.That(malformed.Issues.Select(x => x.Code), Does.Contain("manifest.malformed"));
    }

    [Test]
    public void External_mapping_rules_and_trace_are_provider_neutral()
    {
        var package = Valid();
        var traces = package.ExternalIdMappings!.Take(2).Select(x => PackageInspector.TraceExternal(package, x.SourceName, x.EntityType, x.ExternalIdentifier)).ToArray();
        Assert.That(traces.Select(x => x!.InternalId).Distinct().Count(), Is.EqualTo(1));
        Assert.That(traces.All(x => x!.Provenance is not null), Is.True);

        package.ExternalIdMappings!.Add(package.ExternalIdMappings[0]);
        Code(V2PackageValidator.Validate(package), "external_mapping.duplicate");

        var conflict = Valid();
        conflict.ExternalIdMappings!.Add(conflict.ExternalIdMappings[0] with { InternalId = "ffffffff-ffff-ffff-ffff-ffffffffffff" });
        Code(V2PackageValidator.Validate(conflict), "external_mapping.conflict");

        var invalid = Valid();
        invalid.ExternalIdMappings![0] = invalid.ExternalIdMappings[0] with { EntityType = "contract", ProvenanceId = "ffffffff-ffff-ffff-ffff-ffffffffffff" };
        Code(V2PackageValidator.Validate(invalid), "external_mapping.entity_type.unsupported");
    }

    [Test]
    public void Duplicate_cross_record_identities_and_overlaps_fail()
    {
        var team = Valid();
        team.Data!.TeamSeasons!.Add(team.Data.TeamSeasons[0] with { Id = "50000000-0000-0000-0000-000000000099" });
        Code(V2PackageValidator.Validate(team), "team_season.identity.duplicate");

        var profile = Valid();
        profile.Data!.PlayerSeasonProfiles!.Add(profile.Data.PlayerSeasonProfiles[0] with { Id = "80000000-0000-0000-0000-000000000099" });
        Code(V2PackageValidator.Validate(profile), "profile.identity.duplicate");

        var roster = Valid();
        roster.Data!.RosterMemberships!.Add(roster.Data.RosterMemberships[0] with { Id = "90000000-0000-0000-0000-000000000099", StartsOn = new(1985, 1, 1) });
        Code(V2PackageValidator.Validate(roster), "roster.overlap");
        Assert.That(V2PackageValidator.Validate(Valid()).IsValid, Is.True);
    }

    [Test]
    public void V1_migration_is_explicit_deterministic_and_preserves_source()
    {
        var source = V1();
        var before = DataPackageSerializer.Serialize(source);
        var provenance = Valid().Provenance!.Take(1).ToArray();
        var at = DateTimeOffset.Parse("2026-10-07T12:00:00Z");
        var first = V1ToV2PackageMigration.Migrate(source, at, provenance);
        var second = V1ToV2PackageMigration.Migrate(source, at, provenance);
        Assert.Multiple(() =>
        {
            Assert.That(first.IsSuccess, Is.True, Join(first.Issues));
            Assert.That(V2PackageSerializer.Serialize(first.Package!), Is.EqualTo(V2PackageSerializer.Serialize(second.Package!)));
            Assert.That(first.Package!.Data!.Players!.Select(x => x.Id), Is.EquivalentTo(source.Data.Players.Select(x => x.Id)));
            Assert.That(first.Package.Data.PlayerSeasonProfiles!.Any(x => x.HeightInches!.State == "unknown"), Is.True);
            Assert.That(first.Report.Notes, Is.Not.Empty);
            Assert.That(DataPackageSerializer.Serialize(source), Is.EqualTo(before));
        });
    }

    [Test]
    public void V2_round_trip_is_canonical_and_newer_schema_fails()
    {
        var package = Valid();
        var before = Fixture("v2-cross-era.json");
        var json = V2PackageSerializer.Serialize(package);
        var loaded = V2PackageSerializer.Load(json);
        var newer = V2PackageSerializer.Load(json.Replace("\"schemaVersion\": 2", "\"schemaVersion\": 3", StringComparison.Ordinal));
        Assert.Multiple(() =>
        {
            Assert.That(loaded.IsSuccess, Is.True, Join(loaded.Issues));
            Assert.That(newer.Issues.Select(x => x.Code), Does.Contain("schema.unsupported"));
            Assert.That(Fixture("v2-cross-era.json"), Is.EqualTo(before));
        });
    }

    [Test]
    public void Import_pipeline_stops_on_parse_or_validation_and_packages_valid_input()
    {
        var canonical = V2PackageSerializer.Serialize(Valid());
        var raw = Fixture("raw-synthetic-source.json").Replace("__CANONICAL_V2_JSON__", JsonEncodedText.Encode(canonical).ToString(), StringComparison.Ordinal);
        var result = PackageImportPipeline.Run(raw, new SyntheticRawJsonParser(), new SyntheticPackageNormalizer(DateTimeOffset.Parse("2026-10-07T12:00:00Z")));
        var parseFailure = PackageImportPipeline.Run("not json", new SyntheticRawJsonParser(), new SyntheticPackageNormalizer(DateTimeOffset.UtcNow));
        var normalizeFailure = PackageImportPipeline.Run("[]", new SyntheticRawJsonParser(), new SyntheticPackageNormalizer(DateTimeOffset.UtcNow));
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True, Join(result.Issues));
            Assert.That(result.CanonicalJson, Is.Not.Null);
            Assert.That(parseFailure.Issues.Select(x => x.Code), Does.Contain("import.parse"));
            Assert.That(normalizeFailure.Issues.Select(x => x.Code), Does.Contain("import.normalize"));
            Assert.That(raw, Does.Contain("canonicalPackageJson"));
        });
    }

    [Test]
    public void Inspection_and_diff_are_order_insensitive_but_report_real_changes()
    {
        var left = Valid();
        var reordered = V2PackageSerializer.Load(V2PackageSerializer.Serialize(left)).Package!;
        reordered.Data!.Players!.Reverse();
        reordered.ExternalIdMappings!.Reverse();
        Assert.That(PackageDiffer.Compare(left, reordered).HasChanges, Is.False);

        var changed = V2PackageSerializer.Load(V2PackageSerializer.Serialize(left)).Package!;
        changed = changed with { Manifest = changed.Manifest! with { PackageVersion = "2.0.1" } };
        changed.Data!.Players!.Add(changed.Data.Players[0] with { Id = "70000000-0000-0000-0000-000000000099" });
        changed.ExternalIdMappings![0] = changed.ExternalIdMappings[0] with { InternalId = "70000000-0000-0000-0000-000000000099" };
        var diff = PackageDiffer.Compare(left, changed);
        Assert.Multiple(() =>
        {
            Assert.That(PackageInspector.Inspect(left).EntityCounts["seasons"], Is.EqualTo(2));
            Assert.That(diff.ManifestChanges, Does.Contain("packageVersion"));
            Assert.That(diff.AddedIds["players"], Is.Not.Empty);
            Assert.That(diff.MappingChanges, Is.Not.Empty);
        });
    }

    [Test]
    public void Cli_handlers_return_expected_exit_codes()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "v2-cross-era.json");
        var output = new StringWriter();
        Assert.Multiple(() =>
        {
            Assert.That(CliApplication.Run(["validate", path], output, new StringWriter()), Is.Zero);
            Assert.That(CliApplication.Run(["inspect", path], new StringWriter(), new StringWriter()), Is.Zero);
            Assert.That(CliApplication.Run(["trace-external", path, "SyntheticArchiveA", "player", "player-44"], new StringWriter(), new StringWriter()), Is.Zero);
            Assert.That(CliApplication.Run(["trace-external", path, "SyntheticArchiveA", "player", "missing"], new StringWriter(), new StringWriter()), Is.EqualTo(1));
            Assert.That(CliApplication.Run(["unknown", path], new StringWriter(), new StringWriter()), Is.EqualTo(2));
        });
    }

    private static void Code(ValidationResult result, string code) => Assert.That(result.Issues.Select(x => x.Code), Does.Contain(code));
    private static string Join(IEnumerable<ValidationIssue> issues) => string.Join(Environment.NewLine, issues.Select(x => $"{x.Code}: {x.Message}"));
}
