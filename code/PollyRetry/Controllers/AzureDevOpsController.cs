using System;
using System.Net;
using System.Threading.Tasks;
using Api.Requests;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PollyRetry.Controllers
{
	[ApiController]
	[Route("api/azuredevops")]
	public class AzureDevOpsController : Controller
	{
		static int requestCount;
		readonly ILogger<AzureDevOpsController> logger;

		public AzureDevOpsController(ILogger<AzureDevOpsController> logger)
		{
			this.logger = logger;
		}

		//[HttpGet("api/azuredevops/projects/{id}")]
		[HttpGet("/projects/{id}")]
		public async Task<IActionResult> GetProject(Guid id)
		{
			await Task.Delay(100); 
			requestCount++;

			return requestCount % 3 == 0 ? 
				Ok(new Project
				{
					Id = id,
					Name = "WPC2019"
				}): 
				StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");
		}

		//[HttpPost("api/azuredevops/projects")]	
		[HttpPost("/projects")]
		public async Task<IActionResult> CreateProject(CreateProject createProject)
		{
			await Task.Delay(100);
			//requestCount++;

			return Ok(new Project
			{
				Id = Guid.NewGuid(),
				Name = createProject.Name
			});
		}
	}
}
