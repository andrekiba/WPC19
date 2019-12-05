using System;
using Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

namespace WithoutPolly
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

			//services.AddHttpClient("AzureDevOps", client =>
			//{
			//	client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
			//	client.DefaultRequestHeaders.Add("Accept", "application/json");
			//});

			//services.AddHttpClient("AzureDevOps", client =>
			//	{
			//		client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
			//		client.DefaultRequestHeaders.Add("Accept", "application/json");
			//	})
			//	.AddTypedClient(RestService.For<IAzureDevOpsApi>);

			services.AddRefitClient<IAzureDevOpsApi>()
			//services.AddRefitClient<IAzureDevOpsApi>(new RefitSettings())
			.ConfigureHttpClient(client =>
			{
				client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
				client.DefaultRequestHeaders.Add("Accept", "application/json");
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
