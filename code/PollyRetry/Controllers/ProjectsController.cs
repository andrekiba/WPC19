using System;
using System.Net.Http;
using System.Threading.Tasks;
using Api;
using Api.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PollyRetry.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : Controller
	{
		readonly IHttpClientFactory httpClientFactory;
		readonly IAzureDevOpsApi azureDevOpsApi;
		readonly ILogger<ProjectsController> logger;
		
		public ProjectsController(IHttpClientFactory httpClientFactory, IAzureDevOpsApi azureDevOpsApi, ILogger<ProjectsController> logger)
		{
			this.httpClientFactory = httpClientFactory;
			this.azureDevOpsApi = azureDevOpsApi;
			this.logger = logger;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			//var httpClient = httpClientFactory.CreateClient(nameof(IAzureDevOpsApi));

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
