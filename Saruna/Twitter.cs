using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Saruna.Infrastructures;
using Saruna.Streams;

namespace Saruna
{
	public class Twitter
	{
		TwitterRequest request = new TwitterRequest();

		public TokenSecretPair AccessToken
		{
			get { return request.AccessToken; }
			set { request.AccessToken = value; }
		}

		public TokenSecretPair ConsumerKey
		{
			get { return request.ConsumerKey; }
			set { request.ConsumerKey = value; }
		}

		static void SetUser(TwitterRequestContent content, params IUserIdentifier[] users) { SetUser(content, "screen_name", "user_id", users); }

		static void SetUser(TwitterRequestContent content, string screenNameKey, string idKey, params IUserIdentifier[] users)
		{
			StringBuilder screenNames = new StringBuilder();
			StringBuilder userIds = new StringBuilder();
			foreach (var user in users)
			{
				if (user != null)
				{
					if (user.HasId)
						userIds.Append(user.Id.ToString() + ",");
					else
						screenNames.Append(user.ScreenName + ",");
				}
			}
			if (screenNames.Length > 0 && screenNames[screenNames.Length - 1] == ',')
				screenNames.Remove(screenNames.Length - 1, 1);
			if (userIds.Length > 0 && userIds[userIds.Length - 1] == ',')
				userIds.Remove(userIds.Length - 1, 1);
			if (screenNames.Length > 0)
				content.SetParameter(screenNameKey, screenNames.ToString());
			if (userIds.Length > 0)
				content.SetParameter(idKey, userIds.ToString());
		}

		static void SetList(TwitterRequestContent content, IListIdentifier list)
		{
			if (list != null)
			{
				if (list.HasId)
					content.SetParameter("list_id", list.Id.ToString());
				else
				{
					content.SetParameter("slug", list.Slug);
					SetUser(content, "owner_screen_name", "owner_id", list.User);
				}
			}
		}

		#region Timelines

		Timeline<Tweet> _homeTimeline = null;
		Timeline<Tweet> _mentionsTimeline = null;

		public Timeline<Tweet> HomeTimeline
		{
			get
			{
				if (_homeTimeline == null)
				{
					var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/home_timeline.json");
					content.SetParameter("contributor_details", "true");
					_homeTimeline = new Timeline<Tweet>(request, content);
				}
				return _homeTimeline;
			}
		}

		public Timeline<Tweet> MentionsTimeline
		{
			get
			{
				if (_mentionsTimeline == null)
				{
					var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/mentions_timeline.json");
					content.SetParameter("contributor_details", "true");
					_mentionsTimeline = new Timeline<Tweet>(request, content);
				}
				return _mentionsTimeline;
			}
		}

		public Timeline<Tweet> GetUserTimeline(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/user_timeline.json");
			content.SetParameter("contributor_details", "true");
			SetUser(content, user);
			return new Timeline<Tweet>(request, content);
		}

		#endregion

		#region Tweets

