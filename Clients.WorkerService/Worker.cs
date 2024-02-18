using Abstractions;
using Bogus;

namespace Clients.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        public IClusterClient OrleansClusterClient { get; set; }

        public Worker(ILogger<Worker> logger, IClusterClient orleansClusterClient)
        {
            _logger = logger;
            OrleansClusterClient = orleansClusterClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var rnd = new Randomizer(100);
            var randomDeviceIDs = new List<string>();
            var randomDevices = new Dictionary<string, ISensorTwinGrain>();

            for (var i = 0; i < 1024; i++)
            {
                var key = $"device{i.ToString().PadLeft(5, '0')}-{rnd.Long(1, 1024)}-{Environment.MachineName}";
                randomDeviceIDs.Add(key);
                var sensorTwinGrain = OrleansClusterClient.GetGrain<ISensorTwinGrain>(key);
                randomDevices.Add(key, sensorTwinGrain);
            }
            
            var faker = new Faker<SensorState>()
                .RuleFor(s => s.TimeStamp, DateTime.UtcNow)
                .RuleFor(s => s.Value, (f, s) => f.Random.Double(s.Value))
                .RuleFor(s => s.Type, (f, s) => f.Random.Enum<SensorType>());            

            while (!stoppingToken.IsCancellationRequested)
            {
                await Parallel.ForEachAsync(randomDeviceIDs, stoppingToken, async (deviceId, _) =>
                {
                    var sensorState = faker.RuleFor(s => s.SensorId, deviceId).Generate();
                    _logger.LogInformation("publishing sensor state >> {State}", sensorState.ToString());
                    await randomDevices[deviceId].ReceiveSensorState(sensorState);
                });
            }
        }
    }
}