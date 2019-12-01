using System;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Registry;
using PollyCommons;

namespace PollyCache.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : ControllerBase
	{
		readonly IPolicyRegistry<string> policyRegistry;
		readonly IHttpClientFactory clientFactory;
		readonly AsyncCachePolicy<HttpResponseMessage> cachePolicy;

		public ProjectsController(IHttpClientFactory clientFactory, IPolicyRegistry<string> policyRegistry)
		{
			this.clientFactory = clientFactory;
			this.policyRegistry = policyRegistry;
			//this.cachePolicy = policyRegistry.Get<AsyncCachePolicy<HttpResponseMessage>>(PolicyNames.CachePolicy);
			this.cachePolicy = policyRegistry.Get<AsyncCachePolicy<HttpResponseMessage>>(PolicyNames.CacheOnlyOkPolicy);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var client = clientFactory.CreateClient("AzureDevOpsCache");

			var requestEndpoint = $"api/azuredevops/projects/{id}";
			
			var context = new Polly.Context($"GetProjectById-{id}");

			var response = await cachePolicy.ExecuteAsync(
				c =>
				{
					return client.GetAsync(requestEndpoint);
				}, context);

			return !response.IsSuccessStatusCode ?
				StatusCode((int)response.StatusCode, response.ReasonPhrase) :
				Ok(JsonConvert.DeserializeObject<Project>(await response.Content.ReadAsStringAsync()));

			//var request = new HttpRequestMessage(HttpMethod.Get, requestEndpoint);
			//request.SetPolicyExecutionContext(context);
			//var response = await cachePolicy.ExecuteAsync(
			//	() =>
			//	{
			//		return client.SendAsync(request);
			//	});

			//return !response.IsSuccessStatusCode ?
			//	StatusCode((int)response.StatusCode, response.ReasonPhrase) :
			//	Ok(JsonConvert.DeserializeObject<Project>(await response.Content.ReadAsStringAsync()));
		}
	}
}
