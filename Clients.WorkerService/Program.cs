using Clients.WorkerService;
using Orleans.Configuration;

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
        services.AddWorkerAppApplicationInsights("Worker Service Client");
        services.AddHostedService<Worker>();
    });

var webApplication = webApplicationBuilder.Build();

await webApplication.RunAsync();