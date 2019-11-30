using System;
using System.Threading.Tasks;
using Api;
using Api.Requests;
using Microsoft.AspNetCore.Mvc;

namespace PollyTimeout.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : ControllerBase
	{
		readonly IAzureDevOpsApi azureDevOpsApi;

		public ProjectsController(IAzureDevOpsApi azureDevOpsApi)
		{
			this.azureDevOpsApi = azureDevOpsApi;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			try
			{
				var response = await azureDevOpsApi.GetProject(id);
				return !response.IsSuccessStatusCode ?
					StatusCode((int)response.StatusCode, response.Error.Content) :
					Ok(response.Content);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
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
