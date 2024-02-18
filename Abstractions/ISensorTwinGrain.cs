using Orleans;
using System.Text.Json.Serialization;

namespace Abstractions
{
    public interface ISensorTwinGrain : IGrainWithStringKey
    {
        Task ReceiveSensorState(SensorState sensorState);
    }

    [GenerateSerializer, Immutable]
    public sealed record SensorState
    {
        [Id(0)]
        public string? SensorId { get; init; }
        
        [Id(1)]
        public double Value { get; init; }
        
        [Id(2)]
        public DateTime TimeStamp { get; init; } = DateTime.Now;
        
        [Id(3)]
        public SensorType Type { get; init; } = SensorType.Unspecified;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SensorType
    {
        Unspecified = 0,
        Motion = 1,
        Temperature = 2,
        Noise = 3,
        Breach = 4
    }
}
