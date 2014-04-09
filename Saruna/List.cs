using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;

namespace Saruna
{
	public class List : IListIdentifier, INotifyPropertyChanged, IUpdateable<List>
	{
		public static List FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new List();
			res.Slug = element.Element("slug").Cast<string>();
			res.Name = element.Element("name").Cast<string>();
			res.CreatedTime = DateTime.ParseExact(element.Element("created_at").Cast<string>(), "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture.DateTimeFormat);
			var uri = element.Element("uri").Cast<string>();
			if (!string.IsNullOrEmpty(uri))
				res.Uri = new Uri(uri);
			res.SubscriberCount = element.Element("subscriber_count").Cast<int>();
			res.MemberCount = element.Element("member_count").Cast<int>();
			res.Mode = (ListMode)Enum.Parse(typeof(ListMode), element.Element("mode").Cast<string>(), true);
			res.Id = element.Element("id").Cast<long>();
			res.FullName = element.Element("full_name").Cast<string>();
			res.Description = element.Element("description").Cast<string>();
			res.User = User.FromXml(element.Element("user"));
			res.IsFollowing = element.Element("following").Cast<bool>();
			return res;
		}

		#region Slug
		string m_Slug;
		public string Slug
		{
			get { return m_Slug; }
			private set
			{
				if (value != m_Slug)
				{
					m_Slug = value;
					OnPropertyChanged("Slug");
				}
			}
		}
		#endregion

		#region Name
		string m_Name;
		public string Name
		{
			get { return m_Name; }
			private set
			{
				if (value != m_Name)
				{
					m_Name = value;
					OnPropertyChanged("Name");
				}
			}
		}
		#endregion

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

		#region Uri
		Uri m_Uri;
		public Uri Uri
		{
			get { return m_Uri; }
			private set
			{
				if (value != m_Uri)
				{
					m_Uri = value;
					OnPropertyChanged("Uri");
				}
			}
		}
		#endregion

		#region SubscriberCount
		int m_SubscriberCount;
		public int SubscriberCount
		{
			get { return m_SubscriberCount; }
			private set
			{
				if (value != m_SubscriberCount)
				{
					m_SubscriberCount = value;
					OnPropertyChanged("SubscriberCount");
				}
			}
		}
		#endregion

		#region MemberCount
		int m_MemberCount;
		public int MemberCount
		{
			get { return m_MemberCount; }
			private set
			{
				if (value != m_MemberCount)
				{
					m_MemberCount = value;
					OnPropertyChanged("MemberCount");
				}
			}
		}
		#endregion

		#region Mode
		ListMode m_Mode;
		public ListMode Mode
		{
			get { return m_Mode; }
			private set
			{
				if (value != m_Mode)
				{
					m_Mode = value;
					OnPropertyChanged("Mode");
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

		#region FullName
		string m_FullName;
		public string FullName
		{
			get { return m_FullName; }
			private set
			{
				if (value != m_FullName)
				{
					m_FullName = value;
					OnPropertyChanged("FullName");
				}
			}
		}
		#endregion

		#region Description
		string m_Description;
		public string Description
		{
			get { return m_Description; }
			private set
			{
				if (value != m_Description)
				{
					m_Description = value;
					OnPropertyChanged("Description");
				}
			}
		}
		#endregion

		#region User
		User m_User;
		public User User
		{
			get { return m_User; }
			private set
			{
				if (value != m_User)
				{
					m_User = value;
					OnPropertyChanged("User");
				}
			}
		}
		#endregion

		#region IsFollowing
		bool m_IsFollowing;
		public bool IsFollowing
		{
			get { return m_IsFollowing; }
			private set
			{
				if (value != m_IsFollowing)
				{
					m_IsFollowing = value;
					OnPropertyChanged("IsFollowing");
				}
			}
		}
		#endregion

		IUserIdentifier IListIdentifier.User { get { return User; } }

		public bool HasId { get { return true; } }

		public bool HasSlugAndUser { get { return true; } }

		public override string ToString() { return Name; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Update(List item)
		{		
			Slug = item.Slug;
			Name = item.Name;
			CreatedTime = item.CreatedTime;
			Uri = item.Uri;
			SubscriberCount = item.SubscriberCount;
			MemberCount = item.MemberCount;
			Mode = item.Mode;
			Id = item.Id;
			FullName = item.FullName;
			Description = item.Description;
			User = item.User;
			IsFollowing = item.IsFollowing;
		}
	}

	public enum ListMode
	{
		Default,
		Public,
		Private,
	}
}
