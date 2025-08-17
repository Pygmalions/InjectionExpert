using System.Reflection;
using System.Text;

namespace InjectionExpert;

public class InjectionFailureException(
    Type type,
    IInjectionProvider provider,
    InjectionTarget? requester = null,
    string? message = null)
    : Exception(BuildExceptionMessage(type, requester, message))
{
    public Type ObjectType { get; } = type;

    public IInjectionProvider Provider { get; } = provider;

    public InjectionTarget Target { get; } = requester ?? default;

    private static string BuildExceptionMessage(Type type, InjectionTarget? requester, string? message)
    {
        var builder = new StringBuilder();
        builder.Append("Failed to inject type \"");
        builder.Append(type);
        builder.Append('\"');
        if (requester == null && message == null)
        {
            builder.Append('.');
            return builder.ToString();
        }

        if (requester != null)
        {
            builder.Append(": missing injection for ");
            var member = (object?)requester.Value.Member ?? requester.Value.Parameter;
            switch (member)
            {
                case null:
                    builder.Append("[Unknown Requester Member]");
                    break;
                case FieldInfo field:
                    builder.Append("field \"");
                    builder.Append(field.Name);
                    builder.Append("\" of type \"");
                    builder.Append(field.FieldType);
                    builder.Append("\".");
                    break;
                case PropertyInfo property:
                    builder.Append("property \"");
                    builder.Append(property.Name);
                    builder.Append("\" of type \"");
                    builder.Append(property.PropertyType);
                    builder.Append("\".");
                    break;
                case ParameterInfo parameter:
                    builder.Append("parameter \"");
                    builder.Append(parameter.Name);
                    builder.Append("\" of type \"");
                    builder.Append(parameter.ParameterType);
                    builder.Append("\".");
                    break;
            }
        }

        if (message == null)
            return builder.ToString();

        builder.Append(' ');
        builder.Append(message);

        return builder.ToString();
    }
}