using System.Reflection;
using JetBrains.Annotations;

namespace InjectionExpert;

/// <summary>
/// Information of the target that requests an injection.
/// </summary>
public readonly record struct InjectionTarget
{
    /// <summary>
    /// Instance that owns the injection target.
    /// </summary>
    public object? OwnerInstance { get; }

    /// <summary>
    /// Type of the uninstantiated owner that owns the injection target.
    /// </summary>
    public Type? OwnerType { get; }

    /// <summary>
    /// Member that requests the injection.
    /// </summary>
    public MemberInfo? Member { get; }

    /// <summary>
    /// Parameter that requests the injection.
    /// </summary>
    public ParameterInfo? Parameter { get; }

    private InjectionTarget(object? ownerInstance, Type? ownerType,
        MemberInfo? targetMember,
        ParameterInfo? targetParameter)
    {
        OwnerInstance = ownerInstance;
        OwnerType = ownerType ?? ownerInstance?.GetType()
            ?? targetMember?.DeclaringType
            ?? targetParameter?.Member.DeclaringType;
        Parameter = targetParameter;
        Member = targetMember;
    }

    /// <summary>
    /// Create an empty injection target for unknown requester.
    /// </summary>
    public InjectionTarget() :
        this(null, null, null, null)
    {
    }

    public InjectionTarget(object ownerInstance) 
        : this(ownerInstance, null, null, null)
    {
    }
    
    public InjectionTarget(Type ownerType) 
        : this(null, ownerType, null, null)
    {
    }
    
    /// <summary>
    /// Create an injection target for a member on the specified instance.
    /// </summary>
    /// <param name="member">Member that requests the injection.</param>
    /// <param name="owner">
    /// Instance that owns the member, or null if the member is static.
    /// </param>
    public InjectionTarget(MemberInfo member, object? owner = null) :
        this(owner, null, member, null)
    {
    }

    /// <summary>
    /// Create an injection target for a parameter of a constructor.
    /// </summary>
    /// <param name="parameter">Parameter that requests the injection.</param>
    /// <param name="owner">
    /// Instance that owns the member, or null if the object has not been instantiated.
    /// </param>
    public InjectionTarget(ParameterInfo parameter, object? owner = null) : 
        this(owner, null, null, parameter)
    {}

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