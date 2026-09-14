using System.Runtime.CompilerServices;
using Marten;
using Marten.Linq;

namespace demo_hc16_marten9.Marten;

public static class CustomMartenExecutableHelper
{
    public static MartenQueryableExecutable<T> AsMartenQueryableExecutable<T>(this IMartenQueryable<T> source,
        Func<IQueryable<T>, string>? printer = null)
        where T : notnull
    {
        return new MartenQueryableExecutable<T>(source.AsQueryable(), printer);
    }
}

public class MartenQueryableExecutable<T>(IQueryable<T> source, Func<IQueryable<T>, string>? printer = null) :
    Executable<T>,
    IQueryableExecutable<T> where T : notnull
{
    private readonly Func<IQueryable<T>, string> _printer = printer ?? (q => q.ToString() ?? string.Empty);

    public override object Source => source;

    public bool IsInMemory { get; } = source is EnumerableQuery;

    IQueryable<T> IQueryableExecutable<T>.Source => source;

    public IQueryableExecutable<T> WithSource(IQueryable<T> src)
        => new MartenQueryableExecutable<T>(src);

    public IQueryableExecutable<TQuery> WithSource<TQuery>(IQueryable<TQuery> src)
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        => new MartenQueryableExecutable<TQuery>(src);
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

    public override ValueTask<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return IsInMemory
            ? ValueTask.FromResult(source.FirstOrDefault())
            : new ValueTask<T?>(source.FirstOrDefaultAsync(cancellationToken));
    }

    public override ValueTask<T?> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return IsInMemory
            ? ValueTask.FromResult(source.SingleOrDefault())
            : new ValueTask<T?>(source.SingleOrDefaultAsync(cancellationToken));
    }

    public override async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        if (IsInMemory)
        {
            return [.. source];
        }

        return [.. await source.ToListAsync(cancellationToken)];
    }

    public override async IAsyncEnumerable<T> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (IsInMemory)
        {
            foreach (var element in source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return element;
            }
        }
        else
        {
            await foreach (var element in source.ToAsyncEnumerable(token: cancellationToken))
            {
                yield return element;
            }
        }
    }

    public override ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return IsInMemory
            ? ValueTask.FromResult(source.Count())
            : new ValueTask<int>(source.CountAsync(cancellationToken));
    }

    public override string Print() => _printer(source);
}
