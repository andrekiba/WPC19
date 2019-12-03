using System;
using System.Threading.Tasks;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace PollyCircuitBreaker.Controllers
{
	[ApiController]
	[Route("api/alm")]
	public class AlmController : ControllerBase
	{
		[HttpGet("projects/{id}")]
		public async Task<IActionResult> GetProject(Guid id)
		{
			await Task.Delay(100);

			return Ok(new Project
			{
				Id = id,
				Name = "ALM_WPC19"
			});
		}
	}
}
