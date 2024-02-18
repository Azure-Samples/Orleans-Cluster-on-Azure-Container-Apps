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
                var key = $"device{i.ToString().PadLeft(5, '0')}-{rnd.Long(10000, 99999)}-{Environment.MachineName}";
                randomDeviceIDs.Add(key);
                var sensorTwinGrain = OrleansClusterClient.GetGrain<ISensorTwinGrain>(key);
                randomDevices.Add(key, sensorTwinGrain);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var faker = new Faker<SensorState>("en");
                await Parallel.ForEachAsync(randomDeviceIDs, stoppingToken, async (deviceId, _) =>
                {
                    await randomDevices[deviceId].ReceiveSensorState(faker.Generate());
                });
            }
        }
    }
}