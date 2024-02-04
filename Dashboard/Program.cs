using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebAppApplicationInsights("Dashboard");
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
            options.SiloName = "Dashboard";
        })
        .ConfigureEndpoints(siloPort: 11_112, gatewayPort: 30_001)
        .UseAzureStorageClustering(options => options.ConfigureTableServiceClient(builder.Configuration.GetValue<string>("StorageConnectionString")))
        .UseDashboard(config => 
            config.HideTrace = 
                string.IsNullOrEmpty(builder.Configuration.GetValue<string>("HideTrace")) || builder.Configuration.GetValue<bool>("HideTrace"));
});

// uncomment this if you dont mind hosting grains in the dashboard
builder.Services.DontHostGrainsHere();

var app = builder.Build();

app.MapGet("/", () => Results.Ok("Dashboard"));

app.Run();
