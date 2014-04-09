using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;

namespace Saruna
{
	public class DirectMessage : IDirectMessageIdentifier, INotifyPropertyChanged, IMessage, IUpdateable<DirectMessage>
	{
		public static DirectMessage FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new DirectMessage();
			res.CreatedTime = DateTime.ParseExact(element.Element("created_at").Cast<string>(), "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture.DateTimeFormat);
			res.Entities = TweetEntities.FromXml(element.Element("entities"));
			res.Id = element.Element("id").Cast<long>();
			res.Text = element.Element("text").Cast<string>();
			res.Recipient = User.FromXml(element.Element("recipient"));
			res.Sender = User.FromXml(element.Element("sender"));
			return res;
		}

		#region CreatedTime
		DateTime m_CreatedTime;
		public DateTime CreatedTime
		{
			get { return m_CreatedTime; }
			private set
			{
				if (value != m_CreatedTime)
				{
					m_CreatedTime = value;
					OnPropertyChanged("CreatedTime");
				}
			}
		}
		#endregion

		#region Entities
		TweetEntities m_Entities;
		public TweetEntities Entities
		{
			get { return m_Entities; }
			private set
			{
				if (value != m_Entities)
				{
					m_Entities = value;
					OnPropertyChanged("Entity");
				}
			}
		}
		#endregion

		#region Id
		long m_Id;
		public long Id
		{
			get { return m_Id; }
			private set
			{
				if (value != m_Id)
				{
					m_Id = value;
					OnPropertyChanged("Id");
				}
			}
		}
		#endregion

		#region Text
		string m_Text;
		public string Text
		{
			get { return m_Text; }
			private set
			{
				if (value != m_Text)
				{
					m_Text = value;
					OnPropertyChanged("Text");
				}
			}
		}
		#endregion

		#region Recipient
		User m_Recipient;
		public User Recipient
		{
			get { return m_Recipient; }
			private set
			{
				if (value != m_Recipient)
				{
					m_Recipient = value;
					OnPropertyChanged("Recipient");
				}
			}
		}
		#endregion

		#region Sender
		User m_Sender;
		public User Sender
		{
			get { return m_Sender; }
			private set
			{
				if (value != m_Sender)
				{
					m_Sender = value;
					OnPropertyChanged("Sender");
				}
			}
		}
		#endregion
		
		User IMessage.User { get { return Sender; } }

		public override string ToString() { return Text; }

		public void Update(DirectMessage item)
		{
			if (item == null)
				return;
			CreatedTime = item.CreatedTime;
			Entities = item.Entities;
			Id = item.Id;
			Text = item.Text;
			Recipient = item.Recipient;
			Sender = item.Sender;
		}

		public event PropertyChangedEventHandler PropertyChanged;
	
		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
