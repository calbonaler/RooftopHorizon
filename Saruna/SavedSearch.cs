using System;
using System.Globalization;
using System.Xml.Linq;

namespace Saruna
{
	public class SavedSearch : ISavedSearchIdentifier
	{
		public static SavedSearch FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new SavedSearch();
			res.CreatedTime = DateTime.ParseExact(element.Element("created_at").Cast<string>(), "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture.DateTimeFormat);
			res.Id = element.Element("id").Cast<long>();
			res.Name = element.Element("name").Cast<string>();
			res.Position = element.Element("position").Cast<string>();
			res.Query = element.Element("query").Cast<string>();
			return res;
		}

		public DateTime CreatedTime { get; private set; }

		public long Id { get; private set; }

		public string Name { get; private set; }

		public string Position { get; private set; }

		public string Query { get; private set; }
	}
}
