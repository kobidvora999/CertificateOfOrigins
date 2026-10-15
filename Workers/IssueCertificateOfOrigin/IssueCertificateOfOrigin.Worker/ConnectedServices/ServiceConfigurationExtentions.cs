using CustomsCloud.InfrastructureCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomsCloud.InfrastructureCore;
public static class ServiceConfigurationExtentions
{
	public static IServiceCollection AddWcfProxies(this IServiceCollection services)
	{
		services.AddWcfProxy<ICertificateOfOriginsExternalContract>();
		services.AddWcfProxy<IDocumentsExternalContract>();
		return services;
	}
}
