using Grains;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Orleans.Runtime.Placement;

namespace Orleans.Hosting
{
    public static class DontHostGrainsHereServiceCollectionExtensions
    {
        public static void DontHostGrainsOnDashboard(this IServiceCollection services)
        {
            services.AddSingletonNamedService<PlacementStrategy, DontPlaceMeOnTheDashboardStrategy>(nameof(DontPlaceMeOnTheDashboardStrategy));

            services.AddSingletonKeyedService<Type, IPlacementDirector, DontPlaceMeOnTheDashboardSiloDirector>(typeof(DontPlaceMeOnTheDashboardStrategy));
        }
    }
}
