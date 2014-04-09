using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Saruna
{
	public class ProfileBanner
	{
		public int Width { get; private set; }

		public int Height { get; private set; }

		public Uri Url { get; private set; }

		public static ProfileBanner FromXml(XElement element)
		{
			var res = new ProfileBanner();
			res.Width = element.Element("w").Cast<int>();
			res.Height = element.Element("h").Cast<int>();
			var url = element.Element("url").Cast<string>();
			if (url != null)
				res.Url = new Uri(url);
			return res;
		}
	}

	public class AvailableProfileBanners
	{
		public ProfileBanner IPad { get; private set; }

		public ProfileBanner MobileRetina { get; private set; }

		public ProfileBanner Mobile { get; private set; }

		public ProfileBanner WebRetina { get; private set; }

		public ProfileBanner IPadRetina { get; private set; }

		public ProfileBanner Web { get; private set; }

		public static AvailableProfileBanners FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new AvailableProfileBanners();
			res.IPad = ProfileBanner.FromXml(element.Element("ipad"));
			res.MobileRetina = ProfileBanner.FromXml(element.Element("mobile_retina"));
			res.Mobile = ProfileBanner.FromXml(element.Element("mobile"));
			res.WebRetina = ProfileBanner.FromXml(element.Element("web_retina"));
			res.IPadRetina = ProfileBanner.FromXml(element.Element("ipad_retina"));
			res.Web = ProfileBanner.FromXml(element.Element("web"));
			return res;
		}
	}
}
