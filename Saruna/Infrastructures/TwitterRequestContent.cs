using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Saruna.Infrastructures
{
	public class TwitterRequestContent
	{
		public TwitterRequestContent(HttpMethod method, string url)
		{
			Method = method;
			Url = url;
			CompletionOption = HttpCompletionOption.ResponseContentRead;
			IsMultipart = false;
			UseCompression = true;
		}

		Dictionary<string, IList<object>> normal = new Dictionary<string, IList<object>>();
		Dictionary<string, string> authorization = new Dictionary<string, string>();

		public HttpMethod Method { get; private set; }

		public string Url { get; private set; }

		public bool UseCompression { get; set; }

		public HttpCompletionOption CompletionOption { get; set; }

		public bool IsMultipart { get; set; }

		public void AddParameter(string name, byte[] value) { AddParameterInternal(name, value); }

		public void AddParameter(string name, Stream value) { AddParameterInternal(name, value); }

		void AddParameterInternal(string name, object value)
		{
			if (!name.StartsWith("oauth_"))
			{
				IList<object> values;
				if (!normal.TryGetValue(name, out values))
					normal[name] = values = new List<object>();
				values.Add(value);
			}
		}

		public string GetParameter(string name)
		{
			if (name.StartsWith("oauth_"))
				return authorization[name];
			else
				return (string)normal[name][0];
		}

		public void SetParameter(string name, string value)
		{
			if (name.StartsWith("oauth_"))
				authorization[name] = value;
			else
			{
				IList<object> values;
				if (!normal.TryGetValue(name, out values))
					normal[name] = values = new List<object>();
				else
					values.Clear();
				values.Add(value);
			}
		}

		public void RemoveParameter(string name)
		{
			if (name.StartsWith("oauth_"))
				authorization.Remove(name);
			else
				normal.Remove(name);
		}

		public IReadOnlyDictionary<string, string> AuthorizationParameters { get { return authorization; } }

		public IEnumerable<KeyValuePair<string, string>> StringParameters { get { return normal.SelectMany(kvp => kvp.Value.OfType<string>().Select(value => new KeyValuePair<string, string>(kvp.Key, value))); } }

		public HttpRequestMessage CreateRequestMessage()
		{
			HttpRequestMessage request = null;
			HttpRequestMessage temp = null;
			try
			{
				temp = new HttpRequestMessage();
				temp.Method = Method;
				if (Method == HttpMethod.Get)
				{
					var builder = new UriBuilder(Url);
					if (normal.Count > 0)
						builder.Query = string.Join("&", StringParameters.Select(kvp => PercentEncoding.Encode(kvp.Key) + "=" + PercentEncoding.Encode(kvp.Value)));
					temp.RequestUri = builder.Uri;
				}
				else
				{
					temp.RequestUri = new Uri(Url);
					if (IsMultipart)
					{
						var multipart = new MultipartFormDataContent();
						foreach (var kvp in normal)
						{
							foreach (var value in kvp.Value)
							{
								string str = value as string;
								if (str != null)
									multipart.Add(new StringContent(str), kvp.Key);
								else
								{
									byte[] array = value as byte[];
									if (array != null)
										multipart.Add(new ByteArrayContent(array), kvp.Key);
									else
									{
										Stream stream = value as Stream;
										if (stream != null)
											multipart.Add(new StreamContent(stream), kvp.Key);
									}
								}
							}
						}
						temp.Content = multipart;
					}
					else
						temp.Content = new FormUrlEncodedContent(StringParameters);
				}
				request = temp;
				temp = null;
			}
			finally
			{
				if (temp != null)
					temp.Dispose();
			}
			return request;
		}
	}
}