		public async Task<Tweet> TweetAsync(Tweet tweet)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/update.json");
			content.SetParameter("status", tweet.Text);
			if (tweet.InReplyToTweetId != null)
				content.SetParameter("in_reply_to_status_id", tweet.InReplyToTweetId.Id.ToString());
			if (tweet.Coordinate != null)
			{
				content.SetParameter("lat", tweet.Coordinate.Value.Latitude.ToString());
				content.SetParameter("long", tweet.Coordinate.Value.Longitude.ToString());
				content.SetParameter("display_coordinates", "true");
			}
			if (tweet.Place != null)
				content.SetParameter("place_id", tweet.Place.Id);
			return Tweet.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<Tweet> TweetAsync(Tweet tweet, IEnumerable<byte[]> medias)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/update_with_media.json");
			content.IsMultipart = true;
			content.SetParameter("status", tweet.Text);
			foreach (var media in medias)
				content.AddParameter("media[]", media);
			if (tweet.IsPossiblySensitive != null)
				content.SetParameter("possibly_sensitive", tweet.IsPossiblySensitive.ToString());
			if (tweet.InReplyToTweetId != null)
				content.SetParameter("in_reply_to_status_id", tweet.InReplyToTweetId.Id.ToString());
			if (tweet.Coordinate != null)
			{
				content.SetParameter("lat", tweet.Coordinate.Value.Latitude.ToString());
				content.SetParameter("long", tweet.Coordinate.Value.Longitude.ToString());
				content.SetParameter("display_coordinates", "true");
			}
			if (tweet.Place != null)
				content.SetParameter("place_id", tweet.Place.Id);
			return Tweet.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<Tweet> RetweetAsync(ITweetIdentifier tweet)
		{
			return Tweet.FromXml(await request.GetXmlAsync(new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/retweet/" + tweet.Id.ToString() + ".json")));
		}
		
		public async Task<Tweet> UntweetAsync(ITweetIdentifier tweet)
		{
			return Tweet.FromXml(await request.GetXmlAsync(new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/destroy/" + tweet.Id.ToString() + ".json")));
		}

		public async Task<IReadOnlyList<Tweet>> GetTweetsAsync(params ITweetIdentifier[] tweets)
		{
			var content = new TwitterRequestContent(tweets.Length > 10 ? HttpMethod.Post : HttpMethod.Get, "https://api.twitter.com/1.1/statuses/lookup.json");
			content.SetParameter("id", string.Join(",", tweets.Select(x => x.Id)));
			return (await request.GetXmlAsync(content)).Elements().Select(x => Tweet.FromXml(x)).ToArray();
		}

		public async Task<Tweet> GetTweetAsync(ITweetIdentifier tweet)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/show.json");
			content.SetParameter("id", tweet.Id.ToString());
			return Tweet.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<IReadOnlyList<Tweet>> GetRetweetsAsync(ITweetIdentifier tweet, int count)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/retweets/" + tweet.Id.ToString() + ".json ");
			content.SetParameter("count", count.ToString());
			return (await request.GetXmlAsync(content)).Elements().Select(x => Tweet.FromXml(x)).ToArray();
		}

		#endregion

		#region Search

		public SearchTimeline SearchTweets(string query, DateTime until, GeometryCircle circle, CultureInfo language, CultureInfo locale, SearchResultType resultType)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/search/tweets.json");
			content.SetParameter("q", query);
			if (circle != null)
				content.SetParameter("geocode", circle.ToString());
			if (language != null)
				content.SetParameter("lang", language.TwoLetterISOLanguageName);
			if (locale != null)
				content.SetParameter("locale", locale.TwoLetterISOLanguageName);
			if (resultType != SearchResultType.Default)
				content.SetParameter("result_type", resultType.ToString().ToLower());
			if (until < DateTime.MaxValue)
				content.SetParameter("until", ((DateTime)until).ToString("yyyy-MM-dd"));
			return new SearchTimeline(request, content);
		}

		#endregion

		#region Streaming

		TwitterStream _userStream = null;

		public TwitterStream UserStream
		{
			get
			{
				if (_userStream == null)
				{
					var content = new TwitterRequestContent(HttpMethod.Post, "https://userstream.twitter.com/2/user.json");
					content.CompletionOption = HttpCompletionOption.ResponseHeadersRead;
					content.UseCompression = false;
					_userStream = new TwitterStream(request, content);
				}
				return _userStream;
			}
		}

		#endregion

		#region Direct Messages

		Timeline<DirectMessage> _receivedDirectMessageTimeline = null;
		Timeline<DirectMessage> _sentDirectMessageTimeline = null;

		public Timeline<DirectMessage> ReceivedDirectMessageTimeline
		{
			get
			{
				if (_receivedDirectMessageTimeline == null)
				{
					var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/direct_messages.json");
					_receivedDirectMessageTimeline = new Timeline<DirectMessage>(request, content);
				}
				return _receivedDirectMessageTimeline;
			}
		}

		public Timeline<DirectMessage> SentDirectMessageTimeline
		{
			get
			{
				if (_sentDirectMessageTimeline == null)
				{
					var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/direct_messages/sent.json");
					_sentDirectMessageTimeline = new Timeline<DirectMessage>(request, content);
				}
				return _sentDirectMessageTimeline;
			}
		}

		public async Task<DirectMessage> GetDirectMessageAsync(IDirectMessageIdentifier message)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/direct_messages/show.json");
			content.SetParameter("id", message.Id.ToString());
			return DirectMessage.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<DirectMessage> SendDirectMessageAsync(string text, IUserIdentifier recipient)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/direct_messages/new.json");
			SetUser(content, recipient);
			content.SetParameter("text", text);
			return DirectMessage.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<DirectMessage> DeleteDirectMessageAsync(IDirectMessageIdentifier message)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/direct_messages/destroy.json");
			content.SetParameter("id", message.Id.ToString());
			return DirectMessage.FromXml(await request.GetXmlAsync(content));
		}

		#endregion

		#region Friends & Followers

		public async Task<IReadOnlyList<IUserIdentifier>> GetNoRetweetUserIdsAsync()
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/no_retweets/ids.json");
			return (await request.GetXmlAsync(content)).Elements().Select(x => Identifiers.CreateUser(x.Cast<long>())).ToArray();
		}

