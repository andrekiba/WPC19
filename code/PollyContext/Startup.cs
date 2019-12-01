using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using PollyCommons;
using PollyCommons.Extensions;

namespace PollyContext
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
				.RetryAsync(2, onRetry: (response, retryCount, context) =>
				{
					var logger = context.GetLogger();
					logger?.LogError(response.Result.ReasonPhrase);
					logger?.LogInformation(context.OperationKey);
					logger?.LogInformation(context[PolicyContextKeys.MyTestKey] as string);
				});
			
			AsyncPolicy<HttpResponseMessage> waitAndRetryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.WaitAndRetryAsync(2, 
					retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
					onRetry: (response, timeSpan, context) =>
					{
						if (response.Result.StatusCode != HttpStatusCode.InternalServerError)
						{
							context.GetLogger()?.LogError(response.Result.ReasonPhrase);
						}
						else
						{
							//Log something else
						}
					});

			services.AddControllers();

			services
				//.AddRefitClient<IAzureDevOpsApi>()
				//.ConfigureHttpClient((serviceProvider, client) =>
				//{
				//	client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
				//	client.DefaultRequestHeaders.Add("Accept", "application/json");
				//})
				.AddHttpClient("AzureDevOpsWithoutRefit", client =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				//.AddTransientHttpErrorPolicy(p => p.RetryAsync(2));
				//.AddHttpMessageHandler((serviceProvider) =>
				//{
				//	var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();
				//	var context = new Polly.Context("AzureDevOps").WithLogger(logger);
				//	return new PollyContextMessageHandler(context);
				//})
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

	public class PollyContextMessageHandler : DelegatingHandler
	{
		readonly Polly.Context context;

		public PollyContextMessageHandler(Polly.Context context = null)
		{
			this.context = context;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (context != null)
				request.SetPolicyExecutionContext(context);

			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}
