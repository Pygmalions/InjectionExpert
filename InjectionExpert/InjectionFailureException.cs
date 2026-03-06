using System.Reflection;
using System.Text;

namespace InjectionExpert;

public class InjectionFailureException(
    Type requestedType, 
    object? requestedKey,
    IInjectionProvider provider,
    InjectionTarget target = default,
    string? message = null)
    : Exception(BuildExceptionMessage(requestedType, requestedKey, target, message))
{
    /// <summary>
    /// Type of the injection request.
    /// </summary>
    public Type RequestedType { get; } = requestedType;

    /// <summary>
    /// Optional key of the injection request.
    /// </summary>
    public object? RequestedKey { get; } = requestedKey;
    
    /// <summary>
    /// Provider that cannot provide the requested injection.
    /// </summary>
    public IInjectionProvider Provider { get; } = provider;

    /// <summary>
    /// Metadata about the injection target which requests the injection.
    /// </summary>
    public InjectionTarget Target { get; } = target;

    private static string BuildExceptionMessage(
        Type requestedType, object? requestedKey,
        InjectionTarget target, string? message)
    {
        if (target.GetOwnerType() is not { } owner)
        {
            return $"Cannot find requested injection '{requestedType.Name}' with key '{requestedKey}'.";
        }
        
        var builder = new StringBuilder();
        builder.Append("Failed to inject type '")
            .Append(owner)
            .Append("': ");
        builder.Append("cannot find requested injection '")
            .Append(requestedType.Name)
            .Append('\'');
        if (requestedKey is not null)
            builder.Append($" with key '{requestedKey}'.");
        else 
            builder.Append('.');
        if (message is not null)
            builder.Append("( ").Append(message).Append(" )");
        return builder.ToString();
    }
}