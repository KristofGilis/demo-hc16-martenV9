using System.Collections.Concurrent;
using System.Reflection;
using HotChocolate.Configuration;
using HotChocolate.Types.Descriptors.Configurations;
using Marten.Linq;

namespace demo_hc16_marten9.Marten;

/// <summary>
/// Automatically converts any resolver result that is a <see cref="IMartenQueryable{T}"/>
/// (or an <see cref="IQueryable{T}"/> whose runtime instance is one, e.g. after <c>.OrderBy(...)</c>)
/// into a <see cref="MartenQueryableExecutable{T}"/>.
/// <para>
/// The conversion is registered as a <see cref="ResultFormatterConfiguration"/>, which HotChocolate
/// always compiles closest to the resolver - i.e. before the HotChocolate.Data field middleware
/// (<c>[UsePaging]</c>, <c>[UseFiltering]</c>, <c>[UseSorting]</c>) gets to process the result.
/// This removes the need to call
/// <see cref="CustomMartenExecutableHelper.AsMartenQueryableExecutable{T}(IMartenQueryable{T}, Func{IQueryable{T}, string}?)"/>
/// manually in every query resolver - resolvers can simply return <see cref="IQueryable{T}"/>
/// (or <see cref="IMartenQueryable{T}"/>) directly.
/// </para>
/// </summary>
public sealed class MartenExecutableTypeInterceptor : TypeInterceptor
{
    private const string FormatterKey = "SocialElections.Infrastructure.Marten.MartenExecutable";

    public override void OnBeforeCompleteType(
        ITypeCompletionContext completionContext,
        TypeSystemConfiguration configuration)
    {
        if (configuration is not ObjectTypeConfiguration objectTypeConfiguration)
        {
            return;
        }

        foreach (var field in objectTypeConfiguration.Fields)
        {
            TryAddMartenExecutableFormatter(field);
        }
    }

    private static void TryAddMartenExecutableFormatter(ObjectFieldConfiguration field)
    {
        var resultType = field.ResultType;
        if (resultType is null)
        {
            return;
        }

        var elementType = GetQueryableElementType(resultType);
        if (elementType is null)
        {
            return;
        }

        // Avoid adding the formatter twice, e.g. when a field configuration is merged/copied.
        if (field.FormatterConfigurations.Any(c => c.Key == FormatterKey))
        {
            return;
        }

        var convert = MartenExecutableConverters.GetOrCreate(elementType);

        field.FormatterConfigurations.Add(new ResultFormatterConfiguration(
            (_, result) => result is null ? null : convert(result),
            isRepeatable: true,
            key: FormatterKey));
    }

    private static Type? GetQueryableElementType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>))
        {
            return type.GetGenericArguments()[0];
        }

        var queryableInterface = type
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>));

        return queryableInterface?.GetGenericArguments()[0];
    }
}

/// <summary>
/// Caches, per element type, a delegate that converts a raw resolver result into a
/// <see cref="MartenQueryableExecutable{T}"/> - but only if the result actually is a
/// <see cref="IMartenQueryable{T}"/>. Any other <see cref="IQueryable{T}"/> is passed through
/// unmodified, keeping the automatic conversion scoped strictly to Marten.
/// </summary>
internal static class MartenExecutableConverters
{
    private static readonly ConcurrentDictionary<Type, Func<object, object>> Converters = new();

    private static readonly MethodInfo ConvertMethodDefinition = typeof(MartenExecutableConverters)
        .GetMethod(nameof(Convert), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static Func<object, object> GetOrCreate(Type elementType) =>
        Converters.GetOrAdd(elementType, static t =>
        {
            var method = ConvertMethodDefinition.MakeGenericMethod(t);
            return method.CreateDelegate<Func<object, object>>();
        });

    private static object Convert<T>(object source) where T : notnull =>
        source is IMartenQueryable<T> martenQueryable
            ? martenQueryable.AsMartenQueryableExecutable()
            : source;
}

