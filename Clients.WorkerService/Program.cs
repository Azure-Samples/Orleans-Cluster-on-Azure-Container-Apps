using Clients.WorkerService;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddWorkerAppApplicationInsights("Worker Service Client");
        services.ConnectOrleansClient();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

// extension class that sets the Orleans client up and connects it to the cluster
public static class ServiceCollectionOrleansClientExtension
{
    public static IServiceCollection ConnectOrleansClient(this IServiceCollection services)
    {
        var clientBuilder = new ClientBuilder()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "Cluster";
                options.ServiceId = "Service";
            })
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning).AddJsonConsole())
            .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetValue<string>("StorageConnectionString")));

        Console.WriteLine("Client about to connect to silo host \n");
        var client = clientBuilder.Build();
        Console.WriteLine("Client successfully connected to silo host \n");
        services.AddSingleton(client);
        return services;
    }
}