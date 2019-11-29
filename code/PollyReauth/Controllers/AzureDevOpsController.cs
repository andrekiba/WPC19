using System;
using System.Net;
using System.Threading.Tasks;
using Api.Requests;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace PollyReauth.Controllers
{
	[ApiController]
	[Route("api/azuredevops")]
	public class AzureDevOpsController : ControllerBase
	{
		[HttpGet("projects/{id}")]
		public async Task<IActionResult> GetProject(Guid id)
		{
			if (!Request.Headers.ContainsKey("Authorization") || !Request.Headers["Authorization"][0].StartsWith("Bearer"))
				return StatusCode((int)HttpStatusCode.Unauthorized, "You need authorization");

			//var token = Request.Headers["Authorization"][0].Substring("Bearer ".Length);
			////do stuff...
			await Task.Delay(100);

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
