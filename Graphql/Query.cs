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
}