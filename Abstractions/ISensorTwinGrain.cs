﻿using Orleans;
using System.Text.Json.Serialization;

namespace Abstractions
{
    public interface ISensorTwinGrain : IGrainWithStringKey
    {
        Task ReceiveSensorState(SensorState sensorState);
    }

    [GenerateSerializer, Immutable]
    public class SensorState
    {
        public string? SensorId { get; set; }
        public double Value { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public SensorType Type { get; set; } = SensorType.Unspecified;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [GenerateSerializer]
    public enum SensorType
    {
        Unspecified = 0,
        Motion = 1,
        Temperature = 2,
        Noise = 3,
        Breach = 4
    }
}
