using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace PollyRetry.Services
{
	public class MicroBService : IMicroBService
	{
		#region Fields

		readonly IMicroBApi api;

		#endregion

		#region Constructor

		public MicroBService(IHttpClientFactory clientFactory)
		{
			var client = clientFactory.CreateClient("MicroBClient");
			api = RestService.For<IMicroBApi>(client);
		}

		#endregion

		#region Methods

		public async Task<int> Get(int id)
		{
			return await api.Get(id);
		}

		#endregion
	}
}
