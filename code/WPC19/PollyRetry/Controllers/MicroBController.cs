using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PollyRetry.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class MicroBController : ControllerBase
	{
		static int requestCount;
		readonly ILogger<MicroAController> logger;

		public MicroBController(ILogger<MicroAController> logger)
		{
			this.logger = logger;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			await Task.Delay(100); 
			requestCount++;

			return requestCount % 3 == 0 ? 
				Ok(6) : 
				StatusCode((int)HttpStatusCode.InternalServerError, "Something went wrong");
		}
	}
}
