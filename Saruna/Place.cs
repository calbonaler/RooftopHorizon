using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Saruna
{
	public class Place : IPlaceIdentifier
	{
		public BoundingBox BoundingBox { get; private set; }

		public RegionInfo Country { get; private set; }

		public string FullName { get; private set; }

		public string Id { get; private set; }

		public string Name { get; private set; }

		public string Type { get; private set; }

		public Uri Url { get; private set; }

		public static Place FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new Place();
			res.BoundingBox = BoundingBox.FromXml(element.Element("bounding_box"));
			res.Country = new RegionInfo(element.Element("country_code").Cast<string>());
			res.FullName = element.Element("full_name").Cast<string>();
			res.Id = element.Element("id").Cast<string>();
			res.Name = element.Element("name").Cast<string>();
			res.Type = element.Element("place_type").Cast<string>();
			var url = element.Element("url").Cast<string>();
			if (!string.IsNullOrEmpty(url))
				res.Url = new Uri(url);
			return res;
		}
	}

	public class BoundingBox
	{
		public IReadOnlyList<Point> Coordinates { get; private set; }

		public string Type { get; private set; }

		public static BoundingBox FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new BoundingBox();
			res.Coordinates = element.Element("coordinates").Elements().First().Elements().Select(x => Point.FromXml(x)).ToArray();
			res.Type = element.Element("type").Cast<string>();
			return res;
		}
	}

	public class PlaceSearchResult
	{
		public IReadOnlyList<Place> Places { get; private set; }

		public string Token { get; private set; }

		public static PlaceSearchResult FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new PlaceSearchResult();
			res.Places = element.Element("places").Elements().Select(x => Place.FromXml(x)).ToArray();
			res.Token = element.Element("token").Cast<string>();
			return res;
		}
	}

	public struct Accuracy
	{
		public Accuracy(double value, ShortLengthUnit unit) : this()
		{
			Value = value;
			Unit = unit;
		}

		public double Value { get; set; }

		public ShortLengthUnit Unit { get; set; }

		public override string ToString()
		{
			if (Unit == ShortLengthUnit.Feets)
				return Value.ToString() + "ft";
			else
				return Value.ToString() + "m";
		}
	}

	public enum ShortLengthUnit
	{
		Meters,
		Feets,
	}

	public enum Granularity
	{
		Default,
		Point,
		Neighborhood,
		City,
		Administration,
		Country,
	}
}