		public async Task<CursorNavigable<IUserIdentifier>> GetFriendIdsAsync(IUserIdentifier user, Cursor cursor, int usersPerPage)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friends/ids.json");
			SetUser(content, user);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("count", usersPerPage.ToString());
			return CursorNavigable<IUserIdentifier>.FromXml(await request.GetXmlAsync(content), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public async Task<CursorNavigable<IUserIdentifier>> GetFollowerIdsAsync(IUserIdentifier user, Cursor cursor, int usersPerPage)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/followers/ids.json");
			SetUser(content, user);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("count", usersPerPage.ToString());
			return CursorNavigable<IUserIdentifier>.FromXml(await request.GetXmlAsync(content), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public async Task<CursorNavigable<User>> GetFriendsAsync(IUserIdentifier user, Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friends/list.json");
			SetUser(content, user);
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await request.GetXmlAsync(content), "users", x => User.FromXml(x));
		}

		public async Task<CursorNavigable<User>> GetFollowersAsync(IUserIdentifier user, Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/followers/list.json");
			SetUser(content, user);
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await request.GetXmlAsync(content), "users", x => User.FromXml(x));
		}

		public async Task<IReadOnlyList<ConnectionEntry>> LookupFriendshipsAsync(params IUserIdentifier[] users)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/lookup.json");
			SetUser(content, users);
			return (await request.GetXmlAsync(content)).Elements().Select(x => ConnectionEntry.FromXml(x)).ToArray();
		}

