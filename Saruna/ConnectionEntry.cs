using System;
using System.Linq;
using System.Xml.Linq;

namespace Saruna
{
	public class ConnectionEntry
	{
		public string Name { get; private set; }

		public IUserIdentifiable Id { get; private set; }

		public Connections Connection { get; private set; }

		public static ConnectionEntry FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new ConnectionEntry();
			res.Name = (string)element.Element("name");
			res.Id = Identifiers.CreateUser((string)element.Element("screen_name"), (long)element.Element("id"));
			res.Connection = element.Element("connections").Elements().Select(x =>
			{
				switch ((string)x)
				{
					case "following":
						return Connections.Following;
					case "following_requested":
						return Connections.FollowingRequested;
					case "followed_by":
						return Connections.FollowedBy;
					default:
						return Connections.None;
				}
			}).Aggregate((a, b) => a | b);
			return res;
		}
	}

	[Flags]
	public enum Connections
	{
		None = 0,
		Following = 1,
		FollowingRequested = 2,
		FollowedBy = 4,
	}
}
