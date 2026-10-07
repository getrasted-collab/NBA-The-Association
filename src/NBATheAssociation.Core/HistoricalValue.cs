namespace NBATheAssociation.Core;

public enum HistoricalValueState { Known, Unknown, NotApplicable }

public readonly record struct HistoricalValue<T>
{
    private readonly T? _value;

    private HistoricalValue(HistoricalValueState state, T? value)
    {
        State = state;
        _value = value;
    }

    public HistoricalValueState State { get; }
    public bool IsKnown => State == HistoricalValueState.Known;
    public T Value => IsKnown
        ? _value!
        : throw new InvalidOperationException("A historical value is accessible only when its state is Known.");

    public bool TryGetValue(out T? value)
    {
        value = IsKnown ? _value : default;
        return IsKnown;
    }

    public static HistoricalValue<T> Known(T value) =>
        value is null ? throw new ArgumentNullException(nameof(value)) : new(HistoricalValueState.Known, value);

    public static HistoricalValue<T> Unknown() => new(HistoricalValueState.Unknown, default);
    public static HistoricalValue<T> NotApplicable() => new(HistoricalValueState.NotApplicable, default);
}
