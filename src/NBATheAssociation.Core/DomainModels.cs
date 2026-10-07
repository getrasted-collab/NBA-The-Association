namespace NBATheAssociation.Core;

public sealed record League(LeagueId Id, string Name);
public sealed record RuleSet(RuleSetId Id, string Name, bool ThreePointEnabled, int RegulationPeriodCount, int RegulationPeriodMinutes);
public sealed record Season(SeasonId Id, LeagueId LeagueId, string Label, DateOnly StartsOn, DateOnly EndsOn, RuleSetId RuleSetId);
public sealed record Franchise(FranchiseId Id, string? InternalLabel);
public sealed record TeamSeason(TeamSeasonId Id, FranchiseId FranchiseId, SeasonId SeasonId, string Market, string Name, string Abbreviation, string? BrandingReference);
public sealed record Person(PersonId Id, string GivenName, string FamilyName);
public sealed record Player(PlayerId Id, PersonId PersonId);
public sealed record PlayerSeasonProfile(PlayerSeasonProfileId Id, PlayerId PlayerId, SeasonId SeasonId, string? ListedPosition, int? HeightInches, int? ExampleSkillRating);
public sealed record RosterMembership(RosterMembershipId Id, PlayerId PlayerId, TeamSeasonId TeamSeasonId, DateOnly StartsOn, DateOnly? EndsOn);
public enum GameStatus { Scheduled }
public sealed record Game(GameId Id, SeasonId SeasonId, DateTimeOffset ScheduledStart, TeamSeasonId HomeTeamSeasonId, TeamSeasonId AwayTeamSeasonId, GameStatus Status);

