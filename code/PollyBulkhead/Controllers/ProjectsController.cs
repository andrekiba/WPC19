using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Api;
using Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Polly.Bulkhead;
using Refit;

namespace PollyBulkhead.Controllers
{
	[ApiController]
	[Route("api/projects")]
	public class ProjectsController : ControllerBase
	{
		static int requestCount;
		readonly IAzureDevOpsApi azureDevOpsApi;
		readonly AsyncBulkheadPolicy<ApiResponse<Project>> bulkHeadPolicy;

		public ProjectsController(IAzureDevOpsApi azureDevOpsApi, AsyncBulkheadPolicy<ApiResponse<Project>> bulkHeadPolicy)
		{
			this.azureDevOpsApi = azureDevOpsApi;
			this.bulkHeadPolicy = bulkHeadPolicy;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			requestCount++;
			Debug.WriteLine($"Request count: {requestCount}");
			Debug.WriteLine($"Bulkhead available count: {bulkHeadPolicy.BulkheadAvailableCount}");
			Debug.WriteLine($"Queue available count: {bulkHeadPolicy.QueueAvailableCount}");

			var response = await bulkHeadPolicy.ExecuteAsync(() => azureDevOpsApi.GetProject(id)) ;

			return !response.IsSuccessStatusCode ?
				StatusCode((int)response.StatusCode, response.Error.Content) :
				Ok(response.Content);
		}
	}
}
