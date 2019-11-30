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
			IAsyncPolicy<HttpResponseMessage> retryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
				.RetryAsync(2);

			var defaultProject = new Project
			{
				Id = Guid.Parse("3ec8c89e-7222-4d4e-8d7e-edfdc83c34af"),
				Name = "Default Project"
			};

			IAsyncPolicy<HttpResponseMessage> fallbackPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
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

			services.AddControllers();

			services.AddRefitClient<IAzureDevOpsApi>(/* RefitSettings
				{
					ContentSerializer = new TestJsonContentSerializer()
				}*/)
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

	public class TestJsonContentSerializer : IContentSerializer
	{
		readonly Lazy<JsonSerializerSettings> jsonSerializerSettings;

		public TestJsonContentSerializer() : this(null) { }

		public TestJsonContentSerializer(JsonSerializerSettings jsonSerializerSettings)
		{
			this.jsonSerializerSettings = new Lazy<JsonSerializerSettings>(() =>
			{
				if (jsonSerializerSettings == null)
				{
					return JsonConvert.DefaultSettings == null ? 
						new JsonSerializerSettings() : 
						JsonConvert.DefaultSettings();
				}
				return jsonSerializerSettings;
			});
		}

		public Task<HttpContent> SerializeAsync<T>(T item)
		{
			var content = new StringContent(JsonConvert.SerializeObject(item, jsonSerializerSettings.Value), Encoding.UTF8, "application/json");
			return Task.FromResult((HttpContent)content);
		}

		public async Task<T> DeserializeAsync<T>(HttpContent content)
		{
			var serializer = JsonSerializer.Create(jsonSerializerSettings.Value);

			await using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
			stream.Seek(0, SeekOrigin.Begin);
			using var reader = new StreamReader(stream);
			using var jsonTextReader = new JsonTextReader(reader);
			return serializer.Deserialize<T>(jsonTextReader);
		}
	}
}
