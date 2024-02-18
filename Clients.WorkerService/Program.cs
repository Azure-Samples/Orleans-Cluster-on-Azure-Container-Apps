using System.Diagnostics;
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

        DebugFoo(builder);
        ReleaseFoo(builder, context.Configuration.GetValue<string>("StorageConnectionString") ?? "NOTSET");

        [Conditional("RELEASE")]
        static void ReleaseFoo(IClientBuilder sb, string connectionString)
        {
            sb.UseAzureStorageClustering(options => { options.ConfigureTableServiceClient(connectionString); });
        }

        [Conditional("DEBUG")]
        static void DebugFoo(IClientBuilder sb)
        {
            sb.UseLocalhostClustering();
        }
    })
    .ConfigureServices(services =>
    {
        services.AddWorkerAppApplicationInsights("Worker Service Client");
        services.AddHostedService<Worker>();
    });

var webApplication = webApplicationBuilder.Build();

await webApplication.RunAsync();