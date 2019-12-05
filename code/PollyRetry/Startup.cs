using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Refit;

namespace PollyRetry
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
			services.AddControllers();

			#region Retry

			IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.RetryAsync(2, onRetry: (response, retryCount) =>
				{
					Debug.WriteLine($"Retry {retryCount}");

					if (response.Result.StatusCode != HttpStatusCode.InternalServerError)
					{
						//Do something
					}
					else
					{
						//Log something
					}
				});
			
			AsyncPolicy<HttpResponseMessage> waitAndRetryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.WaitAndRetryAsync(2, 
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
					onRetryAsync: (response, timeSpan, retryCount, context) =>
					{
						Debug.WriteLine($"Retry {retryCount}");

						if (response.Result.StatusCode != HttpStatusCode.InternalServerError)
						{
							//Do something
						}
						else
						{
							//Log something
						}

						return Task.CompletedTask;
					});

			#endregion

			services.AddRefitClient<IAzureDevOpsApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				//.AddTransientHttpErrorPolicy(p => p.RetryAsync(2));
				.AddPolicyHandler(retryPolicy);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
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
