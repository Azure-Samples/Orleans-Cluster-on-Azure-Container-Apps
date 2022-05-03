using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddWebAppApplicationInsights("Silo");

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Silo"));

app.Run();
