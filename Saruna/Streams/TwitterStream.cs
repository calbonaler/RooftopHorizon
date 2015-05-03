using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Saruna.Infrastructures;

namespace Saruna.Streams
{
	public class TwitterStream
	{
		public TwitterStream(IAccount authData, RequestContent content)
		{
			context = SynchronizationContext.Current;
			_authData = authData;
			refreshContent = content;
			distributor = new StreamNoticeDistributor();
			distributor.NoticeTypes.Add(typeof(TweetDeletionNotice));
			distributor.NoticeTypes.Add(typeof(DirectMessageDeletionNotice));
			distributor.NoticeTypes.Add(typeof(LocationDeletionNotice));
			distributor.NoticeTypes.Add(typeof(LimitNotice));
			distributor.NoticeTypes.Add(typeof(TweetWithheldNotice));
			distributor.NoticeTypes.Add(typeof(UserWithheldNotice));
			distributor.NoticeTypes.Add(typeof(DisconnectNotice));
			distributor.NoticeTypes.Add(typeof(StallNotice));
			distributor.NoticeTypes.Add(typeof(FriendListNotice));
			distributor.NoticeTypes.Add(typeof(EventNotice));
			distributor.NoticeTypes.Add(typeof(TooManyFollowsNotice));
			distributor.NoticeTypes.Add(typeof(ControlNotice));
		}

		volatile bool connected = false;
		SynchronizationContext context;
		IAccount _authData;
		RequestContent refreshContent;
		StreamNoticeDistributor distributor;

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public async void ConnectAsync()
		{
			if (connected)
				return;
			connected = true;
			var response = await RequestSender.GetResponseAsync(_authData, refreshContent, System.Net.Http.HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
			var stream = await response.Content.Content.ReadAsStreamAsync().ConfigureAwait(false);
			new Action<IDisposable, Stream>(MainLoop).BeginInvoke(response, stream, null, null);
		}

		public void Discoonect() { connected = false; }

		public bool IsConnected { get { return connected; } }

		void MainLoop(IDisposable response, Stream stream)
		{
			using (response)
			{
				List<byte> bytes = new List<byte>();
				int b;
				while ((b = stream.ReadByte()) >= 0 && connected)
				{
					if (b != '\r' && b != '\n')
					{
						bytes.Add((byte)b);
						continue;
					}
					if (bytes.Count <= 0 || MessageReceived == null)
						continue;
					Console.WriteLine(Encoding.UTF8.GetString(bytes.ToArray()));
					using (var reader = JsonReaderWriterFactory.CreateJsonReader(bytes.ToArray(), new System.Xml.XmlDictionaryReaderQuotas()))
					{
						var xml = XElement.Load(reader);
						var directMessage = xml.Element("direct_message");
						object value = null;
						if (directMessage != null)
							value = DirectMessage.FromXml(directMessage);
						else if (xml.Element("text") != null)
							value = Tweet.FromXml(xml);
						else
							value = distributor.Distribute(xml);
						context.Post(
							ea => MessageReceived(this, (MessageReceivedEventArgs)ea),
							new MessageReceivedEventArgs(xml, value)
						);
						Thread.Sleep(0);
					}
					bytes.Clear();
				}
				connected = false;
			}
		}
	}

	public class MessageReceivedEventArgs : EventArgs
	{
		public MessageReceivedEventArgs(XElement message, object value)
		{
			Message = message;
			Value = value;
		}

		public XElement Message { get; private set; }

		public object Value { get; private set; }
	}
}
