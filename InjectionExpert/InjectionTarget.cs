using System.Reflection;
using JetBrains.Annotations;

namespace InjectionExpert;

public readonly record struct InjectionTarget(
    object? Instance = null,
    Type? Type = null,
    ParameterInfo? Parameter = null,
    MemberInfo? Member = null)
{
    /// <summary>
    /// Try to get the specified attribute from the parameter, member, or instance type.
    /// </summary>
    /// <param name="type">Attribute type.</param>
    /// <param name="inherit">True to inspect ancestors of the element, otherwise false.</param>
    /// <returns>Instance of the specified attribute type, or null if not found.</returns>
    [PublicAPI, System.Diagnostics.Contracts.Pure]
    public Attribute? GetAttribute(Type type, bool inherit = false)
        => Parameter?.GetCustomAttribute(type, inherit) ??
           Member?.GetCustomAttribute(type, inherit);

    /// <summary>
    /// Try to get the specified attribute from the parameter or member.
    /// </summary>
    /// <typeparam name="TAttribute">Attribute type.</typeparam>
    /// <param name="inherit">True to inspect ancestors of the element, otherwise false.</param>
    /// <returns></returns>
    [PublicAPI, System.Diagnostics.Contracts.Pure]
    public TAttribute? GetAttribute<TAttribute>(bool inherit = false) where TAttribute : Attribute
        => (TAttribute?)GetAttribute(typeof(TAttribute), inherit);
}