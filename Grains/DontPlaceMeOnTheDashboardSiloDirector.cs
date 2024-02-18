﻿using JetBrains.Annotations;
using Orleans;
using Orleans.Placement;
using Orleans.Runtime;
using Orleans.Runtime.Placement;

namespace Grains
{
    [UsedImplicitly]
    public sealed class DontPlaceMeOnTheDashboardSiloDirector : IPlacementDirector
    {
        public IGrainFactory GrainFactory { get; set; }
        public IManagementGrain ManagementGrain { get; set; }

        public DontPlaceMeOnTheDashboardSiloDirector(IGrainFactory grainFactory)
        {
            GrainFactory = grainFactory;
            ManagementGrain = GrainFactory.GetGrain<IManagementGrain>(0);
        }

        public async Task<SiloAddress> OnAddActivation(PlacementStrategy strategy, PlacementTarget target, IPlacementContext context)
        {
            var activeSilos = await ManagementGrain.GetDetailedHosts(onlyActive: true);
            var silos = activeSilos.Where(x => !x.RoleName.ToLower().Contains("dashboard")).Select(x => x.SiloAddress).ToArray();
            return silos[new Random().Next(0, silos.Length)];
        }
    }

    [Serializable, GenerateSerializer]
    public sealed class DontPlaceMeOnTheDashboardStrategy : PlacementStrategy
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DontPlaceMeOnTheDashboardAttribute : PlacementAttribute
    {
        public DontPlaceMeOnTheDashboardAttribute() :
            base(new DontPlaceMeOnTheDashboardStrategy())
        {
        }
    }
}
