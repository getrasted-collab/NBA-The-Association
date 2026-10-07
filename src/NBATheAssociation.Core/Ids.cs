namespace NBATheAssociation.Core;

internal static class StrongId
{
    public static Guid Require(Guid value, string name) =>
        value == Guid.Empty ? throw new ArgumentException("ID cannot be empty.", name) : value;

    public static Guid Parse(string value, string name) =>
        Guid.TryParseExact(value, "D", out var parsed) && parsed != Guid.Empty
            ? parsed
            : throw new FormatException($"{name} must be a non-empty canonical GUID.");

    public static string Format(Guid value) => value.ToString("D").ToLowerInvariant();
}

public readonly record struct LeagueId { public LeagueId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static LeagueId Parse(string value) => new(StrongId.Parse(value, nameof(LeagueId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct SeasonId { public SeasonId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static SeasonId Parse(string value) => new(StrongId.Parse(value, nameof(SeasonId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct FranchiseId { public FranchiseId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static FranchiseId Parse(string value) => new(StrongId.Parse(value, nameof(FranchiseId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct TeamSeasonId { public TeamSeasonId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static TeamSeasonId Parse(string value) => new(StrongId.Parse(value, nameof(TeamSeasonId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct PersonId { public PersonId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static PersonId Parse(string value) => new(StrongId.Parse(value, nameof(PersonId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct PlayerId { public PlayerId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static PlayerId Parse(string value) => new(StrongId.Parse(value, nameof(PlayerId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct PlayerSeasonProfileId { public PlayerSeasonProfileId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static PlayerSeasonProfileId Parse(string value) => new(StrongId.Parse(value, nameof(PlayerSeasonProfileId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct RosterMembershipId { public RosterMembershipId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static RosterMembershipId Parse(string value) => new(StrongId.Parse(value, nameof(RosterMembershipId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct RuleSetId { public RuleSetId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static RuleSetId Parse(string value) => new(StrongId.Parse(value, nameof(RuleSetId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct GameId { public GameId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static GameId Parse(string value) => new(StrongId.Parse(value, nameof(GameId))); public override string ToString() => StrongId.Format(Value); }
public readonly record struct DataPackageId { public DataPackageId(Guid value) => Value = StrongId.Require(value, nameof(value)); public Guid Value { get; } public static DataPackageId Parse(string value) => new(StrongId.Parse(value, nameof(DataPackageId))); public override string ToString() => StrongId.Format(Value); }

