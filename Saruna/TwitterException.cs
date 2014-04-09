using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Saruna
{
	[Serializable]
	public class TwitterException : Exception
	{
		public TwitterException() { }
		public TwitterException(string message) : base(message) { }
		public TwitterException(string message, HttpStatusCode statusCode, string response) : this(message, statusCode, response, null) { }
		public TwitterException(string message, Exception inner) : base(message, inner) { }
		public TwitterException(string message, HttpStatusCode statusCode, string response, Exception inner) : base(message, inner)
		{
			StatusCode = statusCode;
			Response = response;
		}
		protected TwitterException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public HttpStatusCode StatusCode { get; private set; }
		public string Response { get; private set; }

		public IEnumerable<ErrorDescription> GetDescriptions()
		{
			using (var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(Response), System.Xml.XmlDictionaryReaderQuotas.Max))
				return System.Xml.Linq.XElement.Load(reader).Element("errors").Elements().Select(x => ErrorDescription.FromXml(x));
		}
	}

	public class ErrorDescription
	{
		public string Message { get; private set; }

		public int Code { get; private set; }

		public static ErrorDescription FromXml(System.Xml.Linq.XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new ErrorDescription();
			res.Message = element.Element("message").Cast<string>();
			res.Code = element.Element("code").Cast<int>();
			return res;
		}
	}
}
