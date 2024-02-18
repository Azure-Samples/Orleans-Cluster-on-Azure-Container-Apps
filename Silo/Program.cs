using System.Diagnostics;
using Orleans.Configuration;
using Orleans.Runtime;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebAppApplicationInsights("Silo");

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "Cluster";
            options.ServiceId = "Service";
        })
        .Configure<SiloOptions>(options => { options.SiloName = "Silo"; })
        .ConfigureEndpoints(siloPort: 11_111, gatewayPort: 30_000)
        ;

    DebugFoo(siloBuilder);
    ReleaseFoo(siloBuilder, builder.Configuration.GetValue<string>("StorageConnectionString") ?? "NOTSET");

    [Conditional("RELEASE")]
    static void ReleaseFoo(ISiloBuilder sb, string connectionString)
    {
        sb.UseAzureStorageClustering(options => { options.ConfigureTableServiceClient(connectionString); })
            .ConfigureServices(services =>
            {
                services.DontHostGrainsOnDashboard();
            })
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

        sb.UseDashboard(config => config.HideTrace = false);
    }
});

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Silo"));

await app.RunAsync();