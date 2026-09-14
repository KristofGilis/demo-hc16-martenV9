using demo_hc16_marten9;
using demo_hc16_marten9.Marten;
using JasperFx;
using Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("MartenConnection")!);
    options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
}).InitializeWith<InitialData>();

builder.Services.AddGraphQLServer()
    .AddQueryType()
    .Adddemo_hc16_marten9Types()
    .AddFiltering()
    .ConfigureSchema(sb => sb.TryAddTypeInterceptor<MartenExecutableTypeInterceptor>())
    .AddSorting()
    .AddMartenSorting()
    .AddPagingArguments();


var app = builder.Build();

app.MapGraphQL();

app.Run();