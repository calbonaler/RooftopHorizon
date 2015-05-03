using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Saruna.Infrastructures
{
	public class RequestContent
	{
		public RequestContent(HttpMethod method, string url) : this(method, url, false) { }
		
		public RequestContent(HttpMethod method, string url, bool multipart)
		{
			_method = method;
			_url = url;
			_multipart = method == HttpMethod.Post && multipart;
		}

		static readonly Random _random = new Random();

		Dictionary<string, object> _normal = new Dictionary<string, object>();
		Dictionary<string, string> _oauth = new Dictionary<string, string>();
		bool _multipart;
		HttpMethod _method;
		string _url;

		public IReadOnlyDictionary<string, string> OAuthParameters { get { return _oauth; } }

		public string GetParameter(string name)
		{
			if (name.StartsWith("oauth_"))
				return _oauth[name];
			else
				return (string)_normal[name];
		}

		public void SetParameter(string name, string value)
		{
			if (name.StartsWith("oauth_"))
				_oauth[name] = value;
			else
				_normal[name] = value;
		}

		public void SetParameter(string name, byte[] value)
		{
			if (!name.StartsWith("oauth_"))
				_normal[name] = value;
		}

		public void RemoveParameter(string name)
		{
			if (name.StartsWith("oauth_"))
				_oauth.Remove(name);
			else
				_normal.Remove(name);
		}

		public HttpRequestMessage CreateRequestMessage()
		{
			HttpRequestMessage request = new HttpRequestMessage();
			request.Method = _method;
			request.RequestUri = new Uri(_url);
			if (!_multipart)
			{
				request.Content = new FormUrlEncodedContent(_normal.Where(x => x.Value is string).Select(x => new KeyValuePair<string, string>(x.Key, (string)x.Value)));
				ProcessRequest(request);
				return request;
			}
			var multipart = new MultipartFormDataContent();
			foreach (var kvp in _normal)
			{
				string str;
				byte[] bytes;
				if ((str = kvp.Value as string) != null)
					multipart.Add(new StringContent(str), kvp.Key);
				else if ((bytes = kvp.Value as byte[]) != null)
					multipart.Add(new ByteArrayContent(bytes), kvp.Key);
			}
			request.Content = multipart;
			return request;
		}

		static void ProcessRequest(HttpRequestMessage message)
		{
			if (message.Method != HttpMethod.Get)
				return;
			if (!(message.Content is FormUrlEncodedContent))
				throw new InvalidOperationException();
			var builder = new UriBuilder(message.RequestUri);
			builder.Query = message.Content.ReadAsStringAsync().Result;
			message.RequestUri = builder.Uri;
			message.Content = null;
		}
	}
}
