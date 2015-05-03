using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Saruna.Infrastructures
{
	public class OAuthHandler : DelegatingHandler
	{
		public OAuthHandler(HttpMessageHandler innerHandler, AuthorizationToken consumer, AuthorizationToken access, IEnumerable<KeyValuePair<string, string>> additionalParameters)
			: base(innerHandler)
		{
			_consumer = consumer;
			_access = access;
			_additionalParameters = new List<KeyValuePair<string, string>>(additionalParameters);
		}

		static readonly Random _random = new Random();
		readonly AuthorizationToken _consumer;
		readonly AuthorizationToken _access;
		readonly IList<KeyValuePair<string, string>> _additionalParameters;

		static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string stringToParse)
		{
			return stringToParse.TrimStart('?').Split('&').Where(x => !string.IsNullOrEmpty(x)).Select(x =>
			{
				var items = x.Split('=');
				return new KeyValuePair<string, string>(Uri.UnescapeDataString(items[0].Replace("+", "%20")), Uri.UnescapeDataString(items.Length <= 1 ? string.Empty : items[1].Replace("+", "%20")));
			});
		}

		static async Task<string> CreateSignature(HttpRequestMessage request, string consumerSecret, string accessSecret, IEnumerable<KeyValuePair<string, string>> oauth)
		{
			oauth = oauth.Concat(ParseQueryString(request.RequestUri.GetComponents(UriComponents.Query, UriFormat.UriEscaped)));
			if (request.Content is FormUrlEncodedContent)
				oauth = oauth.Concat(ParseQueryString(await request.Content.ReadAsStringAsync().ConfigureAwait(false)));

			var parameterString = string.Join("&", oauth.Select(kvp => new
			{
				K = Uri.EscapeDataString(kvp.Key),
				V = Uri.EscapeDataString(kvp.Value)
			}).OrderBy(kvp => kvp.K).ThenBy(kvp => kvp.V).Select(kvp => kvp.K + "=" + kvp.V));

			var signatureBased = request.Method.Method + "&" +
				Uri.EscapeDataString(request.RequestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped)) + "&" +
				Uri.EscapeDataString(parameterString);

			var signingKey = Uri.EscapeDataString(consumerSecret) + "&" + Uri.EscapeDataString(accessSecret);
			using (var hmacsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
				return Convert.ToBase64String(hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(signatureBased)));
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			byte[] bs = new byte[32];
			_random.NextBytes(bs);
			var oauth = new List<KeyValuePair<string, string>>(_additionalParameters)
			{
				new KeyValuePair<string, string>("oauth_consumer_key", _consumer.Key),
				new KeyValuePair<string, string>("oauth_nonce", string.Concat(Convert.ToBase64String(bs).Where(ch => char.IsLetterOrDigit(ch)))),
				new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"),
				new KeyValuePair<string, string>("oauth_timestamp", DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString("0")),
				new KeyValuePair<string, string>("oauth_version", "1.0"),
			};
			if (_access != null)
				oauth.Add(new KeyValuePair<string, string>("oauth_token", _access.Key));

			oauth.Add(new KeyValuePair<string, string>("oauth_signature",
				await CreateSignature(request, _consumer.Secret, _access != null ? _access.Secret : string.Empty, oauth).ConfigureAwait(false)));

			request.Headers.Authorization = new AuthenticationHeaderValue("OAuth",
				string.Join(", ", oauth.Select(kvp => new
				{
					K = Uri.EscapeDataString(kvp.Key),
					V = Uri.EscapeDataString(kvp.Value)
				}).Select(kvp => kvp.K + "=\"" + kvp.V + "\"")));

			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}
