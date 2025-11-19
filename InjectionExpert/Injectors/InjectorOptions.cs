namespace InjectionExpert.Injectors;

[Flags]
public enum SelectionMode
{
    /// <summary>
    /// Select members with the 'required' keyword.
    /// </summary>
    RequiredMembers = 1 << 0,

    /// <summary>
    /// Select members with <see cref="InjectionAttribute"/>.
    /// </summary>
    AttributedMembers = 1 << 1
}

/// <summary>
/// 
/// </summary>
public readonly record struct InjectorOptions
{
    /// <summary>
    /// Default options:
    /// <br/> - Selected members: with 'required' keyword or <see cref="InjectionAttribute"/>.
    /// <br/> - Only null members: false (all selected members will be injected).
    /// <br/> - Fail fast: true (an exception will be thrown when a required injection cannot be found).
    /// <br/> - Found targets: null (will not be recorded).
    /// <br/> - Missing targets: null (will not be recorded).
    /// </summary>
    public static InjectorOptions Default { get; } = new()
    {
        SelectedMembers = SelectionMode.AttributedMembers | SelectionMode.RequiredMembers,
        OnlyNullMembers = false,
        FailFast = true,
        FoundTargets = null,
        MissingTargets = null
    };
    
    /// <summary>
    /// Controls which members will be injected.
    /// </summary>
    public required SelectionMode SelectedMembers { get; init; }

    /// <summary>
    /// If true, only members that are null will be injected.
    /// </summary>
    public required bool OnlyNullMembers { get; init; }

    /// <summary>
    /// If true, the injector will throw an exception
    /// when the injection for any required member cannot be found.
    /// </summary>
    public required bool FailFast { get; init; }

    /// <summary>
    /// If not null, the injector will record the injection targets that are found in this list.
    /// </summary>
    public required ICollection<(InjectionTarget, InjectionItem)>? FoundTargets { get; init; }

    /// <summary>
    /// If not null, the injector will record the injection targets that are not found in this list.
    /// </summary>
    public required ICollection<InjectionTarget>? MissingTargets { get; init; }
}