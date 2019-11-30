using System;
using System.Threading.Tasks;

namespace PollyReauth.Auth
{
	public class TokenProvider : ITokenProvider
	{
		public async Task<string> GetToken()
		{
			await Task.Delay(200);
			var token = Guid.NewGuid().ToString();
			return token;
		}
	}
}
