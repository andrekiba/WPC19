using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Refit;

namespace PollyCircuitBreaker
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
					Debug.WriteLine($"Retry {retryCount}");
				});

			IAsyncPolicy<HttpResponseMessage> breakerPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.CircuitBreakerAsync(2, TimeSpan.FromSeconds(30), OnBreak, OnReset, OnHalfOpen);

			IAsyncPolicy<HttpResponseMessage> advancedBreakerPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.AdvancedCircuitBreakerAsync(.5, TimeSpan.FromSeconds(30), 10, TimeSpan.FromSeconds(30), OnBreak, OnReset, OnHalfOpen);

			services.AddControllers();

			services.AddRefitClient<IAzureDevOpsApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				.AddPolicyHandler(retryPolicy)
				
				//DON'T DO THIS!! It creates a new circuit breaker instance per request --> most of the time is not what you want!
				//.AddPolicyHandler(request => HttpPolicyExtensions.HandleTransientHttpError()
				//	.CircuitBreakerAsync(2, TimeSpan.FromSeconds(60), OnBreak, OnReset, OnHalfOpen))
				
				//CORRECT --> Circuit Breaker is a stateful policy and can (most of the time must) be shared
				.AddPolicyHandler(breakerPolicy);

			services.AddRefitClient<IAlmApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				.AddPolicyHandler(retryPolicy)
				.AddPolicyHandler(breakerPolicy);
		}

		static void OnHalfOpen()
		{
			Debug.WriteLine("Connection half open");
		}

		static void OnReset(Context context)
		{
			Debug.WriteLine("Connection reset");
		}

		static void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, Context context)
		{
			Debug.WriteLine($"Connection break: {delegateResult.Result}, {delegateResult.Result}");
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
