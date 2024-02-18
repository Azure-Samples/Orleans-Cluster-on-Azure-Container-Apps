using Abstractions;
using Orleans.Runtime;
using System.Diagnostics;
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
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddWebAppApplicationInsights("Minimal API Client");        
    });

// if debugging, wait for the back-end services to start before connecting
if (Debugger.IsAttached)
{
    Console.WriteLine("Waiting 5 seconds for the Orleans cluster to start.");
    await Task.Delay(5000);
}

var webApplication = webApplicationBuilder.Build();

// Configure the HTTP request pipeline.
if (webApplication.Environment.IsDevelopment())
{
    webApplication.UseSwagger();
    webApplication.UseSwaggerUI();
}

// reference the grain factory for the cluster
var _clusterClient = webApplication.Services.GetRequiredService<IClusterClient>();

MapAPIEndpoints(webApplication, _clusterClient);

await webApplication.RunAsync();

void MapAPIEndpoints(WebApplication webApplication, IClusterClient clusterClient)
{
// server is up
    webApplication.MapGet("/", () => "The silo is up and running.")
        .Produces<string>(StatusCodes.Status200OK)
        .WithName("Status");

// gets the list of active Orleans grains in the cluster
    webApplication.MapGet("/grains", async () =>
        {
            var managementGrain = clusterClient.GetGrain<IManagementGrain>(0);
            var stats = await managementGrain.GetSimpleGrainStatistics();
            var hosts = await managementGrain.GetDetailedHosts(onlyActive: true);
            var result = stats.Select(x => new GrainSummary(x.GrainType, x.ActivationCount, hosts.First(y => y.SiloAddress == x.SiloAddress).SiloName));
            return Results.Ok(result);
        }).Produces<GrainSummary[]>(StatusCodes.Status200OK)
        .WithName("Grains");

// gets the list of hello grains in the system
    webApplication.MapGet("/providers", async () =>
    {
        var managementGrain = clusterClient.GetGrain<IManagementGrain>(0);
        var allGrains = await managementGrain.GetDetailedGrainStatistics();
        var grains = allGrains.Where(x => x.GrainType.Contains("Hello")).Select(x => x.GrainId.Key).OrderBy(x => x).ToArray();
        return Results.Ok(grains);
    }).Produces<string[]>().WithName("GetHelloProviders");

// gets a hello message from a grain
    webApplication.MapGet("/hello/{grain}", async (string grain) =>
        {
            var helloGrain = clusterClient.GetGrain<IHelloGrain>(grain);
            return Results.Ok(new WelcomeMessage(await helloGrain.SayHello()));
        }).Produces<WelcomeMessage>(StatusCodes.Status200OK)
        .WithName("Welcome");
}