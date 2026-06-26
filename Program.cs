using demo_hc16_marten9;
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
    .AddMartenFiltering()
    .AddSorting()
    .AddMartenSorting()
    .AddPagingArguments();


var app = builder.Build();

app.MapGraphQL();

app.Run();