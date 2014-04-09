using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Saruna
{
	public sealed class TwitterSignInServer
	{
		HttpListener listener;
		string _succeededHtml;
		string _failedHtml;
		readonly object lockObject = new object();

		public event EventHandler<SignInRequestedEventArgs> SignInRequested;

		public void Start(string prefix, string succeededHtml, string failedHtml)
		{
			if (listener != null)
				listener.Abort();
			listener = new HttpListener();
			listener.Prefixes.Add(prefix);
			_succeededHtml = succeededHtml;
			_failedHtml = failedHtml;
			listener.Start();
			listener.BeginGetContext(OnContextReceived, null);
		}

		public void Stop()
		{
			if (listener != null)
			{
				listener.Close();
				listener = null;
			}
		}

		void OnContextReceived(IAsyncResult result)
		{
			lock (lockObject)
			{
				if (listener != null)
				{
					var context = listener.EndGetContext(result);
					string message = null;
					if (context.Request.QueryString.AllKeys.Contains("oauth_verifier"))
					{
						OnSignInRequested(new SignInRequestedEventArgs(context.Request.QueryString["oauth_verifier"]));
						message = _succeededHtml;
					}
					else
						message = _failedHtml;
					var bytes = Encoding.UTF8.GetBytes(message);
					context.Response.ContentLength64 = bytes.Length;
					context.Response.ContentType = "text/html; charset=UTF-8";
					using (var stream = context.Response.OutputStream)
						stream.Write(bytes, 0, bytes.Length);
					listener.BeginGetContext(OnContextReceived, null);
				}
			}
		}

		void OnSignInRequested(SignInRequestedEventArgs e)
		{
			if (SignInRequested != null)
				SignInRequested(this, e);
		}
	}

	public class SignInRequestedEventArgs : EventArgs
	{
		public SignInRequestedEventArgs(string verifier) { Verifier = verifier; }

		public string Verifier { get; private set; }
	}
}