		public async Task<CursorNavigable<IUserIdentifier>> GetIncomingFriendshipsAsync(Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/incoming.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<IUserIdentifier>.FromXml(await request.GetXmlAsync(content), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public async Task<CursorNavigable<IUserIdentifier>> GetOutgoingFriendshipsAsync(Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/outgoing.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<IUserIdentifier>.FromXml(await request.GetXmlAsync(content), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public async Task<User> FollowAsync(IUserIdentifier user, bool? enableNotifications)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/friendships/create.json");
			SetUser(content, user);
			if (enableNotifications != null)
				content.SetParameter("follow", enableNotifications.ToString().ToLower());
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<User> UnfollowAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/friendships/destroy.json");
			SetUser(content, user);
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<Relationship> ChangeNotificationsAsync(IUserIdentifier user, bool? enableNotifications = null, bool? showRetweets = null)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/friendships/update.json");
			SetUser(content, user);
			if (enableNotifications != null)
				content.SetParameter("device", enableNotifications.ToString().ToLower());
			if (showRetweets != null)
				content.SetParameter("retweets", showRetweets.ToString().ToLower());
			return new Relationship(await request.GetXmlAsync(content));
		}

		public async Task<Relationship> GetRelationshipAsync(IUserIdentifier source, IUserIdentifier target)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/show.json");
			SetUser(content, "source_id", "source_screen_name", source);
			SetUser(content, "target_id", "target_screen_name", target);
			return new Relationship(await request.GetXmlAsync(content));
		}

		#endregion

		#region Users
	
		public async Task<User> VerifyCredentialsAsync()
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/account/verify_credentials.json");
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<UserSettings> GetAccountSettingsAsync()
		{
			return UserSettings.FromXml(await request.GetXmlAsync(new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/account/settings.json")));
		}

		public async Task<UserSettings> ChangeAccountSettingsAsync(long? whereOnEarthId = null, TimeInterval sleepTime = null, TimeZoneInfo timeZone = null, CultureInfo language = null)
		{
			TwitterRequestContent content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/settings.json");
			if (whereOnEarthId != null)
				content.SetParameter("trend_location_woeid", whereOnEarthId.ToString());
			if (sleepTime != null)
			{
				if (sleepTime.IsInvalid)
					content.SetParameter("sllep_time_enabled", "false");
				else
				{
					content.SetParameter("sleep_time_enabled", "true");
					content.SetParameter("start_sleep_time", sleepTime.Start.ToString("00"));
					content.SetParameter("end_sleep_time", sleepTime.End.ToString("00"));
				}
			}
			if (timeZone != null)
				content.SetParameter("time_zone", timeZone.GetRubyOldTimeZoneKey());
			if (language != null)
				content.SetParameter("lang", language.TwoLetterISOLanguageName);
			return UserSettings.FromXml(await request.GetXmlAsync(content));
		}

		public async Task ChangeDeliveryDeviceAsync(DeliveryDevice device)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_delivery_device.json");
			content.SetParameter("device", device.ToString().ToLower());
			await request.GetXmlAsync(content);
		}

		public async Task<User> ChangeProfileAsync(string name = null, string url = null, string location = null, string description = null)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile.json");
			if (name != null)
				content.SetParameter("name", name);
			if (url != null)
				content.SetParameter("url", url);
			if (location != null)
				content.SetParameter("location", location);
			if (description != null)
				content.SetParameter("description", description);
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<User> ChangeProfileBackgroundImageAsync(byte[] image = null, bool? tile = null)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_background_image.json");
			if (image != null)
				content.SetParameter("image", Convert.ToBase64String(image));
			if (tile != null)
				content.SetParameter("tile", tile.ToString().ToLower());
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<User> RemoveProfileBackgroundImageAsync()
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_background_image.json");
			content.SetParameter("use", "false");
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<User> ChangeProfileColorsAsync(Color background, Color link, Color sidebarBorder, Color sidebarFill, Color text)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_colors.json");
			if (!background.IsEmpty)
				content.SetParameter("profile_background_color", (background.ToArgb() & 0xFFFFFF).ToString("X6"));
			if (!link.IsEmpty)
				content.SetParameter("profile_link_color", (link.ToArgb() & 0xFFFFFF).ToString("X6"));
			if (!sidebarBorder.IsEmpty)
				content.SetParameter("profile_sidebar_border_color", (sidebarBorder.ToArgb() & 0xFFFFFF).ToString("X6"));
			if (!sidebarFill.IsEmpty)
				content.SetParameter("profile_sidebar_fill_color", (sidebarFill.ToArgb() & 0xFFFFFF).ToString("X6"));
			if (!text.IsEmpty)
				content.SetParameter("profile_text_color", (text.ToArgb() & 0xFFFFFF).ToString("X6"));
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<User> ChangeProfileImageAsync(byte[] image)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_image.json");
			content.SetParameter("image", Convert.ToBase64String(image));
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<AvailableProfileBanners> GetProfileBannerAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/users/profile_banner.json");
			SetUser(content, user);
			return AvailableProfileBanners.FromXml(await request.GetXmlAsync(content));
		}

		public async Task ChangeProfileBannerAsync(byte[] image, Rectangle region)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_banner.json");
			content.SetParameter("banner", Convert.ToBase64String(image));
			if (!region.IsEmpty)
			{
				content.SetParameter("width", region.Width.ToString());
				content.SetParameter("height", region.Height.ToString());
				content.SetParameter("offset_left", region.X.ToString());
				content.SetParameter("offset_top", region.Y.ToString());
			}
			using (await request.GetResponseAsync(content)) { }
		}

		public async Task RemoveProfileBannerAsync()
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/remove_profile_banner.json");
			using (await request.GetResponseAsync(content)) { }
		}

		#region Block

