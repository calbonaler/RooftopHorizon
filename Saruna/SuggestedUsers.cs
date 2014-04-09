using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Saruna
{
	public class SuggestedUserList
	{
		public static SuggestedUserList FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new SuggestedUserList();
			res.Users = element.Element("users").Elements().Select(x => User.FromXml(x)).ToArray();
			res.Name = element.Element("name").Cast<string>();
			res.Slug = element.Element("slug").Cast<string>();
			res.Size = element.Element("size").Cast<int>();
			return res;
		}

		public string Name { get; private set; }

		public string Slug { get; private set; }

		public int Size { get; private set; }

		public IReadOnlyList<User> Users { get; private set; }
	}

	public class SuggestedUserCategory
	{
		public static SuggestedUserCategory FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new SuggestedUserCategory();
			res.Name = element.Element("name").Cast<string>();
			res.Slug = element.Element("slug").Cast<string>();
			res.Size = element.Element("size").Cast<int>();
			return res;
		}

		public string Name { get; private set; }

		public string Slug { get; private set; }

		public int Size { get; private set; }
	}
}
