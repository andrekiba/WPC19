using System;
using System.Net;
using System.Net.Http;
using Api;
using Api.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Polly.Retry;
using PollyCommons;
using PollyCommons.Extensions;
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
			#region Policy

			IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.RetryAsync(2, onRetry: (response, retryCount, context) =>
				{
					if (response.Result.StatusCode != HttpStatusCode.InternalServerError)
					{
						//Log something
					}
					else
					{
						//Log something else
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
							//Log something
						}
						else
						{
							//Log something else
						}
					});

			IAsyncPolicy<ApiResponse<Project>> refitRetryPolicy = Policy
				.Handle<ApiException>()
				.OrResult<ApiResponse<Project>>(r => !r.IsSuccessStatusCode)
				.WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					onRetry: (delegatedResult, timeSpan, context) =>
					{
						if (delegatedResult.Result != null)
						{
							var apiResponse = delegatedResult.Result;
						}
						else if (delegatedResult.Exception != null)
						{
							var apiEx = delegatedResult.Exception as ApiException;
						}
					});

			IAsyncPolicy<HttpResponseMessage> noOpPolicy = Policy.NoOpAsync<HttpResponseMessage>();

			var registry = new PolicyRegistry
			{
				{ PolicyNames.DefaultRetry, retryPolicy },
				{ PolicyNames.DefaultWaitAndRetry, waitAndRetryPolicy },
				{ PolicyNames.NoOp, noOpPolicy},
				{ PolicyNames.RefitRetryPolicy, refitRetryPolicy}
			};
			services.AddPolicyRegistry(registry);

			#endregion 

			services.AddControllers();

			services.AddRefitClient<IAzureDevOpsApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				});
			//.AddPolicyHandlerFromRegistry("defaultRetry")
			//.AddPolicyHandlerFromRegistry((reg, request) =>
			//	{
			//		if(request.Method == HttpMethod.Get)
			//			return reg.Get<IAsyncPolicy<HttpResponseMessage>>(PolicyNames.DefaultRetry);

			//		if(request.RequestUri.LocalPath.StartsWith("matchWhatYouNeed"))
			//			return reg.Get<IAsyncPolicy<HttpResponseMessage>>(PolicyNames.DefaultWaitAndRetry);

			//		return reg.Get<IAsyncPolicy<HttpResponseMessage>>(PolicyNames.NoOp);
			//	});
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
