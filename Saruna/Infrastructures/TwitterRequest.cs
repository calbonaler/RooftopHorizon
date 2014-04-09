using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Saruna.Infrastructures
{
	public class TwitterRequest
	{
		static readonly Random random = new Random();

		public TokenSecretPair ConsumerKey { get; set; }

		public TokenSecretPair AccessToken { get; set; }

		string CreateSignature(TwitterRequestContent content, IEnumerable<KeyValuePair<string, string>> authorization)
		{
			authorization = authorization.Concat(content.AuthorizationParameters);
			if (!content.IsMultipart)
				authorization = authorization.Concat(content.StringParameters);
			var parameterString = string.Join("&",
				authorization.Select(kvp => new KeyValuePair<string, string>(
					PercentEncoding.Encode(kvp.Key),
					PercentEncoding.Encode(kvp.Value)
				)).OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key + "=" + kvp.Value)
			);
			var signatureBased = content.Method.Method + "&" + PercentEncoding.Encode(content.Url) + "&" + PercentEncoding.Encode(parameterString);
			var signingKey = PercentEncoding.Encode(ConsumerKey.Secret) + "&";
			if (AccessToken != null)
				signingKey += PercentEncoding.Encode(AccessToken.Secret);
			using (var hmacsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.ASCII.GetBytes(signingKey)))
				return Convert.ToBase64String(hmacsha1.ComputeHash(Encoding.ASCII.GetBytes(signatureBased)));
		}

		void Authorize(HttpRequestMessage request, TwitterRequestContent content)
		{
			List<KeyValuePair<string, string>> oauth = new List<KeyValuePair<string, string>>();
			byte[] bs = new byte[32];
			random.NextBytes(bs);
			oauth.Add(new KeyValuePair<string, string>("oauth_consumer_key", PercentEncoding.Encode(ConsumerKey.Token)));
			oauth.Add(new KeyValuePair<string, string>("oauth_nonce", string.Concat(Convert.ToBase64String(bs).Where(ch => char.IsLetterOrDigit(ch)))));
			oauth.Add(new KeyValuePair<string, string>("oauth_signature_method", "HMAC-SHA1"));
			oauth.Add(new KeyValuePair<string, string>("oauth_timestamp", DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds.ToString("0")));
			if (AccessToken != null)
				oauth.Add(new KeyValuePair<string, string>("oauth_token", PercentEncoding.Encode(AccessToken.Token)));
			oauth.Add(new KeyValuePair<string, string>("oauth_version", "1.0"));
			oauth.Add(new KeyValuePair<string, string>("oauth_signature", CreateSignature(content, oauth)));
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth",
				string.Join(", ",
					oauth.Concat(content.AuthorizationParameters).Select(kvp => PercentEncoding.Encode(kvp.Key) + "=\"" + PercentEncoding.Encode(kvp.Value) + "\"")
				)
			);
		}

		public async Task<HttpResponseMessage> GetResponseAsync(TwitterRequestContent content)
		{
			using (HttpClient client = new HttpClient(new HttpClientHandler()
			{
				AutomaticDecompression = content.UseCompression ? DecompressionMethods.GZip | DecompressionMethods.Deflate : DecompressionMethods.None
			}))
			using (HttpRequestMessage request = content.CreateRequestMessage())
			{
				if (content.UseCompression)
				{
					request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
					request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
				}
				request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("RooftopHorizon", "1.0"));
				request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));
				request.Headers.Connection.Add("Keep-Alive");
				Authorize(request, content);
				HttpResponseMessage response = null;
				HttpResponseMessage temp = null;
				try
				{
					temp = await client.SendAsync(request, content.CompletionOption);
					if (!temp.IsSuccessStatusCode)
					{
						if (content.CompletionOption == HttpCompletionOption.ResponseHeadersRead)
							temp.EnsureSuccessStatusCode();
						else
							throw new TwitterException("要求が正常に受理されませんでした。", temp.StatusCode, await temp.Content.ReadAsStringAsync());
					}
					response = temp;
					temp = null;
				}
				finally
				{
					if (temp != null)
						temp.Dispose();
				}
				return response;
			}
		}

		public async Task<XElement> GetXmlAsync(TwitterRequestContent content)
		{
			using (var res = await GetResponseAsync(content))
			using (var stream = await res.Content.ReadAsStreamAsync())
			using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, System.Xml.XmlDictionaryReaderQuotas.Max))
				return XElement.Load(reader);
		}
	}
}
