using System.Xml.Linq;

namespace Saruna
{
	public class SearchMetadata
	{
		public static SearchMetadata FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new SearchMetadata();
			res.MaxId = element.Element("max_id").Cast<long>();
			res.SinceId = element.Element("since_id").Cast<long>();
			res.Count = element.Element("count").Cast<int>();
			res.SearchingTime = element.Element("completed_in").Cast<double>();
			res.Query = element.Element("query").Cast<string>();
			res.RefreshParameters = element.Element("refresh_url").Cast<string>();
			res.NextResultParameters = element.Element("next_results").Cast<string>();
			return res;
		}

		public long MaxId { get; private set; }

		public long SinceId { get; private set; }

		public int Count { get; private set; }

		public double SearchingTime { get; private set; }

		public string Query { get; private set; }

		public string RefreshParameters { get; private set; }

		public string NextResultParameters { get; private set; }
	}

	public class GeometryCircle
	{
		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public double Radius { get; set; }

		public LargeLengthUnit RadiusUnit { get; set; }

		public override string ToString()
		{
			string result = Latitude.ToString() + "," + Longitude.ToString() + "," + Radius.ToString();
			if (RadiusUnit == LargeLengthUnit.Kilometers)
				return result + "km";
			else
				return result + "mi";
		}
	}

	public enum LargeLengthUnit
	{
		Kilometers,
		Miles,
	}

	public enum SearchResultType
	{
		Default,
		Recent,
		Popular,
		Mixed,
	}
}
