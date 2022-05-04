using Grains;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebAppApplicationInsights("Dashboard");
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
            options.SiloName = "Dashboard";
        })
        .ConfigureEndpoints(siloPort: 11_111, gatewayPort: 30_000)
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(builder.Configuration.GetValue<string>("StorageConnectionString")))
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(SensorTwinGrain).Assembly).WithReferences())
        .UseDashboard(config => 
            config.HideTrace = 
                !string.IsNullOrEmpty(builder.Configuration.GetValue<string>("HideTrace")) 
                    ? builder.Configuration.GetValue<bool>("HideTrace") 
                    : true);
});

// uncomment this if you dont mind hosting grains in the dashboard
// builder.Services.DontHostGrainsHere();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Dashboard"));

app.Run();
