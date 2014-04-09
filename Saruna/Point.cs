using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Saruna
{
	public struct Point : IEquatable<Point>
	{
		public Point(double longitude, double latitude) : this()
		{
			Longitude = longitude;
			Latitude = latitude;
		}

		public static Point FromXml(XElement element)
		{
			Point res = new Point();
			if (Utils.IsJsonNull(element))
				return res;
			res.Longitude = element.Elements().ElementAt(0).Cast<double>();
			res.Latitude = element.Elements().ElementAt(1).Cast<double>();
			return res;
		}

		public double Longitude { get; set; }

		public double Latitude { get; set; }

		public static bool operator ==(Point left, Point right) { return left.Equals(right); }

		public static bool operator !=(Point left, Point right) { return !(left == right); }

		public bool Equals(Point other) { return Longitude == other.Longitude && Latitude == other.Latitude; }

		public override bool Equals(object obj)
		{
			if (obj is Point)
				return Equals((Point)obj);
			return false;
		}

		public override int GetHashCode() { return Longitude.GetHashCode() ^ Latitude.GetHashCode(); }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (Latitude >= 0)
				sb.Append("N: " + Latitude.ToString());
			else
				sb.Append("S: " + (-Latitude).ToString());
			sb.Append(", ");
			if (Longitude >= 0)
				sb.Append("E: " + Longitude.ToString());
			else
				sb.Append("W: " + (-Longitude).ToString());
			return sb.ToString();
		}
	}
}
