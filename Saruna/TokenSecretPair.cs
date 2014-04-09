
namespace Saruna
{
	public class TokenSecretPair
	{
		public TokenSecretPair(string token, string secret)
		{
			Token = token;
			Secret = secret;
		}

		public string Token { get; private set; }

		public string Secret { get; private set; }
	}
}
