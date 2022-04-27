using Grains;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Placement;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingletonNamedService<
        PlacementStrategy, DontPlaceMeOnTheDashboardStrategy>(
            nameof(DontPlaceMeOnTheDashboardSiloDirector));

builder.Services.AddSingletonKeyedService<
    Type, IPlacementDirector, DontPlaceMeOnTheDashboardSiloDirector>(
        typeof(DontPlaceMeOnTheDashboardStrategy));

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "Cluster";
            options.ServiceId = "Service";
        })
        .Configure<SiloOptions>(options =>
        {
            options.SiloName = "Silo";
        })
        .ConfigureEndpoints(siloPort: 11_111, gatewayPort: 30_000)
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(builder.Configuration.GetValue<string>("StorageConnectionString")))
        ;
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Silo"));

app.Run();
