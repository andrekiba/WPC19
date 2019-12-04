using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Api;
using Api.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Bulkhead;
using Refit;

namespace PollyBulkhead
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
			//IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
			//	.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
			//	.RetryAsync(2, onRetry: (response, retryCount) =>
			//	{
			//		Debug.WriteLine($"Retry {retryCount}");
			//	});

			AsyncBulkheadPolicy<ApiResponse<Project>> bulkheadPolicy = Policy
				.BulkheadAsync<ApiResponse<Project>>(2, 4, onBulkheadRejectedAsync: context =>
				{
					Debug.WriteLine("Bulkhead reject execution");
					return Task.CompletedTask;
				});

			services.AddControllers();

			services.AddRefitClient<IAzureDevOpsApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				});
			//.AddPolicyHandler(bulkheadPolicy);

			services.AddSingleton(bulkheadPolicy);
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
