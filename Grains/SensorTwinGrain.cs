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

        public Task ReceiveSensorState(SensorState sensorState) =>
            Task.Run(() => Logger.LogInformation("Received value from sensorId {SensorId} of {SensorStateValue} for {SensorStateType} state reading from sensor {Key}", 
                sensorState.SensorId, sensorState.Value, sensorState.Type, this.GetGrainId().Key));
    }
}