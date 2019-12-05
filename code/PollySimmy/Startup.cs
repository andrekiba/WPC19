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
using Polly.Contrib.Simmy;
using Polly.Contrib.Simmy.Behavior;
using Polly.Contrib.Simmy.Fault;
using Polly.Wrap;
using Refit;

namespace PollySimmy
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

			#region Behavior

			AsyncInjectBehaviourPolicy behaviorPolicy = MonkeyPolicy.InjectBehaviourAsync(
				async () =>
				{
					await Task.Delay(100);
					Debug.WriteLine("Do what you want here");
				},
				1, // 25% of the time
				async () => await Task.FromResult(true));

			services.AddSingleton(behaviorPolicy);

			#endregion

			#region Latency

			var latencyPolicy = MonkeyPolicy.InjectLatencyAsync<HttpResponseMessage>(
				TimeSpan.FromSeconds(5), // delay di 5 secondi
				0.5, // 50% delle richieste
				() => true); // abilitata

			IAsyncPolicy<HttpResponseMessage> timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(2);

			#endregion

			#region Fault

			var faultHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
			{
				Content = new StringContent("Simmy Internal Server Error")
			};

			AsyncInjectOutcomePolicy<HttpResponseMessage> faultPolicy = MonkeyPolicy.InjectFaultAsync(
				faultHttpResponseMessage,
				injectionRate: .5,
				enabled: () => true
			);

			#endregion

			#region Retry

			IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.Or<Exception>()
				.RetryAsync(2, onRetry: (response, retryCount) =>
				{
					if (response.Result?.StatusCode != HttpStatusCode.InternalServerError)
					{
						//Do somethig
					}
					else
					{
						//Log something
					}
				});

			#endregion 

			AsyncPolicyWrap<HttpResponseMessage> faultAndRetry = Policy.WrapAsync(retryPolicy, faultPolicy);

			IAsyncPolicy<HttpResponseMessage> latencyTimeoutAndRetry = Policy.WrapAsync(
				retryPolicy, timeoutPolicy, latencyPolicy);

			services.AddRefitClient<IAzureDevOpsApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				.AddPolicyHandler(faultAndRetry);
				//.AddPolicyHandler(latencyTimeoutAndRetry);
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
