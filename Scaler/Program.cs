using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Scaler.Services;

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
            options.SiloName = "Scaler";
        })
        .ConfigureEndpoints(siloPort: 11_113, gatewayPort: 30_002)
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(builder.Configuration.GetValue<string>("StorageConnectionString")))
        ;
});

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<ExternalScalerService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
