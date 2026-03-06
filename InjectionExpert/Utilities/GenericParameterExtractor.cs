namespace InjectionExpert.Utilities;

public class GenericParameterExtractor
{
    /// <summary>
    /// Extracts generic type arguments from a concrete type and maps them to their corresponding positions
    /// in the generic type definition's parameter list.
    /// </summary>
    /// <remarks>
    /// This method resolves the actual type arguments by matching them against the generic type parameters
    /// defined in the <paramref name="definition"/>. It handles scenarios where generic parameters may appear
    /// in different orders or be nested within other generic types.
    /// <para>
    /// <b>Example:</b> Given a generic definition <c>Value&lt;T3, T1, T2&gt;</c> with parameters T1, T2, T3,
    /// and a concrete type <c>Value&lt;int, string, bool&gt;</c>, this method extracts the arguments as
    /// <c>[string, bool, int]</c>, mapping them to their original parameter positions [T1→string, T2→bool, T3→int].
    /// </para>
    /// <para>
    /// <b>Use cases:</b>
    /// <list type="bullet">
    /// <item>When <paramref name="definition"/> represents a base type or interface of a generic type where
    /// generic parameters may be reordered or repositioned.</item>
    /// <item>When <paramref name="definition"/> contains nested generic types with type parameters that need
    /// to be resolved from the concrete type arguments hierarchy.</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="target">The concrete generic type from which to extract type arguments.</param>
    /// <param name="definition">The generic type definition that specifies the parameter positions and structure.</param>
    /// <exception cref="Exception">Thrown if the arguments do not match the parameters.</exception>
    /// <returns>
    /// Array containing the extracted type arguments, ordered by their positions
    /// in the generic type definition's parameter list.
    /// </returns>
    public static Type[] ExtractArguments(Type target, Type definition)
    {
        var arguments = new Type[definition.GetGenericArguments().Length];

        Iterate(target, definition);

        return arguments;

        void Iterate(Type currentTarget, Type currentDefinition)
        {
            if (currentDefinition.IsGenericTypeParameter)
            {
                arguments[currentDefinition.GenericParameterPosition] = currentTarget;
                return;
            }

            if (!currentTarget.IsGenericType || !currentDefinition.IsGenericType ||
                currentTarget.GetGenericTypeDefinition() != currentDefinition.GetGenericTypeDefinition())
                throw new Exception("Cannot inject generic arguments: " +
                                    $"target type '{currentTarget}' and definition type '{currentDefinition}' do not match.");

            var targetArguments = currentTarget.GetGenericArguments();
            var definitionParameters = currentDefinition.GetGenericArguments();

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
                    Iterate(targetArgument, definitionParameter);
            }
        }
    }
}