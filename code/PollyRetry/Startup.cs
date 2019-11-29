using System;
using System.Net;
using System.Net.Http;
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
			IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.RetryAsync(2, onRetry: (response, retryCount) =>
				{
					if (response.Result.StatusCode == HttpStatusCode.Forbidden)
					{
						//Perform re-auth
					}
					else
					{
						//Log something
					}
				});
			
			AsyncPolicy<HttpResponseMessage> waitAndRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.WaitAndRetryAsync(2, 
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
					onRetry: (response, timeSpan, context) =>
					{
						if (response.Result.StatusCode == HttpStatusCode.Forbidden)
						{
							//Perform re-auth
						}
						else
						{
							//Log something
						}
					});

			services.AddControllers();

			services.AddHttpClient("AzureDevOps", client =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				.AddTypedClient(RestService.For<IAzureDevOpsApi>)
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
