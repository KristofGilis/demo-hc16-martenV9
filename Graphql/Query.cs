using demo_hc16_marten9.Domain;
using GreenDonut.Data;
using HotChocolate.Types.Pagination;
using Marten;

namespace demo_hc16_marten9.Graphql;

[QueryType]
public static partial class Query
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<Foo> GetFoos(IQuerySession session)
    {
        return session.Query<Foo>();
    }
    
    // [UseConnection]
    // [UseFiltering]
    // [UseSorting]
    // public static Task<PageConnection<Foo>> GetFoosNew(
    //     IQuerySession session, 
    //     PagingArguments pagingArguments, 
    //     QueryContext<Foo> queryContext, 
    //     CancellationToken cancellationToken)
    // {
    //     return session.Query<Foo>().With(queryContext).ToPageAsync(pagingArguments, cancellationToken);
    // }
}