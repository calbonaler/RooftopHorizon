
namespace Saruna
{
	public class AuthorizationToken
	{
		public AuthorizationToken(string token, string secret)
		{
			Key = token;
			Secret = secret;
		}

		public string Key { get; private set; }

		public string Secret { get; private set; }
	}

	public interface IAccount
	{
		AuthorizationToken Consumer { get; }

		AuthorizationToken Access { get; }
	}

	class AccountStub : IAccount
	{
		public AccountStub(AuthorizationToken consumer, AuthorizationToken access)
		{
			Consumer = consumer;
			Access = access;
		}

		public AuthorizationToken Consumer { get; private set; }

		public AuthorizationToken Access { get; private set; }
	}
}
