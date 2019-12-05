using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Registry;
using PollyCommons;

namespace PollyCache
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			#region Cache provider and registry

			services.AddMemoryCache();
			services.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();

			services.AddPolicyRegistry();

			#endregion

			services.AddHttpClient("AzureDevOpsCache", client =>
			{
				client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
				client.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
			IAsyncCacheProvider cacheProvider, IPolicyRegistry<string> registry)
		{
			#region Cache

			static Ttl CacheOnly200OkFilter(Context context, HttpResponseMessage result) => 
				new Ttl(timeSpan: result.StatusCode == HttpStatusCode.OK ? TimeSpan.FromMinutes(1) : TimeSpan.Zero, 
					slidingExpiration: true);

			IAsyncPolicy<HttpResponseMessage> cacheOnlyOkPolicy =
				Policy.CacheAsync<HttpResponseMessage>(
					cacheProvider.AsyncFor<HttpResponseMessage>(),
					new ResultTtl<HttpResponseMessage>((Func<Context, HttpResponseMessage, Ttl>) CacheOnly200OkFilter),
					onCacheGet: (context, key) =>
					{
						//
					},
					onCacheMiss: (context, key) =>
					{
						//
					},
					onCachePut: (context, key) =>
					{
						//
					},
					onCacheGetError: (context, key, ex) =>
					{
						//
					},
					onCachePutError: (context, key, ex) =>
					{
						//
					});

			IAsyncPolicy<HttpResponseMessage> cachePolicy = Policy
				.CacheAsync<HttpResponseMessage>(cacheProvider,
					TimeSpan.FromMinutes(1),
					onCacheGet: (context, key) =>
					{
						//
					},
					onCacheMiss: (context, key) =>
					{
						//
					},
					onCachePut: (context, key) =>
					{
						//
					},
					onCacheGetError: (context, key, ex) =>
					{
						//
					},
					onCachePutError: (context, key, ex) =>
					{
						//
					});

			registry.Add(PolicyNames.CachePolicy, cachePolicy);
			registry.Add(PolicyNames.CacheOnlyOkPolicy, cacheOnlyOkPolicy);

			#endregion 

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
