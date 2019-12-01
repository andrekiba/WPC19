using System;
using System.Net.Http;
using System.Threading.Tasks;
using Api;
using Api.Requests;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Registry;
using PollyCommons;
using Refit;

namespace PollyRegistry.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : ControllerBase
	{
		readonly IAzureDevOpsApi azureDevOpsApi;
		readonly IPolicyRegistry<string> policyRegistry;

		public ProjectsController(IAzureDevOpsApi azureDevOpsApi, IPolicyRegistry<string> policyRegistry)
		{
			this.azureDevOpsApi = azureDevOpsApi;
			this.policyRegistry = policyRegistry;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			//policy inside the MessageHandler
			//var response = await azureDevOpsApi.GetProject(id);

			//policy injected from registry
			var retryPolicy = policyRegistry.Get<IAsyncPolicy<ApiResponse<Project>>>(PolicyNames.RefitRetryPolicy);
			var response  = await retryPolicy.ExecuteAsync(() => azureDevOpsApi.GetProject(id));

			return !response.IsSuccessStatusCode ?
				StatusCode((int)response.StatusCode, response.Error.Content) :
				Ok(response.Content);
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
