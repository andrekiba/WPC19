using System.Threading.Tasks;
using Refit;

namespace PollyRetry.Services
{
	[Headers("Accept: application/json")]
	public interface IMicroBApi
	{
		[Get("/microb/{id}")]
		Task<int> Get(int id);
	}
}
