using Infrastructure;
using Orleans.Runtime;
using Orleans.Runtime.Placement;

namespace Orleans.Hosting
{
    public static class GrainPlacementServiceCollectionExtensions
    {
        public static IServiceCollection DontHostGrainsHere(this IServiceCollection services)
        {
            services.AddSingletonNamedService<PlacementStrategy, DontPlaceMeOnTheDashboardStrategy>(nameof(DontPlaceMeOnTheDashboardSiloDirector));

            services.AddSingletonKeyedService<Type, IPlacementDirector, DontPlaceMeOnTheDashboardSiloDirector>(
                    typeof(DontPlaceMeOnTheDashboardStrategy));

            return services;
        }
    }
}
