using demo_hc16_marten9.Domain;
using Marten;
using Marten.Schema;

namespace demo_hc16_marten9;

public class InitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        await using var session = store.LightweightSession();
        session.Store(
            new Foo
            {
                Id = Guid.NewGuid(),
                Name = "Foo 1"
            },
            new Foo
            {
                Id = Guid.NewGuid(),
                Name = "Foo 2"
            },
            new Foo
            {
                Id = Guid.NewGuid(),
                Name = "Foo 3"
            }
        );
        await session.SaveChangesAsync(cancellation);
    }
}