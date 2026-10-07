namespace NBATheAssociation.Data;

public enum ValidationSeverity { Error, Warning }

public sealed record ValidationIssue(string Code, ValidationSeverity Severity, string Path, string Message, string? EntityType = null, string? EntityId = null);

public sealed class ValidationResult
{
    public ValidationResult(IEnumerable<ValidationIssue> issues) => Issues = issues.ToArray();
    public IReadOnlyList<ValidationIssue> Issues { get; }
    public bool IsValid => Issues.All(x => x.Severity != ValidationSeverity.Error);
}

