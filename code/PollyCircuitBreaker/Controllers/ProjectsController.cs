using System;
using System.Threading.Tasks;
using Api;
using Api.Requests;
using Microsoft.AspNetCore.Mvc;

namespace PollyCircuitBreaker.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : ControllerBase
	{
		readonly IAzureDevOpsApi azureDevOpsApi;
		readonly IAlmApi almApi;

		public ProjectsController(IAzureDevOpsApi azureDevOpsApi, IAlmApi almApi)
		{
			this.azureDevOpsApi = azureDevOpsApi;
			this.almApi = almApi;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var response = await azureDevOpsApi.GetProject(id);

			return !response.IsSuccessStatusCode ?
				StatusCode((int)response.StatusCode, response.Error.Content) :
				Ok(response.Content);
		}

		[HttpGet("alm/{id}")]
		public async Task<IActionResult> AlmGet(Guid id)
		{
			var response = await almApi.GetProject(id);

			return !response.IsSuccessStatusCode ?
				StatusCode((int)response.StatusCode, response.Error.Content) :
				Ok(response.Content);
		}
	}
}
