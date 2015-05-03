using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Saruna.Streams
{
	public interface IStreamNotice
	{
		bool Assign(XElement element);
	}

	public class TweetDeletionNotice : IStreamNotice
	{
		public ITweetIdentifiable TweetId { get; private set; }

		public IUserIdentifiable UserId { get; private set; }

		Tweet tweet;
		public Tweet Tweet
		{
			get
			{
				if (tweet == null)
					tweet = Tweet.GetTweetForId(TweetId.Id);
				return tweet;
			}
		}

		public bool Assign(XElement element)
		{
			var delete = element.Element("delete");
			if (delete != null)
			{
				var status = delete.Element("status");
				if (status != null)
				{
					TweetId = Identifiers.CreateTweet(status.Element("id").Cast<long>());
					UserId = Identifiers.CreateUser(status.Element("user_id").Cast<long>());
					return true;
				}
			}
			return false;
		}
	}

	public class DirectMessageDeletionNotice : IStreamNotice
	{
		public IDirectMessageIdentifiable DirectMessageId { get; private set; }

		public IUserIdentifiable UserId { get; private set; }

		public bool Assign(XElement element)
		{
			var delete = element.Element("delete");
			if (delete != null)
			{
				var directMessage = delete.Element("direct_message");
				if (directMessage != null)
				{
					DirectMessageId = Identifiers.CreateDirectMessage(directMessage.Element("id").Cast<long>());
					UserId = Identifiers.CreateUser(directMessage.Element("user_id").Cast<long>());
					return true;
				}
			}
			return false;
		}
	}

	public class LocationDeletionNotice : IStreamNotice
	{
		public ITweetIdentifiable UpToTweetId { get; private set; }

		public IUserIdentifiable UserId { get; private set; }
		
		public bool Assign(XElement element)
		{
			var scrub_geo = element.Element("scrub_geo");
			if (scrub_geo != null)
			{
				UpToTweetId = Identifiers.CreateTweet(scrub_geo.Element("up_to_status_id").Cast<long>());
				UserId = Identifiers.CreateUser(scrub_geo.Element("user_id").Cast<long>());
				return true;
			}
			return false;
		}
	}

	public class LimitNotice : IStreamNotice
	{
		public int Track { get; private set; }

		public bool Assign(XElement element)
		{
			var limit = element.Element("limit");
			if (limit != null)
			{
				Track = limit.Element("track").Cast<int>();
				return true;
			}
			return false;
		}
	}

	public class TweetWithheldNotice : IStreamNotice
	{
		public ITweetIdentifiable TweetId { get; private set; }

		public IUserIdentifiable UserId { get; private set; }

		public IReadOnlyList<string> WithheldInCountries { get; private set; }

		public bool Assign(XElement element)
		{
			var status_withheld = element.Element("status_withheld");
			if (status_withheld != null)
			{
				TweetId = Identifiers.CreateTweet(status_withheld.Element("id").Cast<long>());
				UserId = Identifiers.CreateUser(status_withheld.Element("user_id").Cast<long>());
				WithheldInCountries = status_withheld.Element("withheld_in_countries").Elements().Select(item => item.Cast<string>()).ToArray();
				return true;
			}
			return false;
		}
	}

	public class UserWithheldNotice : IStreamNotice
	{
		public IUserIdentifiable UserId { get; private set; }

		public IReadOnlyList<string> WithheldInCountries { get; private set; }

		public bool Assign(XElement element)
		{
			var user_withheld = element.Element("user_withheld");
			if (user_withheld != null)
			{
				UserId = Identifiers.CreateUser(user_withheld.Element("id").Cast<long>());
				WithheldInCountries = user_withheld.Element("withheld_in_countries").Elements().Select(item => item.Cast<string>()).ToArray();
				return true;
			}
			return false;
		}
	}

	public class DisconnectNotice : IStreamNotice
	{
		public DisconnectStatusCode Code { get; private set; }

		public string StreamName { get; private set; }

		public string Reason { get; private set; }

		public bool Assign(XElement element)
		{
			var disconnect = element.Element("disconnect");
			if (disconnect != null)
			{
				Code = (DisconnectStatusCode)disconnect.Element("code").Cast<int>();
				StreamName = disconnect.Element("stream_name").Cast<string>();
				Reason = disconnect.Element("reason").Cast<string>();
				return true;
			}
			return false;
		}
	}

	public enum DisconnectStatusCode
	{
		Shutdown = 1,
		DuplicateStream,
		ControlRequest,
		Stall,
		Normal,
		TokenRevoked,
		AdminLogout,
		MaxMessageLimit = 9,
		StreamException,
		BrokerStall,
		ShedLoad,
	}

	public class StallNotice : IStreamNotice
	{
		public string Code { get; private set; }

		public string Message { get; private set; }

		public int PercentFull { get; private set; }

		public bool Assign(XElement element)
		{
			var warning = element.Element("warning");
			if (warning != null)
			{
				Code = warning.Element("code").Cast<string>();
				if (Code == "FALLING_BEHIND")
				{
					Message = warning.Element("message").Cast<string>();
					PercentFull = warning.Element("percent_full").Cast<int>();
					return true;
				}
			}
			return false;
		}
	}

	public class FriendListNotice : IStreamNotice
	{
		public IReadOnlyList<IUserIdentifiable> FriendIds { get; private set; }

		public bool Assign(XElement element)
		{
			var friends = element.Element("friends");
			if (friends != null)
			{
				FriendIds = friends.Elements().Select(item => Identifiers.CreateUser(item.Cast<long>())).ToArray();
				return true;
			}
			return false;
		}
	}

	public class EventNotice : IStreamNotice
	{
		public User Target { get; private set; }

		public User Source { get; private set; }

		public UserStreamEvent Event { get; private set; }

		public object TargetObject { get; private set; }

		public DateTime CreatedTime { get; private set; }

		public bool Assign(XElement element)
		{
			var target = element.Element("target");
			if (target == null)
				return false;
			Target = User.FromXml(target);
			var source = element.Element("source");
			if (source == null)
				return false;
			Source = User.FromXml(source);
			var @event = element.Element("event");
			if (@event == null)
				return false;
			switch (@event.Cast<string>())
			{
				case "block": Event = UserStreamEvent.Block; break;
				case "unblock": Event = UserStreamEvent.Unblock; break;
				case "favorite": Event = UserStreamEvent.Favorite; break;
				case "unfavorite": Event = UserStreamEvent.Unfavorite; break;
				case "follow": Event = UserStreamEvent.Follow; break;
				case "unfollow": Event = UserStreamEvent.Unfollow; break;
				case "list_created": Event = UserStreamEvent.ListCreated; break;
				case "list_destroyed": Event = UserStreamEvent.ListDestroyed; break;
				case "list_updated": Event = UserStreamEvent.ListUpdated; break;
				case "list_member_added": Event = UserStreamEvent.ListMemberAdded; break;
				case "list_member_removed": Event = UserStreamEvent.ListMemberRemoved; break;
				case "list_user_subscribed": Event = UserStreamEvent.ListUserSubscribed; break;
				case "list_user_unsubscribed": Event = UserStreamEvent.ListUserUnsubscribed; break;
				case "user_update": Event = UserStreamEvent.UserUpdated; break;
			}
			var targetObject = element.Element("target_object");
			if (targetObject == null)
				return false;
			if (Event == UserStreamEvent.Favorite || Event == UserStreamEvent.Unfavorite)
				TargetObject = Tweet.FromXml(targetObject);
			else if (Event >= UserStreamEvent.ListCreated && Event <= UserStreamEvent.ListUserUnsubscribed)
				TargetObject = List.FromXml(targetObject);
			var created_at = element.Element("created_at");
			if (created_at == null)
				return false;
			CreatedTime = DateTime.ParseExact(created_at.Cast<string>(), "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture.DateTimeFormat);
			return true;
		}
	}

	public enum UserStreamEvent
	{
		Unknown,
		Block,
		Unblock,
		Favorite,
		Unfavorite,
		Follow,
		Unfollow,
		ListCreated,
		ListDestroyed,
		ListUpdated,
		ListMemberAdded,
		ListMemberRemoved,
		ListUserSubscribed,
		ListUserUnsubscribed,
		UserUpdated,
	}

	public class TooManyFollowsNotice : IStreamNotice
	{
		public string Code { get; private set; }

		public string Message { get; private set; }

		public IUserIdentifiable UserId { get; private set; }

		public bool Assign(XElement element)
		{
			var warning = element.Element("warning");
			if (warning != null)
			{
				Code = warning.Element("code").Cast<string>();
				if (Code == "FOLLOWS_OVER_LIMIT")
				{
					Message = warning.Element("message").Cast<string>();
					UserId = Identifiers.CreateUser(warning.Element("user_id").Cast<long>());
					return true;
				}
			}
			return false;
		}
	}

	public class ControlNotice : IStreamNotice
	{
		public Uri ControlUri { get; private set; }

		public bool Assign(XElement element)
		{
			var control = element.Element("control");
			if (control != null)
			{
				var controlUri = control.Element("control_uri").Cast<string>();
				if (!string.IsNullOrEmpty(controlUri))
					ControlUri = new Uri(controlUri);
				return true;
			}
			return false;
		}
	}
}
