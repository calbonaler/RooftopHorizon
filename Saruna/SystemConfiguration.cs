using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Saruna
{
	public class SystemConfiguration
	{
		public int CharactersReservedPerMedia { get; private set; }

		public int MaximumMediaCountPerUpload { get; private set; }

		public IReadOnlyList<string> NonUserNamePaths { get; private set; }

		public long PhotoLengthLimit { get; private set; }

		public AvailableMediaSizes PhotoSizes { get; private set; }

		public int ShortenedUrlLength { get; private set; }

		public int HttpsShortenedUrlLength { get; private set; }

		public static SystemConfiguration FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new SystemConfiguration();
			res.CharactersReservedPerMedia = element.Element("characters_reserved_per_media").Cast<int>();
			res.MaximumMediaCountPerUpload = element.Element("max_media_per_upload").Cast<int>();
			res.NonUserNamePaths = element.Element("non_username_paths").Elements().Select(x => x.Cast<string>()).ToArray();
			res.PhotoLengthLimit = element.Element("photo_size_limit").Cast<long>();
			res.PhotoSizes = AvailableMediaSizes.FromXml(element.Element("photo_sizes"));
			res.ShortenedUrlLength = element.Element("short_url_length").Cast<int>();
			res.HttpsShortenedUrlLength = element.Element("short_url_length_https").Cast<int>();
			return res;
		}
	}
}
