using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PollyRetry.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class MicroAController : ControllerBase
	{
		readonly ILogger<MicroAController> logger;

		public MicroAController(ILogger<MicroAController> logger)
		{
			this.logger = logger;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var httpClient = GetHttpClient();
			var requestEndpoint = $"microb/{id}";

			var response = await httpClient.GetAsync(requestEndpoint);

			if (!response.IsSuccessStatusCode)
				return StatusCode((int) response.StatusCode, response.Content.ReadAsStringAsync());
			
			var result = JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
			return Ok(result);
		}

		static HttpClient GetHttpClient()
		{
			var httpClient = new HttpClient {BaseAddress = new Uri(@"http://localhost:49818/api/")};
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			return httpClient;
		}
	}
}
