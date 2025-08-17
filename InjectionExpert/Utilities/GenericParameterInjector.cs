namespace InjectionExpert.Utilities;

public class GenericParameterInjector
{
    /// <summary>
    /// This method extracts generic arguments from the target type, and fills them into the generic arguments array
    /// according to their order in the target definition. <br/>
    /// For example, when a generic type has generic parameters T1, T2, T3,
    /// and the target definition is Value{T3, T1, T2}, the target type is Value{int, string, bool},
    /// then the generic parameters will be [string, bool, int]. <br/>
    /// This method is useful when 'definition' is a base type or an interface of another generic type,
    /// where the generic parameters are probably not in the same order as the target type.
    /// </summary>
    /// <param name="type">Type to extract generic arguments from.</param>
    /// <param name="definition">Definition to get the order of parameters.</param>
    /// <param name="arguments">Extracted generic arguments.</param>
    /// <exception cref="Exception">Thrown if the arguments do not match the parameters.</exception>
    public static void Inject(Type type, Type definition, Span<Type> arguments)
    {
        if (definition.IsGenericTypeParameter)
        {
            arguments[definition.GenericParameterPosition] = type;
            return;
        }
        
        if (!type.IsGenericType || !definition.IsGenericType ||
            type.GetGenericTypeDefinition() != definition.GetGenericTypeDefinition())
            throw new Exception("Cannot inject generic arguments: " +
                                $"target type '{type}' and definition type '{definition}' do not match.");
        
        var targetArguments = type.GetGenericArguments();
        var definitionParameters = definition.GetGenericArguments();

        if (targetArguments.Length != definitionParameters.Length)
            throw new Exception("Generic arguments of target type and serializer interface do not match.");
        for (var index = 0; index < targetArguments.Length; ++index)
        {
            // Do the check in the loop to reduce the count of recursive calls.
            var targetArgument = targetArguments[index];
            var definitionParameter = definitionParameters[index];
            if (definitionParameter.IsGenericTypeParameter)
                arguments[definitionParameter.GenericParameterPosition] = targetArgument;
            else
                Inject(targetArgument, definitionParameter, arguments);
        }
    }
}