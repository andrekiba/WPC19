using System;
using System.Threading.Tasks;
using Api;
using Api.Requests;
using Microsoft.AspNetCore.Mvc;
using Polly.Contrib.Simmy.Behavior;

namespace PollySimmy.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : ControllerBase
	{
		readonly IAzureDevOpsApi azureDevOpsApi;
		readonly AsyncInjectBehaviourPolicy behaviorPolicy;

		public ProjectsController(IAzureDevOpsApi azureDevOpsApi, AsyncInjectBehaviourPolicy behaviorPolicy)
		{
			this.azureDevOpsApi = azureDevOpsApi;
			this.behaviorPolicy = behaviorPolicy;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var response = await behaviorPolicy.ExecuteAsync(() => azureDevOpsApi.GetProject(id));

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
