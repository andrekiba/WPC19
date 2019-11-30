using System.Threading.Tasks;

namespace PollyReauth.Auth
{
	public interface ITokenProvider
	{
		Task<string> GetToken();
	}
}
