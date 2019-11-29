using System;
using System.Net.Http;
using System.Threading.Tasks;
using Api;
using Api.Requests;
using Microsoft.AspNetCore.Mvc;

namespace WithoutPolly.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : ControllerBase
	{
		readonly IHttpClientFactory httpClientFactory;
		readonly IAzureDevOpsApi azureDevOpsApi;

		public ProjectsController(IHttpClientFactory httpClientFactory, IAzureDevOpsApi azureDevOpsApi)
		{
			this.httpClientFactory = httpClientFactory;
			this.azureDevOpsApi = azureDevOpsApi;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var httpClient = httpClientFactory.CreateClient("AzureDevOps");
			
			var response = await azureDevOpsApi.GetProject(id);

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
