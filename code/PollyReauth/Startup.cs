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
using PollyReauth.Auth;
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
			IAsyncPolicy<HttpResponseMessage> reauthPolicy = Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
				.RetryAsync(2, onRetry: (response, retryCount, context) =>
				{
					//Perform re-auth
					//if (context.ContainsKey("refreshToken"))
					//{
					//	var newAccessToken = await RefreshAccessToken(context["refreshToken"].ToString());
					//	if (newAccessToken != null)
					//		context["accessToken"] = newAccessToken;
						
					//}
				});

			services.AddControllers();

			services.AddMemoryCache();
			services.AddSingleton<ITokenProvider, TokenProvider>();

			var accessToken = Guid.NewGuid();

			//services
			//	.AddHttpClient("AzureDevOps", (serviceProvider, client) =>
			//	{
			//		client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
			//		client.DefaultRequestHeaders.Add("Accept", "application/json");
			//		client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
			//	})
			//	.AddTypedClient((client, serviceProvider) => RestService.For<IAzureDevOpsApi>(client, new RefitSettings
			//	{
			//		//AuthorizationHeaderValueGetter = serviceProvider.GetService<ITokenProvider>().GetToken
			//	}))
			//	.AddPolicyHandler(reauthPolicy);
			//	//.AddHttpMessageHandler(serviceProvider =>
			//	//{
			//	//	var tokenProvider = serviceProvider.GetService<ITokenProvider>();
			//	//	return new AuthMessageHandler(tokenProvider.GetToken);
			//	//});

			services
				.AddRefitClient<IAzureDevOpsApi>()
				.ConfigureHttpClient((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri(Configuration["AppSettings:AzureDevOpsApiAddress"]);
					client.DefaultRequestHeaders.Add("Accept", "application/json");
					client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
				})
				.AddPolicyHandler((serviceProvider, request) => Policy
					.HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
					.WaitAndRetryAsync(1,
						retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
						onRetry: async (response, retryCount, context) =>
						{
							//Attenzione che onRetry non è thread safe!!
							var newToken = await serviceProvider.GetService<ITokenProvider>().GetToken();
							if(request.Headers.Authorization != null)
								request.Headers.Authorization = new AuthenticationHeaderValue(request.Headers.Authorization.Scheme, newToken);
							else
								request.Headers.Add("Authorization", $"Bearer {newToken}");
						}));
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

	public class AuthMessageHandler : DelegatingHandler
	{
		readonly Func<Task<string>> getToken;

		public AuthMessageHandler(Func<Task<string>> getToken)
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
