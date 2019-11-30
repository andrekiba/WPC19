using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
		static readonly ConcurrentDictionary<string,ConcurrentBag<string>> expiredTokens = new ConcurrentDictionary<string, ConcurrentBag<string>>();

		[HttpGet("projects/{id}")]
		public async Task<IActionResult> GetProject(Guid id)
		{
			if (!Request.Headers.ContainsKey("Authorization") ||
			    !Request.Headers["Authorization"][0].StartsWith("Bearer "))
				return StatusCode((int)HttpStatusCode.Unauthorized, "You need an authorization token");
			
			var authToken = Request.Headers["Authorization"][0];

			if (expiredTokens.TryGetValue(id.ToString(), out var tokens) && tokens.Contains(authToken))
				return StatusCode((int)HttpStatusCode.Unauthorized, "You need a new authorization token");

			if (tokens is null)
				expiredTokens.TryAdd(id.ToString(), new ConcurrentBag<string> {authToken});
			else
				expiredTokens[id.ToString()].Add(authToken);

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
