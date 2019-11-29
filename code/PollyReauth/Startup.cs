using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Refit;

namespace PollyReauth
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
			IAsyncPolicy<HttpResponseMessage> reauthPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.RetryAsync(2, onRetry: async (response, retryCount, context) =>
				{
					if (response.Result.StatusCode == HttpStatusCode.Unauthorized)
					{
						//Perform re-auth
						if (context.ContainsKey("refreshToken"))
						{
							var newAccessToken = await RefreshAccessToken(context["refreshToken"].ToString());
							if (newAccessToken != null)
							{
								//await tokenRefreshed(newAccessToken);

								context["accessToken"] = newAccessToken;
							}
						}
					}
				});

			services.AddControllers();

			services.AddHttpClient("AzureDevOps", client =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
				})
				.AddTypedClient(RestService.For<IAzureDevOpsApi>)
				//.AddHttpMessageHandler<AuthHttpClientHandler>()
				.AddPolicyHandler(reauthPolicy);
		}

		static async Task<string> RefreshAccessToken(string refreshToken)
		{
			//call the service to refresh the token
			await Task.Delay(100);
			return "UpdatedToken" + refreshToken;
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

	public class AuthHttpClientHandler : DelegatingHandler
	{
		readonly Func<Task<string>> getToken;

		public AuthHttpClientHandler(Func<Task<string>> getToken)
		{
			var func = getToken;
			this.getToken = func ?? throw new ArgumentNullException(nameof(getToken));
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var auth = request.Headers.Authorization;
			if (auth != null)
				request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, await getToken().ConfigureAwait(false));
			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}
