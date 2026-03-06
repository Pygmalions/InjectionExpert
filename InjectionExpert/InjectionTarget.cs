using System.Reflection;
using OneOf;

namespace InjectionExpert;

/// <summary>
/// Information of the target that is currently requesting an injection.
/// </summary>
public readonly record struct InjectionTarget()
{
    /// <summary>
    /// Optional instance that is currently requesting the injection.
    /// </summary>
    public object? Instance { get; init; }

    /// <summary>
    /// Optional metadata of the target which is current requesting the injection.
    /// </summary>
    public OneOf<ParameterInfo, FieldInfo, PropertyInfo>? Metadata { get; init; }

    /// <summary>
    /// Get the instance type of the target,
    /// either from <see cref="Instance"/> property or <see cref="Metadata"/>> property.
    /// </summary>
    /// <returns>Type of the owner or null if this target does not have any information about the owner.</returns>
    public Type? GetOwnerType()
    {
        if (Instance != null)
            return Instance.GetType();
        return Metadata?.Match(
            parameter => parameter.Member.DeclaringType,
            field => field.DeclaringType,
            property => property.DeclaringType);
    }

    public InjectionTarget(ParameterInfo parameter, object? instance) : this()
    {
        Metadata = parameter;
        Instance = instance;
    }

    public InjectionTarget(FieldInfo field, object? instance) : this()
    {
        Metadata = field;
        Instance = instance;
    }

    public InjectionTarget(PropertyInfo property, object? instance) : this()
    {
        Metadata = property;
        Instance = instance;
    }
}