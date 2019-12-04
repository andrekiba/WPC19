using System;
using System.Threading.Tasks;
using Api.Requests;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace PollyBulkhead.Controllers
{
	[ApiController]
	[Route("api/azuredevops")]
	public class AzureDevOpsController : ControllerBase
	{
		[HttpGet("projects/{id}")]
		public async Task<IActionResult> GetProject(Guid id)
		{
			await Task.Delay(TimeSpan.FromSeconds(10));

			return Ok(new Project
			{
				Id = id,
				Name = "WPC2019"
			});
		}

		[HttpPost("projects")]
		public async Task<IActionResult> CreateProject(CreateProject createProject)
		{
			await Task.Delay(100);

			return Ok(new Project
			{
				Id = Guid.NewGuid(),
				Name = createProject.Name
			});
		}
	}
}
