namespace InjectionExpert;

/// <summary>
/// This attribute marks a member as dependency that should be injected (when 'enabled' is true),
/// or a parameter that should be ignored by the injector (when 'enabled' is false).
/// This attribute can also be used to provide an optional key for the dependency.
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