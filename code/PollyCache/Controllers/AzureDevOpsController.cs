using System;
using System.Net;
using System.Threading.Tasks;
using Api.Requests;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace PollyCache.Controllers
{
	[ApiController]
	[Route("api/azuredevops")]
	public class AzureDevOpsController : ControllerBase
	{
		static int requestCount;

		[HttpGet("projects/{id}")]
		public async Task<IActionResult> GetProject(Guid id)
		{
			await Task.Delay(100);
			
			requestCount++;

			if (requestCount % 3 == 0)
				return Ok(new Project
				{
					Id = id,
					Name = "WPC2019"
				});

			return StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");
		}
	}
}
