using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace Saruna
{
	public class Tweet : ITweetIdentifier, IMessage, INotifyPropertyChanged, IUpdateable<Tweet>
	{
		public static Tweet FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			WeakReference<Tweet> weakTweet;
			long id = element.Element("id").Cast<long>();
			Tweet res;
			if (cache.TryGetValue(id, out weakTweet) && !weakTweet.TryGetTarget(out res))
				weakTweet.SetTarget(res = new Tweet());
			else
				cache[id] = new WeakReference<Tweet>(res = new Tweet());
			res.Id = id;
			res.CreatedTime = DateTime.ParseExact(element.Element("created_at").Cast<string>(), "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture.DateTimeFormat);
			res.Source = HtmlNode.CreateNode(element.Element("source").Cast<string>());
			if (!Utils.IsJsonNull(element.Element("contributors")))
				res.Contributors = element.Element("contributors").Elements().Select(x => Contributor.FromXml(x)).ToArray();
			else
				res.Contributors = new Contributor[0];
			if (!Utils.IsJsonNull(element.Element("coordinates")))
				res.Coordinate = Point.FromXml(element.Element("coordinates").Element("coordinates"));
			else
				res.Coordinate = null;
			res.Entities = TweetEntities.FromXml(element.Element("entities"));
			res.FavoriteCount = element.Element("favorite_count").Cast<int>();
			res.HasCurrentUserFavorited = element.Element("favorited").Cast<bool>();
			var filterLevel = element.Element("filter_level").Cast<string>();
			if (filterLevel != null)
				res.FilterLevel = (StreamFilterLevel)Enum.Parse(typeof(StreamFilterLevel), filterLevel, true);
			else
				res.FilterLevel = StreamFilterLevel.None;
			res.InReplyToUserId = Identifiers.CreateUser(element.Element("in_reply_to_screen_name").Cast<string>(), element.Element("in_reply_to_user_id").Cast<long?>());
			res.InReplyToTweetId = Identifiers.CreateTweet(element.Element("in_reply_to_status_id").Cast<long?>());
			var lang = element.Element("lang").Cast<string>();
			if (!string.IsNullOrEmpty(lang))
			{
				try
				{
					res.Language = CultureInfo.GetCultureInfo(lang);
				}
				catch (CultureNotFoundException)
				{
					res.Language = null;
				}
			}
			else
				res.Language = null;
			res.Place = Place.FromXml(element.Element("place"));
			res.IsPossiblySensitive = element.Element("possibly_sensitive").Cast<bool?>();
			res.RetweetCount = element.Element("retweet_count").Cast<int>();
			res.HasCurrentUserRetweeted = element.Element("retweeted").Cast<bool>();
			res.RetweetSource = Tweet.FromXml(element.Element("retweeted_status"));
			res.Text = element.Element("text").Cast<string>();
			res.IsTextTruncated = element.Element("truncated").Cast<bool>();
			res.User = User.FromXml(element.Element("user"));
			res.HasWithheldDueToCopyright = element.Element("withheld_copyright").Cast<bool?>();
			if (!Utils.IsJsonNull(element.Element("withheld_in_countries")))
			{
				var countryCodes = element.Element("withheld_in_countries").Elements().Select(x => x.Cast<string>());
				if (countryCodes.Contains("xx", StringComparer.OrdinalIgnoreCase))
					res.HasWithheldInAllCountries = true;
				else
					res.WithholdingCountries = countryCodes.Where(x => char.ToLower(x[0]) != 'x').Select(x => new RegionInfo(x)).ToArray();
			}
			else
				res.WithholdingCountries = new RegionInfo[0];
			var withheldScope = element.Element("withheld_scope").Cast<string>();
			if (withheldScope != null)
			{
				if (withheldScope.Equals("status", StringComparison.OrdinalIgnoreCase))
					res.WithheldScope = ContentWithheldScope.Tweet;
				else
					res.WithheldScope = ContentWithheldScope.User;
			}
			else
				res.WithheldScope = ContentWithheldScope.None;
			return res;
		}

		static Dictionary<long, WeakReference<Tweet>> cache = new Dictionary<long, WeakReference<Tweet>>();

		public static Tweet GetTweetForId(long id)
		{
			WeakReference<Tweet> weakTweet;
			Tweet tweet;
			if (cache.TryGetValue(id, out weakTweet))
			{
				if (weakTweet.TryGetTarget(out tweet))
					return tweet;
				else
					cache.Remove(id);
			}
			return null;
		}

		#region Contributors
		IReadOnlyList<Contributor> m_Contributors;
		public IReadOnlyList<Contributor> Contributors
		{
			get { return m_Contributors; }
			private set
			{
				if (value != m_Contributors)
				{
					m_Contributors = value;
					OnPropertyChanged("Contributors");
				}
			}
		}
		#endregion

		#region Coordinate
		Point? m_Coordinate;
		public Point? Coordinate
		{
			get { return m_Coordinate; }
			set
			{
				if (value != m_Coordinate)
				{
					m_Coordinate = value;
					OnPropertyChanged("Coordinate");
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

		#region FavoriteCount
		int m_FavoriteCount;
		public int FavoriteCount
		{
			get { return m_FavoriteCount; }
			private set
			{
				if (value != m_FavoriteCount)
				{
					m_FavoriteCount = value;
					OnPropertyChanged("FavoriteCount");
				}
			}
		}
		#endregion

		#region HasCurrentUserFavorited
		bool m_HasCurrentUserFavorited;
		public bool HasCurrentUserFavorited
		{
			get { return m_HasCurrentUserFavorited; }
			set
			{
				if (value != m_HasCurrentUserFavorited)
				{
					m_HasCurrentUserFavorited = value;
					OnPropertyChanged("HasCurrentUserFavorited");
				}
			}
		}
		#endregion

		#region FilterLevel
		StreamFilterLevel m_FilterLevel;
		public StreamFilterLevel FilterLevel
		{
			get { return m_FilterLevel; }
			private set
			{
				if (value != m_FilterLevel)
				{
					m_FilterLevel = value;
					OnPropertyChanged("FilterLevel");
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

		#region InReplyToTweet
		Tweet m_InReplyToTweet;
		public Tweet InReplyToTweet
		{
			get
			{
				if (InReplyToTweetId != null && m_InReplyToTweet == null)
				{
					m_InReplyToTweet = GetTweetForId(InReplyToTweetId.Id);
					if (m_InReplyToTweet != null)
						OnPropertyChanged("InReplyToTweet");
				}
				return m_InReplyToTweet;
			}
		}
		#endregion

		#region InReplyToTweetId
		ITweetIdentifier m_InReplyToTweetId;
		public ITweetIdentifier InReplyToTweetId
		{
			get { return m_InReplyToTweetId; }
			set
			{
				if (value != m_InReplyToTweetId)
				{
					m_InReplyToTweetId = value;
					OnPropertyChanged("InReplyToTweetId");
					OnPropertyChanged("InReplyToTweet");
				}
			}
		}
		#endregion

		#region InReplyToUserId
		IUserIdentifier m_InReplyToUserId;
		public IUserIdentifier InReplyToUserId
		{
			get { return m_InReplyToUserId; }
			set
			{
				if (value != m_InReplyToUserId)
				{
					m_InReplyToUserId = value;
					OnPropertyChanged("InReplyToUserId");
				}
			}
		}
		#endregion

		#region Language
		CultureInfo m_Language;
		public CultureInfo Language
		{
			get { return m_Language; }
			private set
			{
				if (value != m_Language)
				{
					m_Language = value;
					OnPropertyChanged("Language");
				}
			}
		}
		#endregion

		#region Place
		Place m_Place;
		public Place Place
		{
			get { return m_Place; }
			set
			{
				if (value != m_Place)
				{
					m_Place = value;
					OnPropertyChanged("Place");
				}
			}
		}
		#endregion

		#region IsPossiblySensitive
		bool? m_IsPossiblySensitive;
		public bool? IsPossiblySensitive
		{
			get { return m_IsPossiblySensitive; }
			set
			{
				if (value != m_IsPossiblySensitive)
				{
					m_IsPossiblySensitive = value;
					OnPropertyChanged("IsPossiblySensitive");
				}
			}
		}
		#endregion

		#region RetweetCount
		int m_RetweetCount;
		public int RetweetCount
		{
			get { return m_RetweetCount; }
			private set
			{
				if (value != m_RetweetCount)
				{
					m_RetweetCount = value;
					OnPropertyChanged("RetweetCount");
				}
			}
		}
		#endregion

		#region HasCurrentUserRetweeted
		bool m_HasCurrentUserRetweeted;
		public bool HasCurrentUserRetweeted
		{
			get { return m_HasCurrentUserRetweeted; }
			set
			{
				if (value != m_HasCurrentUserRetweeted)
				{
					m_HasCurrentUserRetweeted = value;
					OnPropertyChanged("HasCurrentUserRetweeted");
				}
			}
		}
		#endregion

		#region RetweetSource
		Tweet m_RetweetSource;
		public Tweet RetweetSource
		{
			get { return m_RetweetSource; }
			private set
			{
				if (value != m_RetweetSource)
				{
					m_RetweetSource = value;
					OnPropertyChanged("RetweetSource");
				}
			}
		}
		#endregion

		#region Source
		HtmlNode m_Source;
		public HtmlNode Source
		{
			get { return m_Source; }
			private set
			{
				if (value != m_Source)
				{
					m_Source = value;
					OnPropertyChanged("Source");
				}
			}
		}
		#endregion

		#region Text
		string m_Text;
		public string Text
		{
			get { return m_Text; }
			set
			{
				if (value != m_Text)
				{
					m_Text = value;
					OnPropertyChanged("Text");
				}
			}
		}
		#endregion

		#region IsTextTruncated
		bool m_IsTextTruncated;
		public bool IsTextTruncated
		{
			get { return m_IsTextTruncated; }
			private set
			{
				if (value != m_IsTextTruncated)
				{
					m_IsTextTruncated = value;
					OnPropertyChanged("IsTextTruncated");
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

		#region HasWithheldDueToCopyright
		bool? m_HasWithheldDueToCopyright;
		public bool? HasWithheldDueToCopyright
		{
			get { return m_HasWithheldDueToCopyright; }
			private set
			{
				if (value != m_HasWithheldDueToCopyright)
				{
					m_HasWithheldDueToCopyright = value;
					OnPropertyChanged("HasWithheldDueToCopyright");
				}
			}
		}
		#endregion

		#region WithholdingCountries
		IReadOnlyList<RegionInfo> m_WithholdingCountries;
		public IReadOnlyList<RegionInfo> WithholdingCountries
		{
			get { return m_WithholdingCountries; }
			private set
			{
				if (value != m_WithholdingCountries)
				{
					m_WithholdingCountries = value;
					OnPropertyChanged("WithholdingCountries");
				}
			}
		}
		#endregion
		
		#region HasWithheldInAllCountries
		bool m_HasWithheldInAllCountries;
		public bool HasWithheldInAllCountries
		{
			get { return m_HasWithheldInAllCountries; }
			private set
			{
				if (value != m_HasWithheldInAllCountries)
				{
					m_HasWithheldInAllCountries = value;
					OnPropertyChanged("HasWithheldInAllCountries");
				}
			}
		}
		#endregion

		#region WithheldScope
		ContentWithheldScope m_WithheldScope;
		public ContentWithheldScope WithheldScope
		{
			get { return m_WithheldScope; }
			private set
			{
				if (value != m_WithheldScope)
				{
					m_WithheldScope = value;
					OnPropertyChanged("WithheldScope");
				}
			}
		}
		#endregion

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public async Task UpdateInReplyToTweetAsync(Twitter twitter)
		{
			try
			{
				if (InReplyToTweetId != null && m_InReplyToTweet == null)
				{
					m_InReplyToTweet = await twitter.GetTweetAsync(InReplyToTweetId);
					if (m_InReplyToTweet != null)
						OnPropertyChanged("InReplyToTweet");
				}
			}
			catch (TwitterException) { }
		}

		public void Update(Tweet item)
		{
			if (item == null)
				return;
			Contributors = item.Contributors;
			WithholdingCountries = item.WithholdingCountries;
			Coordinate = item.Coordinate;
			CreatedTime = item.CreatedTime;
			Entities = item.Entities;
			FavoriteCount = item.FavoriteCount;
			HasCurrentUserFavorited = item.HasCurrentUserFavorited;
			FilterLevel = item.FilterLevel;
			Id = item.Id;
			InReplyToTweetId = item.InReplyToTweetId;
			InReplyToUserId = item.InReplyToUserId;
			Language = item.Language;
			Place = item.Place;
			IsPossiblySensitive = item.IsPossiblySensitive;
			RetweetCount = item.RetweetCount;
			HasCurrentUserRetweeted = item.HasCurrentUserRetweeted;
			RetweetSource = item.RetweetSource;
			Source = item.Source;
			Text = item.Text;
			IsTextTruncated = item.IsTextTruncated;
			User = item.User;
			HasWithheldDueToCopyright = item.HasWithheldDueToCopyright;
			WithheldScope = item.WithheldScope;
		}

		public override string ToString() { return Text; }

		public event PropertyChangedEventHandler PropertyChanged;
	}

	public class Contributor
	{
		public static Contributor FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			Contributor res = new Contributor();
			res.Id = Identifiers.CreateUser(element.Element("screen_name").Cast<string>(), element.Element("id").Cast<long>());
			return res;
		}

		public IUserIdentifier Id { get; private set; }

		public override string ToString() { return Id.ScreenName; }
	}

	public enum StreamFilterLevel
	{
		None,
		Low,
		Medium,
	}

	public enum ContentWithheldScope
	{
		None,
		Tweet,
		User,
	}
}
