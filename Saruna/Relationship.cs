using System.Xml.Linq;

namespace Saruna
{
	public class Relationship
	{
		public Relationship(XElement element)
		{
			var relationship = element.Element("relationship");
			{
				var target = relationship.Element("target");
				TargetId = Identifiers.CreateUser(target.Element("screen_name").Value, (long)target.Element("id"));
				IsFollowing = (bool)target.Element("followed_by");
				IsFollowedBy = (bool)target.Element("following");
			}
			{
				var source = relationship.Element("source");
				SourceId = Identifiers.CreateUser(source.Element("screen_name").Value, (long)source.Element("id"));
				IsFollowedBy = (bool)source.Element("followed_by");
				IsFollowing = (bool)source.Element("following");
				IsBlocking = (bool?)source.Element("blocking");
				ShowAllReplies = (bool?)source.Element("all_replies");
				WantRetweets = (bool?)source.Element("want_retweets");
				HasMarkedSpam = (bool?)source.Element("marked_spam");
				AreNotificationsEnabled = (bool?)source.Element("notifications_enabled");
			}
		}

		public IUserIdentifiable SourceId { get; private set; }

		public IUserIdentifiable TargetId { get; private set; }

		public bool IsFollowing { get; private set; }

		public bool IsFollowedBy { get; private set; }

		public bool? IsBlocking { get; private set; }

		public bool? ShowAllReplies { get; private set; }

		public bool? WantRetweets { get; private set; }

		public bool? HasMarkedSpam { get; private set; }

		public bool? AreNotificationsEnabled { get; private set; }
	}
}
