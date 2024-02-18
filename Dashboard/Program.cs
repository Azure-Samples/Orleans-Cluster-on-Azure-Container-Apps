using System.Diagnostics;
using Orleans.Configuration;

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
        ;
        
    DebugFoo(siloBuilder);
    ReleaseFoo(siloBuilder, builder.Configuration.GetValue<string>("StorageConnectionString") ?? "NOTSET");

    siloBuilder.UseDashboard(config => 
            config.HideTrace = 
                string.IsNullOrEmpty(builder.Configuration.GetValue<string>("HideTrace")) || builder.Configuration.GetValue<bool>("HideTrace"));
    
    [Conditional("RELEASE")]
    static void ReleaseFoo(ISiloBuilder sb, string connectionString)
    {
        sb.UseAzureStorageClustering(options => { options.ConfigureTableServiceClient(connectionString); })
            .ConfigureLogging(logging => logging.AddConsole());
    }

    [Conditional("DEBUG")]
    static void DebugFoo(ISiloBuilder sb)
    {
        sb.UseLocalhostClustering()
            .AddMemoryGrainStorage("InMemoryStore")
            .ConfigureServices(services =>
            {
                services.DontHostGrainsOnDashboard();
            })            
            .ConfigureLogging(logging => logging.AddConsole());
    }
});

// uncomment this if you dont want to host grains
builder.Services.DontHostGrainsOnDashboard();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Dashboard"));

await app.RunAsync();
