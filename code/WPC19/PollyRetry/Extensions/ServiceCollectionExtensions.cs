using System;
using Microsoft.Extensions.DependencyInjection;
using PollyRetry.Services;
using Refit;

namespace PollyRetry.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRefit(this IServiceCollection services, string baseAddress)
		{
			services.AddRefitClient<IMicroBApi>()
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));

			return services;
		}
	}
}
