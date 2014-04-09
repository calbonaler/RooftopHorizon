using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Saruna
{
	public class Trend
	{
		public static Trend FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new Trend();
			res.Events = element.Element("events").Cast<string>();
			res.Name = element.Element("name").Cast<string>();
			res.PromotedContent = element.Element("promoted_content").Cast<string>();
			res.Query = element.Element("query").Cast<string>();
			var url = element.Element("url").Cast<string>();
			if (!string.IsNullOrEmpty(url))
				res.Url = new Uri(url);
			return res;
		}

		public string Events { get; private set; }

		public string Name { get; private set; }

		public string PromotedContent { get; private set; }

		public string Query { get; private set; }

		public Uri Url { get; private set; }

		public override string ToString() { return Name; }
	}

	public class TrendLocation
	{
		public static TrendLocation FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new TrendLocation();
			res.Country = new RegionInfo(element.Element("countryCode").Cast<string>());
			res.Name = element.Element("name").Cast<string>();
			res.ParentId = element.Element("parentid").Cast<long>();
			res.PlaceType = PlaceType.FromXml(element.Element("placeType"));
			var url = element.Element("url").Cast<string>();
			if (!string.IsNullOrEmpty(url))
				res.Url = new Uri(url);
			res.WhereOnEarthId = element.Element("woeid").Cast<long>();
			return res;
		}

		public RegionInfo Country { get; private set; }

		public string Name { get; private set; }

		public long ParentId { get; private set; }

		public PlaceType PlaceType { get; private set; }

		public Uri Url { get; private set; }

		public long WhereOnEarthId { get; private set; }

		public override string ToString() { return Name; }
	}

	public class PlaceType
	{
		public static PlaceType FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new PlaceType();
			res.Code = element.Element("code").Cast<long>();
			res.Name = element.Element("name").Cast<string>();
			return res;
		}

		public long Code { get; private set; }

		public string Name { get; private set; }

		public override string ToString() { return Name; }
	}

	public class TrendInformation
	{
		public static TrendInformation FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new TrendInformation();
			res.CreatedTime = DateTime.ParseExact(element.Element("created_at").Cast<string>(), "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture.DateTimeFormat);
			res.AcquiredTime = DateTime.ParseExact(element.Element("as_of").Cast<string>(), "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture.DateTimeFormat);
			res.Locations = element.Element("locations").Elements().Select(x => TrendLocation.FromXml(x)).ToArray();
			res.Trends = element.Element("trends").Elements().Select(x => Trend.FromXml(x)).ToArray();
			return res;
		}

		public DateTime CreatedTime { get; private set; }

		public DateTime AcquiredTime { get; private set; }

		public IReadOnlyList<TrendLocation> Locations { get; private set; }

		public IReadOnlyList<Trend> Trends { get; private set; }
	}
}
