using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Saruna.Infrastructures
{
	public static class RequestSender
	{
		public static string ProductName { get; set; }

		public static Version ProductVersion { get; set; }

		public static async Task<DisposableWrapper<HttpResponseMessage>> GetResponseAsync(IAccount authData, RequestContent content, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
		{
			using (var request = content.CreateRequestMessage())
			{
				HttpClient client = null;
				try
				{
					client = new HttpClient(new OAuthHandler(new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }, authData.Consumer, authData.Access, content.OAuthParameters), true);
					if (completionOption == HttpCompletionOption.ResponseHeadersRead)
						client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
					request.Headers.ConnectionClose = false;
					request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue(ProductName ?? string.Empty, ProductVersion != null ? ProductVersion.ToString(2) : string.Empty));
					var response = await client.SendAsync(request, completionOption).ConfigureAwait(false);
					if (!response.IsSuccessStatusCode)
						throw new TwitterException("要求が正常に受理されませんでした。", response.StatusCode, completionOption == HttpCompletionOption.ResponseContentRead ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : string.Empty);
					var result = new DisposableWrapper<HttpResponseMessage>(response, client);
					client = null;
					return result;
				}
				finally
				{
					if (client != null)
						client.Dispose();
				}
			}
		}

		public static async Task SendAsync(IAccount authData, RequestContent content) { using (await GetResponseAsync(authData, content).ConfigureAwait(false)) { } }

		public static async Task<XElement> GetXmlAsync(IAccount authData, RequestContent content)
		{
			using (var res = await GetResponseAsync(authData, content, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false))
			using (var stream = await res.Content.Content.ReadAsStreamAsync().ConfigureAwait(false))
			using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, System.Xml.XmlDictionaryReaderQuotas.Max))
				return XElement.Load(reader);
		}
	}

	public sealed class DisposableWrapper<T> : IDisposable where T : IDisposable
	{
		public DisposableWrapper(T content, IDisposable disposable)
		{
			_disposable = disposable;
			Content = content;
		}

		IDisposable _disposable;

		public void Dispose()
		{
			_disposable.Dispose();
			Content.Dispose();
		}

		public T Content { get; private set; }
	}
}
