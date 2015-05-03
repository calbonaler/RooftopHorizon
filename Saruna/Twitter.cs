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
	public class Twitter : IAccount
	{
		public Twitter()
		{
			{
				var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/home_timeline.json");
				content.SetParameter("contributor_details", "true");
				HomeTimeline = new Timeline<Tweet>(this, content);
			}
			{
				var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/mentions_timeline.json");
				content.SetParameter("contributor_details", "true");
				MentionsTimeline = new Timeline<Tweet>(this, content);
			}
			UserStream = new TwitterStream(this, new RequestContent(HttpMethod.Get, "https://userstream.twitter.com/1.1/user.json"));
			ReceivedDirectMessageTimeline = new Timeline<DirectMessage>(this, new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/direct_messages.json"));
			SentDirectMessageTimeline = new Timeline<DirectMessage>(this, new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/direct_messages/sent.json"));
		}

		public AuthorizationToken Consumer { get; set; }

		public AuthorizationToken Access { get; set; }

		public Timeline<Tweet> HomeTimeline { get; private set; }

		public Timeline<Tweet> MentionsTimeline { get; private set; }

		public TwitterStream UserStream { get; private set; }

		public Timeline<DirectMessage> ReceivedDirectMessageTimeline { get; private set; }

		public Timeline<DirectMessage> SentDirectMessageTimeline { get; private set; }
	}

	public static class Tweets
	{
		public static async Task<Tweet> TweetAsync(this Tweet tweet, IAccount sender)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/update.json");
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
			return Tweet.FromXml(await RequestSender.GetXmlAsync(sender, content).ConfigureAwait(false));
		}

#if DEPRECATED

		public async Task<Tweet> TweetAsync(Tweet tweet, IEnumerable<byte[]> medias)
		{
			var content = new TwitterRequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/update_with_media.json", true);
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
			return Tweet.FromXml(await TwitterClient.GetXmlAsync(AuthorizationData, content).ConfigureAwait(false));
		}

#endif

		public static async Task<Tweet> RetweetAsync(this ITweetIdentifiable tweet, IAccount sender)
		{
			return Tweet.FromXml(await RequestSender.GetXmlAsync(sender, new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/retweet/" + tweet.Id.ToString() + ".json")).ConfigureAwait(false));
		}

		public static async Task<Tweet> UntweetAsync(this ITweetIdentifiable tweet, IAccount deleter)
		{
			return Tweet.FromXml(await RequestSender.GetXmlAsync(deleter, new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/statuses/destroy/" + tweet.Id.ToString() + ".json")).ConfigureAwait(false));
		}

		public static async Task<Tweet> FavoriteAsync(this ITweetIdentifiable tweet, IAccount marker)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/favorites/create.json");
			content.SetParameter("id", tweet.Id.ToString());
			return Tweet.FromXml(await RequestSender.GetXmlAsync(marker, content).ConfigureAwait(false));
		}

		public static async Task<Tweet> UnfavoriteAsync(this ITweetIdentifiable tweet, IAccount unmarker)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/favorites/destroy.json");
			content.SetParameter("id", tweet.Id.ToString());
			return Tweet.FromXml(await RequestSender.GetXmlAsync(unmarker, content).ConfigureAwait(false));
		}

		public static async Task<Tweet> ResolveAsync(this ITweetIdentifiable tweet, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/show.json");
			content.SetParameter("id", tweet.Id.ToString());
			return Tweet.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<IReadOnlyList<Tweet>> GetRetweetsAsync(this ITweetIdentifiable tweet, IAccount context, int count)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/retweets/" + tweet.Id.ToString() + ".json ");
			content.SetParameter("count", count.ToString());
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => Tweet.FromXml(x)).ToArray();
		}

		public static SearchTimeline Search(IAccount context, string query, DateTime until, GeometryCircle circle, CultureInfo language, CultureInfo locale, SearchResultType resultType)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/search/tweets.json");
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
			return new SearchTimeline(context, content);
		}
	}

	public static class Accounts
	{
		public static async Task<IReadOnlyList<Tweet>> GetTweetsAsync(this IAccount context, params ITweetIdentifiable[] tweets)
		{
			var content = new RequestContent(tweets.Length > 10 ? HttpMethod.Post : HttpMethod.Get, "https://api.twitter.com/1.1/statuses/lookup.json");
			content.SetParameter("id", string.Join(",", tweets.Select(x => x.Id)));
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => Tweet.FromXml(x)).ToArray();
		}

		public static async Task<IReadOnlyList<ConnectionEntry>> LookupFriendshipsAsync(this IAccount self, params IUserIdentifiable[] users)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/lookup.json");
			content.SetUser(users);
			return (await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false)).Elements().Select(x => ConnectionEntry.FromXml(x)).ToArray();
		}

		public static async Task<IReadOnlyList<IUserIdentifiable>> GetNoRetweetUserIdsAsync(this IAccount receiver)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/no_retweets/ids.json");
			return (await RequestSender.GetXmlAsync(receiver, content).ConfigureAwait(false)).Elements().Select(x => Identifiers.CreateUser(x.Cast<long>())).ToArray();
		}

		public static async Task<CursorNavigable<IUserIdentifiable>> GetIncomingFriendshipsAsync(this IAccount receiver, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/incoming.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<IUserIdentifiable>.FromXml(await RequestSender.GetXmlAsync(receiver, content).ConfigureAwait(false), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public static async Task<CursorNavigable<IUserIdentifiable>> GetOutgoingFriendshipsAsync(this IAccount sender, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/outgoing.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<IUserIdentifiable>.FromXml(await RequestSender.GetXmlAsync(sender, content).ConfigureAwait(false), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public static async Task<Relationship> GetRelationshipAsync(this IAccount context, IUserIdentifiable source, IUserIdentifiable target)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friendships/show.json");
			content.SetUser("source_id", "source_screen_name", source);
			content.SetUser("target_id", "target_screen_name", target);
			return new Relationship(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<IReadOnlyList<User>> GetUsersAsync(this IAccount context, params IUserIdentifiable[] users)
		{
			var content = new RequestContent(users.Length > 10 ? HttpMethod.Post : HttpMethod.Get, "https://api.twitter.com/1.1/users/lookup.json");
			content.SetUser(users);
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		public static async Task<User> VerifyCredentialsAsync(this IAccount self)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/account/verify_credentials.json");
			return User.FromXml(await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false));
		}

		public static async Task<UserSettings> GetSettingsAsync(this IAccount self)
		{
			return UserSettings.FromXml(await RequestSender.GetXmlAsync(self, new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/account/settings.json")).ConfigureAwait(false));
		}

		public static async Task<UserSettings> ChangeSettingsAsync(this IAccount self, long? whereOnEarthId = null, TimeInterval sleepTime = null, TimeZoneInfo timeZone = null, CultureInfo language = null)
		{
			RequestContent content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/settings.json");
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
			return UserSettings.FromXml(await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false));
		}

		public static async Task ChangeDeliveryDeviceAsync(this IAccount self, DeliveryDevice device)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_delivery_device.json");
			content.SetParameter("device", device.ToString().ToLower());
			await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false);
		}

		public static async Task<User> ChangeProfileAsync(this IAccount self, string name = null, string url = null, string location = null, string description = null)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile.json");
			if (name != null)
				content.SetParameter("name", name);
			if (url != null)
				content.SetParameter("url", url);
			if (location != null)
				content.SetParameter("location", location);
			if (description != null)
				content.SetParameter("description", description);
			return User.FromXml(await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false));
		}

		public static async Task<User> ChangeBackgroundImageAsync(this IAccount self, byte[] image = null, bool? tile = null)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_background_image.json");
			if (image != null)
				content.SetParameter("image", Convert.ToBase64String(image));
			if (tile != null)
				content.SetParameter("tile", tile.ToString().ToLower());
			return User.FromXml(await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false));
		}

		public static async Task<User> RemoveBackgroundImageAsync(this IAccount self)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_background_image.json");
			content.SetParameter("use", "false");
			return User.FromXml(await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false));
		}

		public static async Task<User> ChangeColorsAsync(this IAccount self, Color background, Color link, Color sidebarBorder, Color sidebarFill, Color text)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_colors.json");
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
			return User.FromXml(await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false));
		}

		public static async Task<User> ChangeImageAsync(this IAccount self, byte[] image)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_image.json");
			content.SetParameter("image", Convert.ToBase64String(image));
			return User.FromXml(await RequestSender.GetXmlAsync(self, content).ConfigureAwait(false));
		}

		public static async Task ChangeBannerAsync(this IAccount self, byte[] image, Rectangle region)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/update_profile_banner.json");
			content.SetParameter("banner", Convert.ToBase64String(image));
			if (!region.IsEmpty)
			{
				content.SetParameter("width", region.Width.ToString());
				content.SetParameter("height", region.Height.ToString());
				content.SetParameter("offset_left", region.X.ToString());
				content.SetParameter("offset_top", region.Y.ToString());
			}
			await RequestSender.SendAsync(self, content).ConfigureAwait(false);
		}

		public static async Task RemoveBannerAsync(this IAccount self)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/account/remove_profile_banner.json");
			await RequestSender.SendAsync(self, content).ConfigureAwait(false);
		}

		public static async Task<CursorNavigable<User>> GetBlockedUsersAsync(this IAccount blocker, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/blocks/list.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await RequestSender.GetXmlAsync(blocker, content).ConfigureAwait(false), "users", x => User.FromXml(x));
		}

		public static async Task<CursorNavigable<IUserIdentifiable>> GetBlockedUserIdsAsync(this IAccount blocker, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/blocks/ids.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<IUserIdentifiable>.FromXml(await RequestSender.GetXmlAsync(blocker, content).ConfigureAwait(false), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public static async Task<CursorNavigable<User>> GetMutedUsersAsync(this IAccount muter, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/mutes/users/list.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await RequestSender.GetXmlAsync(muter, content).ConfigureAwait(false), "users", x => User.FromXml(x));
		}

		public static async Task<CursorNavigable<IUserIdentifiable>> GetMutedUserIdsAsync(this IAccount muter, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/mutes/users/ids.json");
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<IUserIdentifiable>.FromXml(await RequestSender.GetXmlAsync(muter, content).ConfigureAwait(false), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public static async Task<SavedSearch> SaveSearchAsync(this IAccount saver, string query)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/saved_searches/create.json");
			content.SetParameter("query", query);
			return SavedSearch.FromXml(await RequestSender.GetXmlAsync(saver, content).ConfigureAwait(false));
		}

		public static async Task<IReadOnlyList<SavedSearch>> GetSavedSearchesAsync(this IAccount holder)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/saved_searches/list.json");
			return (await RequestSender.GetXmlAsync(holder, content).ConfigureAwait(false)).Elements().Select(x => SavedSearch.FromXml(x)).ToArray();
		}

		public static async Task<SystemConfiguration> GetSystemConfigurationAsync(this IAccount account)
		{
			return SystemConfiguration.FromXml(await RequestSender.GetXmlAsync(account, new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/help/configuration.json")).ConfigureAwait(false));
		}

		public static async Task<IReadOnlyList<RateLimitEntry>> GetRateLimitStatusAsync(this IAccount account, ResourceFamilies resources)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/application/rate_limit_status.json");
			if (resources != ResourceFamilies.None)
				content.SetParameter("resources", Utils.GetResourceFamilyString(resources));
			return (await RequestSender.GetXmlAsync(account, content).ConfigureAwait(false)).Element("resources").Elements().Elements().Select(element => new RateLimitEntry(
				element.Attribute("item").Value,
				int.Parse(element.Element("remaining").Value),
				int.Parse(element.Element("limit").Value),
				long.Parse(element.Element("reset").Value)
			)).ToArray();
		}
	}

	public static class DirectMessages
	{
		public static async Task<DirectMessage> SendAsync(IAccount sender, IUserIdentifiable recipient, string text)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/direct_messages/new.json");
			content.SetUser(recipient);
			content.SetParameter("text", text);
			return DirectMessage.FromXml(await RequestSender.GetXmlAsync(sender, content).ConfigureAwait(false));
		}

		public static async Task<DirectMessage> ResolveAsync(this IDirectMessageIdentifiable message, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/direct_messages/show.json");
			content.SetParameter("id", message.Id.ToString());
			return DirectMessage.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<DirectMessage> DeleteAsync(this IDirectMessageIdentifiable message, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/direct_messages/destroy.json");
			content.SetParameter("id", message.Id.ToString());
			return DirectMessage.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}
	}

	public static class Users
	{
		public static Timeline<Tweet> GetTimeline(this IUserIdentifiable user, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/statuses/user_timeline.json");
			content.SetParameter("contributor_details", "true");
			content.SetUser(user);
			return new Timeline<Tweet>(context, content);
		}

		public static async Task<CursorNavigable<IUserIdentifiable>> GetFriendIdsAsync(this IUserIdentifiable user, IAccount context, Cursor cursor, int usersPerPage)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friends/ids.json");
			content.SetUser(user);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("count", usersPerPage.ToString());
			return CursorNavigable<IUserIdentifiable>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public static async Task<CursorNavigable<IUserIdentifiable>> GetFollowerIdsAsync(this IUserIdentifiable user, IAccount context, Cursor cursor, int usersPerPage)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/followers/ids.json");
			content.SetUser(user);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("count", usersPerPage.ToString());
			return CursorNavigable<IUserIdentifiable>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "ids", x => Identifiers.CreateUser(x.Cast<long>()));
		}

		public static async Task<CursorNavigable<User>> GetFriendsAsync(this IUserIdentifiable user, IAccount context, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/friends/list.json");
			content.SetUser(user);
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "users", x => User.FromXml(x));
		}

		public static async Task<CursorNavigable<User>> GetFollowersAsync(this IUserIdentifiable user, IAccount context, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/followers/list.json");
			content.SetUser(user);
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "users", x => User.FromXml(x));
		}

		public static async Task<User> FollowAsync(this IUserIdentifiable user, IAccount follower, bool? enableNotifications)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/friendships/create.json");
			content.SetUser(user);
			if (enableNotifications != null)
				content.SetParameter("follow", enableNotifications.ToString().ToLower());
			return User.FromXml(await RequestSender.GetXmlAsync(follower, content).ConfigureAwait(false));
		}

		public static async Task<User> UnfollowAsync(this IUserIdentifiable user, IAccount unfollower)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/friendships/destroy.json");
			content.SetUser(user);
			return User.FromXml(await RequestSender.GetXmlAsync(unfollower, content).ConfigureAwait(false));
		}

		public static async Task<Relationship> ChangeNotificationsAsync(this IUserIdentifiable user, IAccount receiver, bool? enableNotifications = null, bool? showRetweets = null)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/friendships/update.json");
			content.SetUser(user);
			if (enableNotifications != null)
				content.SetParameter("device", enableNotifications.ToString().ToLower());
			if (showRetweets != null)
				content.SetParameter("retweets", showRetweets.ToString().ToLower());
			return new Relationship(await RequestSender.GetXmlAsync(receiver, content).ConfigureAwait(false));
		}

		public static async Task<AvailableProfileBanners> GetProfileBannerAsync(this IUserIdentifiable user, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/users/profile_banner.json");
			content.SetUser(user);
			return AvailableProfileBanners.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<User> ResolveAsync(this IUserIdentifiable user, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/show.json");
			content.SetUser(user);
			return User.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<IReadOnlyList<User>> GetContributeesAsync(this IUserIdentifiable user, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/contributees.json");
			content.SetUser(user);
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		public static async Task<IReadOnlyList<User>> GetContributorsAsync(this IUserIdentifiable user, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/contributors.json");
			content.SetUser(user);
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		public static async Task<IReadOnlyList<User>> SearchAsync(IAccount context, string query, int usersPerPage, int? page)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/search.json");
			content.SetParameter("q", query);
			if (page != null)
				content.SetParameter("page", page.ToString());
			content.SetParameter("count", usersPerPage.ToString());
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => User.FromXml(x)).ToArray();
		}

		public static Timeline<Tweet> GetFavoritesTimeline(this IUserIdentifiable user, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/favorites/list.json");
			content.SetUser(user);
			return new Timeline<Tweet>(context, content);
		}

		public static async Task<User> BlockAsync(this IUserIdentifiable user, IAccount blocker)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/blocks/create.json");
			content.SetUser(user);
			return User.FromXml(await RequestSender.GetXmlAsync(blocker, content).ConfigureAwait(false));
		}

		public static async Task<User> UnblockAsync(this IUserIdentifiable user, IAccount unblocker)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/blocks/destroy.json");
			content.SetUser(user);
			return User.FromXml(await RequestSender.GetXmlAsync(unblocker, content).ConfigureAwait(false));
		}

		public static async Task<User> MuteAsync(this IUserIdentifiable user, IAccount muter)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/mutes/users/create.json");
			content.SetUser(user);
			return User.FromXml(await RequestSender.GetXmlAsync(muter, content).ConfigureAwait(false));
		}

		public static async Task<User> UnmuteAsync(this IUserIdentifiable user, IAccount unmuter)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/mutes/users/destroy.json");
			content.SetUser(user);
			return User.FromXml(await RequestSender.GetXmlAsync(unmuter, content).ConfigureAwait(false));
		}

		public static async Task<User> ReportSpamAsync(this IUserIdentifiable user, IAccount reporter)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/users/report_spam.json");
			content.SetUser(user);
			return User.FromXml(await RequestSender.GetXmlAsync(reporter, content).ConfigureAwait(false));
		}

		public static async Task<IReadOnlyList<List>> GetRelatedListsAsync(this IUserIdentifiable user, IAccount context, bool ownedListFirst)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/list.json");
			content.SetUser(user);
			content.SetParameter("reverse", ownedListFirst.ToString().ToLower());
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => List.FromXml(x)).ToArray();
		}

		public static async Task<CursorNavigable<List>> GetBelongingListsAsync(this IUserIdentifiable user, IAccount context, Cursor cursor, bool? ownedListsOnly)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/memberships.json");
			content.SetUser(user);
			content.SetParameter("cursor", cursor.Value.ToString());
			if (ownedListsOnly != null)
				content.SetParameter("filter_to_owned_lists", ownedListsOnly.ToString().ToLower());
			return CursorNavigable<List>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "lists", x => List.FromXml(x));
		}

		public static async Task<CursorNavigable<List>> GetSubscribingListsAsync(this IUserIdentifiable user, IAccount context, Cursor cursor, int listsPerPage)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/subscriptions.json");
			content.SetUser(user);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("count", listsPerPage.ToString());
			return CursorNavigable<List>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "lists", x => List.FromXml(x));
		}

		public static async Task<CursorNavigable<List>> GetOwningListsAsync(this IUserIdentifiable user, IAccount context, Cursor cursor, int listsPerPage)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/ownerships.json");
			content.SetUser(user);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("count", listsPerPage.ToString());
			return CursorNavigable<List>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "lists", x => List.FromXml(x));
		}
	}

	public static class Lists
	{
		public static async Task<List> CreateAsync(IAccount owner, string name, ListMode mode, string description)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/create.json");
			content.SetParameter("name", name);
			if (mode != ListMode.Default)
				content.SetParameter("mode", mode.ToString().ToLower());
			if (!string.IsNullOrEmpty(description))
				content.SetParameter("description", description);
			return List.FromXml(await RequestSender.GetXmlAsync(owner, content).ConfigureAwait(false));
		}

		public static Timeline<Tweet> GetTimeline(this IListIdentifiable list, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/statuses.json");
			content.SetList(list);
			content.SetParameter("include_rts", "true");
			return new Timeline<Tweet>(context, content);
		}

		public static async Task<User> GetMemberAsync(this IListIdentifiable list, IAccount context, IUserIdentifiable user)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/members/show.json");
			content.SetList(list);
			content.SetUser(user);
			content.SetParameter("include_entities", "true");
			return User.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<CursorNavigable<User>> GetMembersAsync(this IListIdentifiable list, IAccount context, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/members.json");
			content.SetList(list);
			content.SetParameter("cursor", cursor.Value.ToString());
			return CursorNavigable<User>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "users", x => User.FromXml(x));
		}

		public static async Task AddMemberAsync(this IListIdentifiable list, IAccount context, IUserIdentifiable user)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/members/create.json");
			content.SetList(list);
			content.SetUser(user);
			await RequestSender.SendAsync(context, content).ConfigureAwait(false);
		}

		public static async Task AddMembersAsync(this IListIdentifiable list, IAccount context, params IUserIdentifiable[] users)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/members/create_all.json");
			content.SetList(list);
			content.SetUser(users);
			await RequestSender.SendAsync(context, content).ConfigureAwait(false);
		}

		public static async Task RemoveMemberAsync(this IListIdentifiable list, IAccount context, IUserIdentifiable user)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/members/destroy.json");
			content.SetList(list);
			content.SetUser(user);
			await RequestSender.SendAsync(context, content).ConfigureAwait(false);
		}

		public static async Task RemoveMembersAsync(this IListIdentifiable list, IAccount context, params IUserIdentifiable[] users)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/members/destroy_all.json");
			content.SetList(list);
			content.SetUser(users);
			await RequestSender.SendAsync(context, content).ConfigureAwait(false);
		}

		public static async Task<User> GetSubscriberAsync(this IListIdentifiable list, IAccount context, IUserIdentifiable user)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/subscribers/show.json");
			content.SetList(list);
			content.SetUser(user);
			content.SetParameter("include_entities", "true");
			return User.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<CursorNavigable<User>> GetSubscribersAsync(this IListIdentifiable list, IAccount context, Cursor cursor)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/subscribers.json");
			content.SetList(list);
			content.SetParameter("cursor", cursor.Value.ToString());
			content.SetParameter("include_entities", "true");
			return CursorNavigable<User>.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false), "users", x => User.FromXml(x));
		}

		public static async Task<List> DeleteAsync(this IListIdentifiable list, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/destroy.json");
			content.SetList(list);
			return List.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task ChangeAsync(this IListIdentifiable list, IAccount context, string name = null, ListMode mode = ListMode.Default, string description = null)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/update.json");
			content.SetList(list);
			if (name != null)
				content.SetParameter("name", name);
			if (mode != ListMode.Default)
				content.SetParameter("mode", mode.ToString().ToLower());
			if (description != null)
				content.SetParameter("description", description);
			await RequestSender.SendAsync(context, content).ConfigureAwait(false);
		}

		public static async Task<List> ResolveAsync(this IListIdentifiable list, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/lists/show.json");
			content.SetList(list);
			return List.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<List> SubscribeAsync(this IListIdentifiable list, IAccount subscriber)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/subscribers/create.json");
			content.SetList(list);
			return List.FromXml(await RequestSender.GetXmlAsync(subscriber, content).ConfigureAwait(false));
		}

		public static async Task UnsubscribeAsync(this IListIdentifiable list, IAccount unsubscriber)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/lists/subscribers/destroy.json");
			content.SetList(list);
			await RequestSender.SendAsync(unsubscriber, content).ConfigureAwait(false);
		}
	}

	public static class SavedSearches
	{
		public static async Task<SavedSearch> ResolveAsync(this ISavedSearchIdentifiable search, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/saved_searches/show/" + search.Id.ToString() + ".json");
			return SavedSearch.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<SavedSearch> DeleteAsync(this ISavedSearchIdentifiable search, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/saved_searches/destroy/" + search.Id.ToString() + ".json");
			return SavedSearch.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}
	}

	public static class Places
	{
		public static async Task<Place> ResolveAsync(this IPlaceIdentifiable place, IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/geo/id/" + place.Id + ".json");
			return Place.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<IReadOnlyList<Place>> SearchFromCoordinateAsync(IAccount context, Point coordinate, Accuracy? accuracy, Granularity granularity, int? maxResults)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/geo/reverse_geocode.json");
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
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Element("result").Element("places").Elements().Select(x => Place.FromXml(x)).ToArray();
		}

		public static async Task<IReadOnlyList<Place>> SearchAsync(IAccount context, Point? coordinate, string query, string ip, Accuracy? accuracy, Granularity granularity, int? maxResults, IPlaceIdentifiable containedWithin, string streetAddress)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/geo/search.json");
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
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Element("result").Element("places").Elements().Select(x => Place.FromXml(x)).ToArray();
		}

		public static async Task<PlaceSearchResult> SearchSimilarAsync(IAccount context, Point coordinate, string name, IPlaceIdentifiable containedWithin, string streetAddress)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/geo/similar_places.json");
			content.SetParameter("lat", coordinate.Latitude.ToString());
			content.SetParameter("long", coordinate.Longitude.ToString());
			content.SetParameter("name", name);
			if (containedWithin != null)
				content.SetParameter("contained_within", containedWithin.Id);
			if (!string.IsNullOrEmpty(streetAddress))
				content.SetParameter("attribute:street_address", streetAddress);
			return PlaceSearchResult.FromXml((await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Element("result"));
		}

		public static async Task<Place> CreateAsync(this IPlaceIdentifiable containedWithin, IAccount creator, Point coordinate, string name, string token, string streetAddress)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/1.1/geo/place.json");
			content.SetParameter("lat", coordinate.Latitude.ToString());
			content.SetParameter("long", coordinate.Longitude.ToString());
			content.SetParameter("name", name);
			content.SetParameter("contained_within", containedWithin.Id);
			content.SetParameter("token", token);
			if (!string.IsNullOrEmpty(streetAddress))
				content.SetParameter("attribute:street_address", streetAddress);
			return Place.FromXml(await RequestSender.GetXmlAsync(creator, content).ConfigureAwait(false));
		}
	}

	public static class SuggestedUsers
	{
		public static async Task<IReadOnlyList<SuggestedUserCategory>> GetCategoriesAsync(IAccount context, CultureInfo language)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/suggestions.json");
			if (language != null)
				content.SetParameter("lang", language.TwoLetterISOLanguageName);
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => SuggestedUserCategory.FromXml(x)).ToArray();
		}

		public static async Task<SuggestedUserList> GetListAsync(IAccount context, string slug, CultureInfo language)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/suggestions/" + slug + ".json");
			if (language != null)
				content.SetParameter("lang", language.TwoLetterISOLanguageName);
			return SuggestedUserList.FromXml(await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false));
		}

		public static async Task<IReadOnlyList<User>> GetUsersAsync(IAccount context, string slug)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/users/suggestions/" + slug + "/members.json");
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => User.FromXml(x)).ToArray();
		}
	}

	public static class Trends
	{
		public static async Task<IReadOnlyList<TrendInformation>> GetTrendsAsync(IAccount context, long whereOnEarthId)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/trends/place.json");
			content.SetParameter("id", whereOnEarthId.ToString());
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => TrendInformation.FromXml(x)).ToArray();
		}

		public static async Task<IReadOnlyList<TrendLocation>> GetAvailableLocationsAsync(IAccount context)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/trends/available.json");
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => TrendLocation.FromXml(x)).ToArray();
		}

		public static async Task<IReadOnlyList<TrendLocation>> GetClosestLocationsAsync(IAccount context, Point coordinate)
		{
			var content = new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/trends/closest.json");
			content.SetParameter("lat", coordinate.Latitude.ToString());
			content.SetParameter("long", coordinate.Longitude.ToString());
			return (await RequestSender.GetXmlAsync(context, content).ConfigureAwait(false)).Elements().Select(x => TrendLocation.FromXml(x)).ToArray();
		}
	}

	public static class Authentication
	{
		public static async Task<IAccount> GetTemporaryAccountAsync(AuthorizationToken consumer, string callback)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/oauth/request_token");
			if (!string.IsNullOrWhiteSpace(callback))
				content.SetParameter("oauth_callback", callback);
			using (var res = await RequestSender.GetResponseAsync(new AccountStub(consumer, null), content).ConfigureAwait(false))
			{
				var str = await res.Content.Content.ReadAsStringAsync();
				Dictionary<string, string> dict = new Dictionary<string, string>();
				foreach (var kvp in str.Split('&'))
				{
					var items = kvp.Split('=');
					dict.Add(items[0], items[1]);
				}
				return new AccountStub(consumer, new AuthorizationToken(dict["oauth_token"], dict["oauth_token_secret"]));
			}
		}

		public static async Task<IAccount> GetPersistentAccountAsync(IAccount temporary, string verifier)
		{
			var content = new RequestContent(HttpMethod.Post, "https://api.twitter.com/oauth/access_token");
			content.SetParameter("oauth_verifier", verifier);
			using (var res = await RequestSender.GetResponseAsync(temporary, content).ConfigureAwait(false))
			{
				var str = await res.Content.Content.ReadAsStringAsync();
				Dictionary<string, string> dict = new Dictionary<string, string>();
				foreach (var kvp in str.Split('&'))
				{
					var items = kvp.Split('=');
					dict.Add(items[0], items[1]);
				}
				return new AccountStub(temporary.Consumer, new AuthorizationToken(dict["oauth_token"], dict["oauth_token_secret"]));
			}
		}

		public static string CreateSignInAuthenticationUrl(AuthorizationToken requestToken) { return "https://api.twitter.com/oauth/authenticate?oauth_token=" + requestToken.Key; }
	}

	static class RequestContentExtensions
	{
		public static void SetUser(this RequestContent content, params IUserIdentifiable[] users) { SetUser(content, "screen_name", "user_id", users); }

		public static void SetUser(this RequestContent content, string screenNameKey, string idKey, params IUserIdentifiable[] users)
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

		public static void SetList(this RequestContent content, IListIdentifiable list)
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
	}

	static class Help
	{
		public static async Task<IReadOnlyList<CultureInfo>> GetSupportedLanguagesAsync(IAccount context)
		{
			return (await RequestSender.GetXmlAsync(context, new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/help/languages.json")).ConfigureAwait(false))
				.Elements().Select(x => CultureInfo.GetCultureInfo(x.Element("code").Cast<string>())).ToArray();
		}

		public static async Task<string> GetPrivacyPolicyAsync(IAccount context)
		{
			return (await RequestSender.GetXmlAsync(context, new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/help/privacy.json")).ConfigureAwait(false)).Element("privacy").Cast<string>();
		}

		public static async Task<string> GetTermsOfServiceAsync(IAccount context)
		{
			return (await RequestSender.GetXmlAsync(context, new RequestContent(HttpMethod.Get, "https://api.twitter.com/1.1/help/tos.json")).ConfigureAwait(false)).Element("tos").Cast<string>();
		}
	}
}
