using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Saruna
{
	public class User : IUserIdentifier, INotifyPropertyChanged, IUpdateable<User>
	{
		#region IsContributorsEnabled
		bool m_IsContributorsEnabled;
		public bool IsContributorsEnabled
		{
			get { return m_IsContributorsEnabled; }
			private set
			{
				if (value != m_IsContributorsEnabled)
				{
					m_IsContributorsEnabled = value;
					OnPropertyChanged("IsContributorsEnabled");
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

		#region IsDefaultProfile
		bool m_IsDefaultProfile;
		public bool IsDefaultProfile
		{
			get { return m_IsDefaultProfile; }
			private set
			{
				if (value != m_IsDefaultProfile)
				{
					m_IsDefaultProfile = value;
					OnPropertyChanged("IsDefaultProfile");
				}
			}
		}
		#endregion

		#region UseDefaultProfileImage
		bool m_UseDefaultProfileImage;
		public bool UseDefaultProfileImage
		{
			get { return m_UseDefaultProfileImage; }
			private set
			{
				if (value != m_UseDefaultProfileImage)
				{
					m_UseDefaultProfileImage = value;
					OnPropertyChanged("UseDefaultProfileImage");
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

		#region Entity
		TweetEntities m_Entity;
		public TweetEntities Entity
		{
			get { return m_Entity; }
			private set
			{
				if (value != m_Entity)
				{
					m_Entity = value;
					OnPropertyChanged("Entity");
				}
			}
		}
		#endregion

		#region FavoritesCount
		int m_FavoritesCount;
		public int FavoritesCount
		{
			get { return m_FavoritesCount; }
			private set
			{
				if (value != m_FavoritesCount)
				{
					m_FavoritesCount = value;
					OnPropertyChanged("FavoritesCount");
				}
			}
		}
		#endregion

		#region HasSentFollowRequest
		bool? m_HasSentFollowRequest;
		public bool? HasSentFollowRequest
		{
			get { return m_HasSentFollowRequest; }
			private set
			{
				if (value != m_HasSentFollowRequest)
				{
					m_HasSentFollowRequest = value;
					OnPropertyChanged("HasSentFollowRequest");
				}
			}
		}
		#endregion

		#region FollowersCount
		int m_FollowersCount;
		public int FollowersCount
		{
			get { return m_FollowersCount; }
			private set
			{
				if (value != m_FollowersCount)
				{
					m_FollowersCount = value;
					OnPropertyChanged("FollowersCount");
				}
			}
		}
		#endregion

		#region FriendsCount
		int m_FriendsCount;
		public int FriendsCount
		{
			get { return m_FriendsCount; }
			private set
			{
				if (value != m_FriendsCount)
				{
					m_FriendsCount = value;
					OnPropertyChanged("FriendsCount");
				}
			}
		}
		#endregion

		#region HasGeotaggingEnabled
		bool m_HasGeotaggingEnabled;
		public bool HasGeotaggingEnabled
		{
			get { return m_HasGeotaggingEnabled; }
			private set
			{
				if (value != m_HasGeotaggingEnabled)
				{
					m_HasGeotaggingEnabled = value;
					OnPropertyChanged("HasGeotaggingEnabled");
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

		#region IsTranslator
		bool m_IsTranslator;
		public bool IsTranslator
		{
			get { return m_IsTranslator; }
			private set
			{
				if (value != m_IsTranslator)
				{
					m_IsTranslator = value;
					OnPropertyChanged("IsTranslator");
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

		#region ListedCount
		int m_ListedCount;
		public int ListedCount
		{
			get { return m_ListedCount; }
			private set
			{
				if (value != m_ListedCount)
				{
					m_ListedCount = value;
					OnPropertyChanged("ListedCount");
				}
			}
		}
		#endregion

		#region Location
		string m_Location;
		public string Location
		{
			get { return m_Location; }
			private set
			{
				if (value != m_Location)
				{
					m_Location = value;
					OnPropertyChanged("Location");
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

		#region BackgroundColor
		Color m_BackgroundColor;
		public Color BackgroundColor
		{
			get { return m_BackgroundColor; }
			private set
			{
				if (value != m_BackgroundColor)
				{
					m_BackgroundColor = value;
					OnPropertyChanged("BackgroundColor");
				}
			}
		}
		#endregion

		#region BackgroundImageUrl
		Uri m_BackgroundImageUrl;
		public Uri BackgroundImageUrl
		{
			get { return m_BackgroundImageUrl; }
			private set
			{
				if (value != m_BackgroundImageUrl)
				{
					m_BackgroundImageUrl = value;
					OnPropertyChanged("BackgroundImageUrl");
				}
			}
		}
		#endregion

		#region SecureBackgroundImageUrl
		Uri m_SecureBackgroundImageUrl;
		public Uri SecureBackgroundImageUrl
		{
			get { return m_SecureBackgroundImageUrl; }
			private set
			{
				if (value != m_SecureBackgroundImageUrl)
				{
					m_SecureBackgroundImageUrl = value;
					OnPropertyChanged("SecureBackgroundImageUrl");
				}
			}
		}
		#endregion

		#region IsBackgroundTile
		bool m_IsBackgroundTile;
		public bool IsBackgroundTile
		{
			get { return m_IsBackgroundTile; }
			private set
			{
				if (value != m_IsBackgroundTile)
				{
					m_IsBackgroundTile = value;
					OnPropertyChanged("IsBackgroundTile");
				}
			}
		}
		#endregion

		#region ProfileBannerUrl
		Uri m_ProfileBannerUrl;
		public Uri ProfileBannerUrl
		{
			get { return m_ProfileBannerUrl; }
			private set
			{
				if (value != m_ProfileBannerUrl)
				{
					m_ProfileBannerUrl = value;
					OnPropertyChanged("ProfileBannerUrl");
				}
			}
		}
		#endregion

		#region ProfileImageUrl
		Uri m_ProfileImageUrl;
		public Uri ProfileImageUrl
		{
			get { return m_ProfileImageUrl; }
			private set
			{
				if (value != m_ProfileImageUrl)
				{
					m_ProfileImageUrl = value;
					OnPropertyChanged("ProfileImageUrl");
				}
			}
		}
		#endregion

		#region SecureProfileImageUrl
		Uri m_SecureProfileImageUrl;
		public Uri SecureProfileImageUrl
		{
			get { return m_SecureProfileImageUrl; }
			private set
			{
				if (value != m_SecureProfileImageUrl)
				{
					m_SecureProfileImageUrl = value;
					OnPropertyChanged("SecureProfileImageUrl");
				}
			}
		}
		#endregion

		#region LinkColor
		Color m_LinkColor;
		public Color LinkColor
		{
			get { return m_LinkColor; }
			private set
			{
				if (value != m_LinkColor)
				{
					m_LinkColor = value;
					OnPropertyChanged("LinkColor");
				}
			}
		}
		#endregion

		#region SidebarBorderColor
		Color m_SidebarBorderColor;
		public Color SidebarBorderColor
		{
			get { return m_SidebarBorderColor; }
			private set
			{
				if (value != m_SidebarBorderColor)
				{
					m_SidebarBorderColor = value;
					OnPropertyChanged("SidebarBorderColor");
				}
			}
		}
		#endregion

		#region SidebarFillColor
		Color m_SidebarFillColor;
		public Color SidebarFillColor
		{
			get { return m_SidebarFillColor; }
			private set
			{
				if (value != m_SidebarFillColor)
				{
					m_SidebarFillColor = value;
					OnPropertyChanged("SidebarFillColor");
				}
			}
		}
		#endregion

		#region TextColor
		Color m_TextColor;
		public Color TextColor
		{
			get { return m_TextColor; }
			private set
			{
				if (value != m_TextColor)
				{
					m_TextColor = value;
					OnPropertyChanged("TextColor");
				}
			}
		}
		#endregion

		#region UseBackgroundImage
		bool m_UseBackgroundImage;
		public bool UseBackgroundImage
		{
			get { return m_UseBackgroundImage; }
			private set
			{
				if (value != m_UseBackgroundImage)
				{
					m_UseBackgroundImage = value;
					OnPropertyChanged("UseBackgroundImage");
				}
			}
		}
		#endregion

		#region IsProtected
		bool m_IsProtected;
		public bool IsProtected
		{
			get { return m_IsProtected; }
			private set
			{
				if (value != m_IsProtected)
				{
					m_IsProtected = value;
					OnPropertyChanged("IsProtected");
				}
			}
		}
		#endregion

		#region ScreenName
		string m_ScreenName;
		public string ScreenName
		{
			get { return m_ScreenName; }
			private set
			{
				if (value != m_ScreenName)
				{
					m_ScreenName = value;
					OnPropertyChanged("ScreenName");
				}
			}
		}
		#endregion

		#region ShowAllInlineMedia
		bool? m_ShowAllInlineMedia;
		public bool? ShowAllInlineMedia
		{
			get { return m_ShowAllInlineMedia; }
			private set
			{
				if (value != m_ShowAllInlineMedia)
				{
					m_ShowAllInlineMedia = value;
					OnPropertyChanged("ShowAllInlineMedia");
				}
			}
		}
		#endregion

		#region MostRecentTweet
		Tweet m_MostRecentTweet;
		public Tweet MostRecentTweet
		{
			get { return m_MostRecentTweet; }
			private set
			{
				if (value != m_MostRecentTweet)
				{
					m_MostRecentTweet = value;
					OnPropertyChanged("MostRecentTweet");
				}
			}
		}
		#endregion

		#region TweetsCount
		int m_TweetsCount;
		public int TweetsCount
		{
			get { return m_TweetsCount; }
			private set
			{
				if (value != m_TweetsCount)
				{
					m_TweetsCount = value;
					OnPropertyChanged("TweetsCount");
				}
			}
		}
		#endregion

		#region TimeZone
		TimeZoneInfo m_TimeZone;
		public TimeZoneInfo TimeZone
		{
			get { return m_TimeZone; }
			private set
			{
				if (value != m_TimeZone)
				{
					m_TimeZone = value;
					OnPropertyChanged("TimeZone");
				}
			}
		}
		#endregion

		#region Url
		Uri m_Url;
		public Uri Url
		{
			get { return m_Url; }
			private set
			{
				if (value != m_Url)
				{
					m_Url = value;
					OnPropertyChanged("Url");
				}
			}
		}
		#endregion

		#region IsVerified
		bool m_IsVerified;
		public bool IsVerified
		{
			get { return m_IsVerified; }
			private set
			{
				if (value != m_IsVerified)
				{
					m_IsVerified = value;
					OnPropertyChanged("IsVerified");
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

		public bool HasScreenName { get { return true; } }

		public bool HasId { get { return true; } }

		public void Update(User item)
		{
			IsContributorsEnabled = item.IsContributorsEnabled;
			CreatedTime = item.CreatedTime;
			IsDefaultProfile = item.IsDefaultProfile;
			UseDefaultProfileImage = item.UseDefaultProfileImage;
			Description = item.Description;
			Entity = item.Entity;
			FavoritesCount = item.FavoritesCount;
			HasSentFollowRequest = item.HasSentFollowRequest;
			FollowersCount = item.FollowersCount;
			FriendsCount = item.FriendsCount;
			HasGeotaggingEnabled = item.HasGeotaggingEnabled;
			Id = item.Id;
			IsTranslator = item.IsTranslator;
			Language = item.Language;
			ListedCount = item.ListedCount;
			Location = item.Location;
			Name = item.Name;
			BackgroundColor = item.BackgroundColor;
			BackgroundImageUrl = item.BackgroundImageUrl;
			SecureBackgroundImageUrl = item.SecureBackgroundImageUrl;
			IsBackgroundTile = item.IsBackgroundTile;
			ProfileBannerUrl = item.ProfileBannerUrl;
			ProfileImageUrl = item.ProfileImageUrl;
			SecureProfileImageUrl = item.SecureProfileImageUrl;
			LinkColor = item.LinkColor;
			SidebarBorderColor = item.SidebarBorderColor;
			SidebarFillColor = item.SidebarFillColor;
			TextColor = item.TextColor;
			UseBackgroundImage = item.UseBackgroundImage;
			IsProtected = item.IsProtected;
			ScreenName = item.ScreenName;
			ShowAllInlineMedia = item.ShowAllInlineMedia;
			MostRecentTweet = item.MostRecentTweet;
			TweetsCount = item.TweetsCount;
			TimeZone = item.TimeZone;
			Url = item.Url;
			IsVerified = item.IsVerified;
			WithholdingCountries = item.WithholdingCountries;
			WithheldScope = item.WithheldScope;
		}
		
		public override string ToString() { return Name + ", @" + ScreenName; }

		public static User FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new User();
			res.CreatedTime = DateTime.ParseExact(element.Element("created_at").Cast<string>(), "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture.DateTimeFormat);
			res.BackgroundColor = new Color(255, new Color(int.Parse(element.Element("profile_background_color").Cast<string>(), NumberStyles.AllowHexSpecifier)));
			res.LinkColor = new Color(255, new Color(int.Parse(element.Element("profile_link_color").Cast<string>(), NumberStyles.AllowHexSpecifier)));
			res.SidebarBorderColor = new Color(255, new Color(int.Parse(element.Element("profile_sidebar_border_color").Cast<string>(), NumberStyles.AllowHexSpecifier)));
			res.SidebarFillColor = new Color(255, new Color(int.Parse(element.Element("profile_sidebar_fill_color").Cast<string>(), NumberStyles.AllowHexSpecifier)));
			res.TextColor = new Color(255, new Color(int.Parse(element.Element("profile_text_color").Cast<string>(), NumberStyles.AllowHexSpecifier)));
			res.IsContributorsEnabled = element.Element("contributors_enabled").Cast<bool>();
			res.IsDefaultProfile = element.Element("default_profile").Cast<bool>();
			res.UseDefaultProfileImage = element.Element("default_profile_image").Cast<bool>();
			res.Description = element.Element("description").Cast<string>();
			res.Entity = TweetEntities.FromXml(element.Element("entities"));
			res.FavoritesCount = element.Element("favourites_count").Cast<int>();
			res.HasSentFollowRequest = element.Element("follow_request_sent").Cast<bool?>();
			res.FollowersCount = element.Element("followers_count").Cast<int>();
			res.FriendsCount = element.Element("friends_count").Cast<int>();
			res.HasGeotaggingEnabled = element.Element("geo_enabled").Cast<bool>();
			res.Id = element.Element("id").Cast<long>();
			res.IsTranslator = element.Element("is_translator").Cast<bool>();
			res.Language = CultureInfo.GetCultureInfo(element.Element("lang").Cast<string>());
			res.ListedCount = element.Element("listed_count").Cast<int>();
			res.Location = element.Element("location").Cast<string>();
			res.Name = element.Element("name").Cast<string>();
			var backgroundImageUrl = element.Element("profile_background_image_url").Cast<string>();
			if (!string.IsNullOrEmpty(backgroundImageUrl))
				res.BackgroundImageUrl = new Uri(backgroundImageUrl);
			var backgroundImageUrlHttps = element.Element("profile_background_image_url_https").Cast<string>();
			if (!string.IsNullOrEmpty(backgroundImageUrlHttps))
				res.SecureBackgroundImageUrl = new Uri(backgroundImageUrlHttps);
			res.IsBackgroundTile = element.Element("profile_background_tile").Cast<bool>();
			var profileBannerUrl = element.Element("profile_banner_url").Cast<string>();
			if (!string.IsNullOrEmpty(profileBannerUrl))
				res.ProfileBannerUrl = new Uri(profileBannerUrl);
			var profileImageUrl = element.Element("profile_image_url").Cast<string>();
			if (!string.IsNullOrEmpty(profileImageUrl))
				res.ProfileImageUrl = new Uri(profileImageUrl);
			var profileImageUrlHttps = element.Element("profile_image_url_https").Cast<string>();
			if (!string.IsNullOrEmpty(profileImageUrlHttps))
				res.SecureProfileImageUrl = new Uri(profileImageUrlHttps);
			res.UseBackgroundImage = element.Element("profile_use_background_image").Cast<bool>();
			res.IsProtected = element.Element("protected").Cast<bool>();
			res.ScreenName = element.Element("screen_name").Cast<string>();
			res.ShowAllInlineMedia = element.Element("show_all_inline_media").Cast<bool?>();
			res.MostRecentTweet = Tweet.FromXml(element.Element("status"));
			res.TweetsCount = element.Element("statuses_count").Cast<int>();
			var url = element.Element("url").Cast<string>();
			if (!string.IsNullOrEmpty(url))
				res.Url = new Uri(url);
			res.TimeZone = Utils.GetTimeZoneInfo(element.Element("time_zone").Cast<string>());
			res.IsVerified = element.Element("verified").Cast<bool>();
			var withheldCountries = element.Element("withheld_in_countries").Cast<string>();
			if (!string.IsNullOrEmpty(withheldCountries))
				res.WithholdingCountries = withheldCountries.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => new RegionInfo(x)).ToArray();
			else
				res.WithholdingCountries = new RegionInfo[0];
			var withheldScope = element.Element("withheld_scope").Cast<string>();
			if (!string.IsNullOrEmpty(withheldScope))
			{
				if (withheldScope.Equals("status", StringComparison.OrdinalIgnoreCase))
					res.WithheldScope = ContentWithheldScope.Tweet;
				else
					res.WithheldScope = ContentWithheldScope.User;
			}
			return res;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class UserSettings
	{
		public bool AlwaysUseHttps { get; private set; }

		public bool DiscoverableByEmail { get; private set; }

		public bool HasGeotaggingEnabled { get; private set; }

		public CultureInfo Language { get; private set; }

		public bool IsProtected { get; private set; }

		public string ScreenName { get; private set; }

		public bool ShowAllInlineMedia { get; private set; }

		public TimeInterval SleepTime { get; private set; }

		public TimeZoneInfo TimeZone { get; private set; }

		public IReadOnlyList<TrendLocation> TrendLocation { get; private set; }

		public bool UseCookiePersonalization { get; private set; }

		public static UserSettings FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new UserSettings();
			res.AlwaysUseHttps = element.Element("always_use_https").Cast<bool>();
			res.DiscoverableByEmail = element.Element("discoverable_by_email").Cast<bool>();
			res.HasGeotaggingEnabled = element.Element("geo_enabled").Cast<bool>();
			res.Language = CultureInfo.GetCultureInfo(element.Element("language").Cast<string>());
			res.IsProtected = element.Element("protected").Cast<bool>();
			res.ScreenName = element.Element("screen_name").Cast<string>();
			res.ShowAllInlineMedia = element.Element("show_all_inline_media").Cast<bool>();
			res.SleepTime = TimeInterval.FromXml(element.Element("sleep_time"));
			res.TimeZone = Utils.GetTimeZoneInfo(element.Element("time_zone").Element("name").Cast<string>());
			res.TrendLocation = element.Element("trend_location").Elements().Select(x => Saruna.TrendLocation.FromXml(x)).ToArray();
			res.UseCookiePersonalization = element.Element("use_cookie_personalization").Cast<bool>();
			return res;
		}
	}

	public class TimeInterval
	{
		public static readonly TimeInterval Invalid = new TimeInterval(-1, -1);

		public TimeInterval(int start, int end)
		{
			Start = start;
			End = end;
		}

		public int Start { get; private set; }

		public int End { get; private set; }

		public bool IsInvalid { get { return Start >= 0 && Start < 24 && End >= 0 && End < 24; } }

		public static TimeInterval FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element) || !element.Element("enabled").Cast<bool>())
				return null;
			return new TimeInterval(
				element.Element("start_time").Cast<int>(),
				element.Element("end_time").Cast<int>()
			);
		}
	}
}
