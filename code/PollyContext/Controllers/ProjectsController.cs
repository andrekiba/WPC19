using System;
using System.Net.Http;
using System.Threading.Tasks;
using Api;
using Api.Requests;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using PollyCommons;
using PollyCommons.Extensions;

namespace PollyContext.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : ControllerBase
	{
		readonly IHttpClientFactory clientFactory;
		readonly IAzureDevOpsApi azureDevOpsApi;
		readonly ILogger<ProjectsController> logger;

		public ProjectsController(IHttpClientFactory clientFactory,
			//IAzureDevOpsApi azureDevOpsApi,
			ILogger<ProjectsController> logger)
		{
			this.clientFactory = clientFactory;
			//this.azureDevOpsApi = azureDevOpsApi;
			this.logger = logger;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var client = clientFactory.CreateClient("AzureDevOpsWithoutRefit");
			
			var requestEndpoint = $"api/azuredevops/projects/{id}";
			var request = new HttpRequestMessage(HttpMethod.Get, requestEndpoint);
			
			var context = new Polly.Context($"{nameof(ProjectsController)}-{nameof(Get)}-{Guid.NewGuid()}")
				.WithLogger(logger);
			
			context.Add(PolicyContextKeys.MyTestKey, "ciao!");
			
			request.SetPolicyExecutionContext(context);

			var response = await client.SendAsync(request);

			return !response.IsSuccessStatusCode ?
				StatusCode((int)response.StatusCode, response.ReasonPhrase) :
				Ok(JsonConvert.DeserializeObject<Project>(await response.Content.ReadAsStringAsync()));

			//var response = await azureDevOpsApi.GetProject(id);
			//return !response.IsSuccessStatusCode ?
			//	StatusCode((int)response.StatusCode, response.Error.Content) :
			//	Ok(response.Content);
		}

		[HttpPost]
		public async Task<IActionResult> Create(CreateProject createProject)
		{
			var response = await azureDevOpsApi.CreateProject(createProject);
			
			return !response.IsSuccessStatusCode ? 
				StatusCode((int)response.StatusCode, response.Error) : 
				Ok(response.Content);
		}
	}
}
