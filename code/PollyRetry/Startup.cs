using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using PollyRetry.Services;
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
			IAsyncPolicy<HttpResponseMessage> httpRetryPolicy =
				Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).RetryAsync(3);

			services.AddControllers();

			//services.AddHttpClient("RemoteServer", client =>
			//{
			//	client.BaseAddress = new Uri("http://localhost:57696/api/");
			//	client.DefaultRequestHeaders.Add("Accept", "application/json");
			//}).AddPolicyHandler(httpRetryPolicy);

			services.AddRefitClient<IMicroBApi>()
				.ConfigureHttpClient(c => c.BaseAddress = new Uri(""))
				.AddPolicyHandler(httpRetryPolicy); ;
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
