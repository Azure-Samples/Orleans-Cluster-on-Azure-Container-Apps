using Abstractions;
using Grains;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Placement;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(siloBuilder =>
{
    builder.Services.AddSingletonNamedService<
        PlacementStrategy, DontPlaceMeOnTheDashboardStrategy>(
            nameof(DontPlaceMeOnTheDashboardSiloDirector));

    builder.Services.AddSingletonKeyedService<
        Type, IPlacementDirector, DontPlaceMeOnTheDashboardSiloDirector>(
            typeof(DontPlaceMeOnTheDashboardStrategy));

    siloBuilder
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "Cluster";
            options.ServiceId = "Service";
        })
        .Configure<SiloOptions>(options =>
        {
            options.SiloName = "Dashboard";
        })
        .ConfigureEndpoints(siloPort: 11_112, gatewayPort: 30_001)
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(builder.Configuration.GetValue<string>("StorageConnectionString")))
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(SensorTwinGrain).Assembly).WithReferences())
        .UseDashboard(config => 
            config.HideTrace = 
                !string.IsNullOrEmpty(builder.Configuration.GetValue<string>("HideTrace")) 
                    ? builder.Configuration.GetValue<bool>("HideTrace") 
                    : true);
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Dashboard"));

app.Run();
