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
using Polly.Registry;
using Refit;

namespace PollyRegistry
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
			IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.RetryAsync(2, onRetry: (response, retryCount) =>
				{
					if (response.Result.StatusCode != HttpStatusCode.InternalServerError)
					{
						//Do somethig
					}
					else
					{
						//Log something
					}
				});
			
			IAsyncPolicy<HttpResponseMessage> waitAndRetryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.WaitAndRetryAsync(2, 
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
					onRetry: (response, timeSpan, context) =>
					{
						if (response.Result.StatusCode != HttpStatusCode.InternalServerError)
						{
							//Perform re-auth
						}
						else
						{
							//Log something
						}
					});

			IAsyncPolicy<HttpResponseMessage> noOpPolicy = Policy.NoOpAsync<HttpResponseMessage>();

			var registry = new PolicyRegistry
			{
				{ "defaultRetry", retryPolicy },
				{ "defaultWaitAndRetry", waitAndRetryPolicy },
				{ "noOp", noOpPolicy}
			};
			services.AddPolicyRegistry(registry);

			services.AddControllers();

			services.AddRefitClient<IAzureDevOpsApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				.AddPolicyHandlerFromRegistry("defaultRetry")
				.AddPolicyHandlerFromRegistry((reg, request) =>
					{
						if(request.Method == HttpMethod.Get)
							return reg.Get<IAsyncPolicy<HttpResponseMessage>>("defaultRetry");
						
						if(request.RequestUri.LocalPath.StartsWith("matchWhatYouNeed"))
							return reg.Get<IAsyncPolicy<HttpResponseMessage>>("defaultWaitAndRetry");
						
						return reg.Get<IAsyncPolicy<HttpResponseMessage>>("noOp");
					});
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
