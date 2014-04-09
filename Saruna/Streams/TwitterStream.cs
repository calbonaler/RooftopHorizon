using System;
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
		public TwitterStream(TwitterRequest request, TwitterRequestContent content)
		{
			context = SynchronizationContext.Current;
			refreshRequest = request;
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
		TwitterRequest refreshRequest;
		TwitterRequestContent refreshContent;
		StreamNoticeDistributor distributor;

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public async void ConnectAsync()
		{
			if (connected)
				return;
			connected = true;
			var response = await refreshRequest.GetResponseAsync(refreshContent);
			var stream = await response.Content.ReadAsStreamAsync();
			new Action<IDisposable, Stream>(MainLoop).BeginInvoke(response, stream, null, null);
		}

		public void Discoonect() { connected = false; }

		public bool IsConnected { get { return connected; } }

		void MainLoop(IDisposable response, Stream stream)
		{
			using (response)
			using (var streamReader = new StreamReader(stream, Encoding.UTF8))
			{
				string s;
				while ((s = streamReader.ReadLine()) != null && connected)
				{
					if (string.IsNullOrEmpty(s) || MessageReceived == null)
						continue;
					using (var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(s), new System.Xml.XmlDictionaryReaderQuotas()))
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
