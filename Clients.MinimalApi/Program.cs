using Abstractions;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWebAppApplicationInsights("Minimal API Client");

// if debugging, wait for the back-end services to start before connecting
if(Debugger.IsAttached)
{
    Console.WriteLine("Waiting 5 seconds for the Orleans cluster to start.");
    await Task.Delay(5000);
}

builder.Services.ConnectOrleansClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// reference the grain factory for the cluster
var _clusterClient = app.Services.GetRequiredService<IClusterClient>();

// connect to the orleans cluster
Console.WriteLine("Client about to connect to silo host \n");
await _clusterClient.Connect();
Console.WriteLine("Client successfully connected to silo host \n");

// -------------------
// map the API methods
// -------------------

// server is up
app.MapGet("/", () => "The silo is up and running.")
   .Produces<string>(StatusCodes.Status200OK)
   .WithName("Status");

// gets the list of active Orleans grains in the cluster
app.MapGet("/grains", async () =>
{
    var managementGrain = _clusterClient.GetGrain<IManagementGrain>(0);
    var stats = await managementGrain.GetSimpleGrainStatistics();
    var hosts = await managementGrain.GetDetailedHosts(onlyActive: true);
    var result = stats.Select(x => new GrainSummary(x.GrainType, x.ActivationCount, hosts.First(y => y.SiloAddress == x.SiloAddress).SiloName));
    return Results.Ok(result);
}).Produces<GrainSummary[]>(StatusCodes.Status200OK)
  .WithName("Grains");

// gets the list of hello grains in the system
app.MapGet("/providers", async () =>
{
    var managementGrain = _clusterClient.GetGrain<IManagementGrain>(0);
    var allGrains = await managementGrain.GetDetailedGrainStatistics();
    var grains = allGrains.Where(x => x.GrainType.Contains("Hello")).Select(x => x.GrainIdentity.PrimaryKeyString).OrderBy(x => x).ToArray();
    return Results.Ok(grains);
}).Produces<string[]>(StatusCodes.Status200OK).WithName("GetHelloProviders");

// gets a hello message from a grain
app.MapGet("/hello/{grain}", async (string grain) => {
    var helloGrain = _clusterClient.GetGrain<IHelloGrain>(grain);
    return Results.Ok(new WelcomeMessage(await helloGrain.SayHello()));
}).Produces<WelcomeMessage>(StatusCodes.Status200OK)
  .WithName("Welcome");

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

        var client = clientBuilder.Build();
        services.AddSingleton(client);
        return services;
    }
}

// record to show a summary of the cluster
public record GrainSummary(string GrainType, int Count, string Host);

// record to show the message from the grain
public record WelcomeMessage(string GrainType);