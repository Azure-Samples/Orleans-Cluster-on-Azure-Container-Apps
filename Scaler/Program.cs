using Orleans.Configuration;
using Scaler.Services;

var webApplicationBuilder = WebApplication.CreateBuilder(args);

webApplicationBuilder.Host.UseOrleansClient((context, builder) =>
    {
        builder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "Cluster";
            options.ServiceId = "Service";
        });

        var connectionString = context.Configuration.GetValue<string>("StorageConnectionString");

        builder.UseAzureStorageClustering(options => options.ConfigureTableServiceClient(connectionString));
    })
    .ConfigureServices(services =>
    {
        services.AddWorkerAppApplicationInsights("Scaler");
        services.AddGrpc();
    });

var webApplication = webApplicationBuilder.Build();

webApplication.MapGrpcService<ExternalScalerService>();
webApplication.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await webApplication.RunAsync();