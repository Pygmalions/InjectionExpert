namespace InjectionExpert;

/// <summary>
/// The marked member is a dependency injected or configured from the outside.
/// This member should be selected by injectors, no matter they are marked as `required` or not.
/// </summary>
/// <param name="enabled">
/// Whether injection is enabled for this member.
/// Set it false to ignore members with the 'required' keyword.
/// </param>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
public class InjectionAttribute(bool enabled = true) : Attribute
{
    /// <summary>
    /// If true, this member will be ignored by injectors, even if it is marked as `required`.
    /// </summary>
    public bool Ignored { get; } = !enabled;
    
    /// <summary>
    /// Optional key for the dependency.
    /// </summary>
    public object? Key { get; init; }
}