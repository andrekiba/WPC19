using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api;
using Api.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Polly;
using Refit;

namespace PollyFallback
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
				.RetryAsync(2);

			#endregion

			#region Fallback

			var defaultProject = new Project
			{
				Id = Guid.Parse("3ec8c89e-7222-4d4e-8d7e-edfdc83c34af"),
				Name = "Default Project"
			};

			IAsyncPolicy<HttpResponseMessage> fallbackPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.Or<Exception>()
				.FallbackAsync(
					//fallbackValue: new HttpResponseMessage(HttpStatusCode.OK)
					//{
					//	Content = new StringContent(JsonConvert.SerializeObject(defaultProject))
					//},
					fallbackAction: ct => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent(JsonConvert.SerializeObject(defaultProject))
					}),
					onFallbackAsync: delegateResult =>
					{
						if (delegateResult.Exception != null)
						{
							//
						}
						else if (delegateResult.Result != null)
						{
							//
						}
						return Task.CompletedTask;
					}
				);

			#endregion

			services.AddRefitClient<IAzureDevOpsApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				.AddPolicyHandler(fallbackPolicy)
				.AddTransientHttpErrorPolicy(p => p.RetryAsync(2));
				//.AddPolicyHandler(retryPolicy);
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
