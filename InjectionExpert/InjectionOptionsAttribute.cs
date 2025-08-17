namespace InjectionExpert;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class InjectionOptionsAttribute : Attribute
{
    /// <summary>
    /// If true, members with `required` keyword will be selected as injection target.
    /// This property is true by default.
    /// </summary>
    public bool WithRequiredMembers { get; set; } = true;

    /// <summary>
    /// If true, members with <see cref="InjectionAttribute"/> will be selected as injection target.
    /// This property is true by default.
    /// </summary>
    public bool WithAttributedMembers { get; set; } = true;
}