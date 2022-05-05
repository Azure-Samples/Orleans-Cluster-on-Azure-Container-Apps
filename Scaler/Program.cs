using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Scaler.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConnectOrleansClient();
builder.Services.AddGrpc();
builder.Services.AddWebAppApplicationInsights("Scaler");

var app = builder.Build();

app.MapGrpcService<ExternalScalerService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


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