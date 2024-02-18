using Abstractions;
using Microsoft.Extensions.Logging;

namespace Grains
{
    [CollectionAgeLimit(Minutes = 2)]
    [DontPlaceMeOnTheDashboard]
    public class SensorTwinGrain : Grain, ISensorTwinGrain
    {
        private ILogger<SensorTwinGrain> Logger { get; }

        public SensorTwinGrain(ILogger<SensorTwinGrain> logger) => Logger = logger;

        public async Task ReceiveSensorState(SensorState sensorState) =>
            await Task.Run(() => Logger.LogInformation($"Received value of {sensorState.Value} for {sensorState.Type} state reading from sensor {this.GetGrainId().Key}"));
    }
}