		public async Task<CursorNavigable<User>> GetBlockedUsersAsync(Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/blocks/list.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await request.GetXmlAsync(content), "users", x => User.FromXml(x));
		}

		public async Task<CursorNavigable<IUserIdentifier>> GetBlockedUserIdsAsync(Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/blocks/ids.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<IUserIdentifier>.FromXml(await request.GetXmlAsync(content), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public async Task<User> BlockAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/blocks/create.json");
			SetUser(content, user);
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<User> UnblockAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/blocks/destroy.json");
			SetUser(content, user);
			return User.FromXml(await request.GetXmlAsync(content));
		}

		#endregion

		#region Mute

		public async Task<CursorNavigable<User>> GetMutedUsersAsync(Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/mutes/users/list.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await request.GetXmlAsync(content), "users", x => User.FromXml(x));
		}

		public async Task<CursorNavigable<IUserIdentifier>> GetMutedUserIdsAsync(Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/mutes/users/ids.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<IUserIdentifier>.FromXml(await request.GetXmlAsync(content), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public async Task<User> MuteAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/mutes/users/create.json");
			SetUser(content, user);
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<User> UnmuteAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/mutes/users/destroy.json");
			SetUser(content, user);
			return User.FromXml(await request.GetXmlAsync(content));
		}

		#endregion

		public async Task<IReadOnlyList<User>> GetUsersAsync(params IUserIdentifier[] users)
		{
			var content = new TwitterRequestContent(users.Length > 10 ? HttpMethod.Post : HttpMethod.Get, "https://api.twitter.com/1.1/users/lookup.json");
			SetUser(content, users);
			return (await request.GetXmlAsync(content)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		public async Task<User> GetUserAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/show.json");
			SetUser(content, user);
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<IReadOnlyList<User>> SearchUsersAsync(string query, int usersPerPage, int? page)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/search.json");
			content.SetParameter("q", query);
			if (page != null)
				content.SetParameter("page", page.ToString());
			content.SetParameter("count", usersPerPage.ToString());
			return (await request.GetXmlAsync(content)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		public async Task<IReadOnlyList<User>> GetContributeesAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/contributees.json");
			SetUser(content, user);
			return (await request.GetXmlAsync(content)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		public async Task<IReadOnlyList<User>> GetContributorsAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/contributors.json");
			SetUser(content, user);
			return (await request.GetXmlAsync(content)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		#endregion

		#region Suggested Users

		public async Task<IReadOnlyList<SuggestedUserCategory>> GetSuggestedUserCategoriesAsync(CultureInfo language)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/suggestions.json");
			if (language != null)
				content.SetParameter("lang", language.TwoLetterISOLanguageName);
			return (await request.GetXmlAsync(content)).Elements().Select(x => SuggestedUserCategory.FromXml(x)).ToArray();
		}

		public async Task<SuggestedUserList> GetSuggestedUserListAsync(string slug, CultureInfo language)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/suggestions/" + slug + ".json");
			if (language != null)
				content.SetParameter("lang", language.TwoLetterISOLanguageName);
			return SuggestedUserList.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<IReadOnlyList<User>> GetSuggestedUsersAsync(string slug)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/suggestions/" + slug + "/members.json");
			return (await request.GetXmlAsync(content)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		#endregion

		#region Favorites

		public async Task<Tweet> FavoriteAsync(ITweetIdentifier tweet)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/favorites/create.json");
			content.SetParameter("id", tweet.Id.ToString());
			return Tweet.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<Tweet> UnfavoriteAsync(ITweetIdentifier tweet)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/favorites/destroy.json");
			content.SetParameter("id", tweet.Id.ToString());
			return Tweet.FromXml(await request.GetXmlAsync(content));
		}

		public Timeline<Tweet> GetFavoritesTimeline(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/favorites/list.json");
			SetUser(content, user);
			return new Timeline<Tweet>(request, content);
		}

		#endregion

		#region Lists

		public async Task<IReadOnlyList<List>> GetListsAsync(IUserIdentifier user, bool ownedListFirst)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/list.json");
			SetUser(content, user);
			content.SetParameter("reverse", ownedListFirst.ToString().ToLower());
			return (await request.GetXmlAsync(content)).Elements().Select(x => List.FromXml(x)).ToArray();
		}

		public Timeline<Tweet> GetListTimeline(IListIdentifier list)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/statuses.json");
			SetList(content, list);
			content.SetParameter("include_rts", "true");
			return new Timeline<Tweet>(request, content);
		}

		public async Task RemoveMemberFromListAsync(IListIdentifier list, IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/members/destroy.json");
			SetList(content, list);
			SetUser(content, user);
			using (await request.GetResponseAsync(content)) { }
		}

		public async Task<CursorNavigable<List>> FindListsByMemberAsync(IUserIdentifier user, Cursor cursor, bool? ownedListsOnly)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/memberships.json");
			SetUser(content, user);
			content.SetParameter("cursor", cursor.Value.ToString());
			if (ownedListsOnly != null)
				content.SetParameter("filter_to_owned_lists", ownedListsOnly.ToString().ToLower());
			return CursorNavigable<List>.FromXml(await request.GetXmlAsync(content), "lists", x => List.FromXml(x));
		}

		public async Task<CursorNavigable<User>> GetSubscribersAsync(IListIdentifier list, Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/subscribers.json");
			SetList(content, list);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("include_entities", "true");
			return CursorNavigable<User>.FromXml(await request.GetXmlAsync(content), "users", x => User.FromXml(x));
		}

		public async Task<List> SubscribeAsync(IListIdentifier list)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/subscribers/create.json");
			SetList(content, list);
			return List.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<User> GetSubscriberAsync(IListIdentifier list, IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/subscribers/show.json");
			SetList(content, list);
			SetUser(content, user);
			content.SetParameter("include_entities", "true");
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task UnsubscribeAsync(IListIdentifier list)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/subscribers/destroy.json");
			SetList(content, list);
			using (await request.GetResponseAsync(content)) { }
		}

		public async Task AddMembersToListAsync(IListIdentifier list, params IUserIdentifier[] users)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/members/create_all.json");
			SetList(content, list);
			SetUser(content, users);
			using (await request.GetResponseAsync(content)) { }
		}

		public async Task<User> GetMemberAsync(IListIdentifier list, IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/members/show.json");
			SetList(content, list);
			SetUser(content, user);
			content.SetParameter("include_entities", "true");
			return User.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<CursorNavigable<User>> GetMembersAsync(IListIdentifier list, Cursor cursor)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/members.json");
			SetList(content, list);
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await request.GetXmlAsync(content), "users", x => User.FromXml(x));
		}

		public async Task AddMemberToListAsync(IListIdentifier list, IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/members/create.json");
			SetList(content, list);
			SetUser(content, user);
			using (await request.GetResponseAsync(content)) { }
		}

		public async Task<List> DeleteListAsync(IListIdentifier list)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/destroy.json");
			SetList(content, list);
			return List.FromXml(await request.GetXmlAsync(content));
		}

		public async Task ChangeListAsync(IListIdentifier list, string name = null, ListMode mode = ListMode.Default, string description = null)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/update.json");
			SetList(content, list);
			if (name != null)
				content.SetParameter("name", name);
			if (mode != ListMode.Default)
				content.SetParameter("mode", mode.ToString().ToLower());
			if (description != null)
				content.SetParameter("description", description);
			using (await request.GetResponseAsync(content)) { }
		}

		public async Task<List> CreateListAsync(string name, ListMode mode, string description)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/create.json");
			content.SetParameter("name", name);
			if (mode != ListMode.Default)
				content.SetParameter("mode", mode.ToString().ToLower());
			if (!string.IsNullOrEmpty(description))
				content.SetParameter("description", description);
			return List.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<List> GetListAsync(IListIdentifier list)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/show.json");
			SetList(content, list);
			return List.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<CursorNavigable<List>> GetSubscribedListsAsync(IUserIdentifier user, Cursor cursor, int listsPerPage)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/subscriptions.json");
			SetUser(content, user);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("count", listsPerPage.ToString());
			return CursorNavigable<List>.FromXml(await request.GetXmlAsync(content), "lists", x => List.FromXml(x));
		}

		public async Task RemoveMembersFromListAsync(IListIdentifier list, params IUserIdentifier[] users)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/members/destroy_all.json");
			SetList(content, list);
			SetUser(content, users);
			using (await request.GetResponseAsync(content)) { }
		}

		public async Task<CursorNavigable<List>> GetOwnedListsAsync(IUserIdentifier user, Cursor cursor, int listsPerPage)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/ownerships.json");
			SetUser(content, user);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("count", listsPerPage.ToString());
			return CursorNavigable<List>.FromXml(await request.GetXmlAsync(content), "lists", x => List.FromXml(x));
		}

		#endregion

		#region Saved Sarches

		public async Task<IReadOnlyList<SavedSearch>> GetSavedSearchesAsync()
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/saved_searches/list.json");
			return (await request.GetXmlAsync(content)).Elements().Select(x => SavedSearch.FromXml(x)).ToArray();
		}

		public async Task<SavedSearch> GetSavedSearchAsync(ISavedSearchIdentifier search)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/saved_searches/show/" + search.Id.ToString() + ".json");
			return SavedSearch.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<SavedSearch> SaveSearchAsync(string query)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/saved_searches/create.json");
			content.SetParameter("query", query);
			return SavedSearch.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<SavedSearch> DeleteSearchAsync(ISavedSearchIdentifier search)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/saved_searches/destroy/" + search.Id.ToString() + ".json");
			return SavedSearch.FromXml(await request.GetXmlAsync(content));
		}

		#endregion

		#region Places & Geo

		public async Task<Place> GetPlaceAsync(IPlaceIdentifier place)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/geo/id/" + place.Id + ".json");
			return Place.FromXml(await request.GetXmlAsync(content));
		}

		public async Task<IReadOnlyList<Place>> SearchPlaceFromCoordinateAsync(Point coordinate, Accuracy? accuracy, Granularity granularity, int? maxResults)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/geo/reverse_geocode.json");
			content.SetParameter("lat", coordinate.Latitude.ToString());
			content.SetParameter("long", coordinate.Longitude.ToString());
			if (accuracy != null)
				content.SetParameter("accuracy", accuracy.ToString());
			if (granularity == Granularity.Administration)
				content.SetParameter("granularity", "admin");
			else if (granularity == Granularity.City)
				content.SetParameter("granularity", "city");
			else if (granularity == Granularity.Country)
				content.SetParameter("granularity", "country");
			else if (granularity == Granularity.Neighborhood)
				content.SetParameter("granularity", "neighborhood");
			else if (granularity == Granularity.Point)
				content.SetParameter("granularity", "poi");
			if (maxResults != null)
				content.SetParameter("max_results", maxResults.ToString());
			return (await request.GetXmlAsync(content)).Element("result").Element("places").Elements().Select(x => Place.FromXml(x)).ToArray();
		}

		public async Task<IReadOnlyList<Place>> SearchPlaceAsync(Point? coordinate, string query, string ip, Accuracy? accuracy, Granularity granularity, int? maxResults, IPlaceIdentifier containedWithin, string streetAddress)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/geo/search.json");
			if (coordinate != null)
			{
				content.SetParameter("lat", ((Point)coordinate).Latitude.ToString());
				content.SetParameter("long", ((Point)coordinate).Longitude.ToString());
			}
			if (!string.IsNullOrEmpty(query))
				content.SetParameter("query", query);
			if (!string.IsNullOrEmpty(ip))
				content.SetParameter("ip", ip);
			if (accuracy != null)
				content.SetParameter("accuracy", accuracy.ToString());
			if (granularity == Granularity.Administration)
				content.SetParameter("granularity", "admin");
			else if (granularity == Granularity.City)
				content.SetParameter("granularity", "city");
			else if (granularity == Granularity.Country)
				content.SetParameter("granularity", "country");
			else if (granularity == Granularity.Neighborhood)
				content.SetParameter("granularity", "neighborhood");
			else if (granularity == Granularity.Point)
				content.SetParameter("granularity", "poi");
			if (maxResults != null)
				content.SetParameter("max_results", maxResults.ToString());
			if (containedWithin != null)
				content.SetParameter("contained_within", containedWithin.Id);
			if (!string.IsNullOrEmpty(streetAddress))
				content.SetParameter("attribute:street_address", streetAddress);
			return (await request.GetXmlAsync(content)).Element("result").Element("places").Elements().Select(x => Place.FromXml(x)).ToArray();
		}

		public async Task<PlaceSearchResult> GetSimilarPlacesAsync(Point coordinate, string name, IPlaceIdentifier containedWithin, string streetAddress)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/geo/similar_places.json");
			content.SetParameter("lat", coordinate.Latitude.ToString());
			content.SetParameter("long", coordinate.Longitude.ToString());
			content.SetParameter("name", name);
			if (containedWithin != null)
				content.SetParameter("contained_within", containedWithin.Id);
			if (!string.IsNullOrEmpty(streetAddress))
				content.SetParameter("attribute:street_address", streetAddress);
			return PlaceSearchResult.FromXml((await request.GetXmlAsync(content)).Element("result"));
		}

		public async Task<Place> CreatePlaceAsync(Point coordinate, string name, IPlaceIdentifier containedWithin, string token, string streetAddress)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/geo/place.json");
			content.SetParameter("lat", coordinate.Latitude.ToString());
			content.SetParameter("long", coordinate.Longitude.ToString());
			content.SetParameter("name", name);
			content.SetParameter("contained_within", containedWithin.Id);
			content.SetParameter("token", token);
			if (!string.IsNullOrEmpty(streetAddress))
				content.SetParameter("attribute:street_address", streetAddress);
			return Place.FromXml(await request.GetXmlAsync(content));
		}

		#endregion

		#region Trends

		public async Task<IReadOnlyList<TrendInformation>> GetTrendsAsync(long whereOnEarthId)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/trends/place.json");
			content.SetParameter("id", whereOnEarthId.ToString());
			return (await request.GetXmlAsync(content)).Elements().Select(x => TrendInformation.FromXml(x)).ToArray();
		}

		public async Task<IReadOnlyList<TrendLocation>> GetTrendAvailableLocationsAsync()
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/trends/available.json");
			return (await request.GetXmlAsync(content)).Elements().Select(x => TrendLocation.FromXml(x)).ToArray();
		}

		public async Task<IReadOnlyList<TrendLocation>> GetClosestTrendLocationsAsync(Point coordinate)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/trends/closest.json");
			content.SetParameter("lat", coordinate.Latitude.ToString());
			content.SetParameter("long", coordinate.Longitude.ToString());
			return (await request.GetXmlAsync(content)).Elements().Select(x => TrendLocation.FromXml(x)).ToArray();
		}

		#endregion

		#region Spam Reporting

		public async Task<User> ReportSpamAsync(IUserIdentifier user)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/users/report_spam.json");
			SetUser(content, user);
			return User.FromXml(await request.GetXmlAsync(content));
		}

		#endregion

		#region OAuth

		public async Task ApplyRequestTokenAsync(string callback)
		{
			request.AccessToken = null;
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/oauth/request_token");
			if (!string.IsNullOrWhiteSpace(callback))
				content.SetParameter("oauth_callback", callback);
			using (var res = await request.GetResponseAsync(content))
			{
				var str = await res.Content.ReadAsStringAsync();
				Dictionary<string, string> dict = new Dictionary<string, string>();
				foreach (var kvp in str.Split('&'))
				{
					var items = kvp.Split('=');
					dict.Add(items[0], items[1]);
				}
				request.AccessToken = new TokenSecretPair(dict["oauth_token"], dict["oauth_token_secret"]);
			}
		}

		public async Task ApplyAccessTokenAsync(string verifier)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/oauth/access_token");
			content.SetParameter("oauth_verifier", verifier);
			using (var res = await request.GetResponseAsync(content))
			{
				var str = await res.Content.ReadAsStringAsync();
				Dictionary<string, string> dict = new Dictionary<string, string>();
				foreach (var kvp in str.Split('&'))
				{
					var items = kvp.Split('=');
					dict.Add(items[0], items[1]);
				}
				request.AccessToken = new TokenSecretPair(dict["oauth_token"], dict["oauth_token_secret"]);
			}
		}

		public string CreateSignInAuthorizationUrl(TokenSecretPair requestToken) { return "https://api.twitter.com/oauth/authorize?oauth_token=" + requestToken.Token; }

		public string CreatePinAuthorizationUrl(TokenSecretPair requestToken) { return "https://api.twitter.com/oauth/authorize?oauth_token=" + requestToken.Token + "&oauth_token_secret=" + requestToken.Secret; }

		#endregion

		#region Help

		public async Task<SystemConfiguration> GetSystemConfigurationAsync()
		{
			return SystemConfiguration.FromXml(await request.GetXmlAsync(new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/help/configuration.json")));
		}

		public async Task<IReadOnlyList<CultureInfo>> GetSupportedLanguagesAsync()
		{
			return (await request.GetXmlAsync(new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/help/languages.json")))
				.Elements().Select(x => CultureInfo.GetCultureInfo(x.Element("code").Cast<string>())).ToArray();
		}

		public async Task<string> GetPrivacyPolicyAsync()
		{
			return (await request.GetXmlAsync(new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/help/privacy.json"))).Element("privacy").Cast<string>();
		}

		public async Task<string> GetTermsOfServiceAsync()
		{
			return (await request.GetXmlAsync(new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/help/tos.json"))).Element("tos").Cast<string>();
		}

		public async Task<IReadOnlyList<RateLimitEntry>> GetRateLimitStatusAsync(ResourceFamilies resources)
		{
			var content = new TwitterRequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/application/rate_limit_status.json");
			if (resources != ResourceFamilies.None)
				content.SetParameter("resources", Utils.GetResourceFamilyString(resources));
			return (await request.GetXmlAsync(content)).Element("resources").Elements().Elements().Select(element => new RateLimitEntry(
				element.Attribute("item").Value,
				int.Parse(element.Element("remaining").Value),
				int.Parse(element.Element("limit").Value),
				long.Parse(element.Element("reset").Value)
			)).ToArray();
		}

		#endregion
	}
}
