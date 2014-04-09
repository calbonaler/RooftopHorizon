using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Saruna
{
	public class TweetEntities
	{
		public IReadOnlyList<TweetHashtag> Hashtags { get; private set; }

		public IReadOnlyList<TweetMedia> Media { get; private set; }

		public IReadOnlyList<TweetUrl> Urls { get; private set; }

		public IReadOnlyList<TweetUserMention> UserMentions { get; private set; }

		public IEnumerable<ITweetEntity> All { get { return ((IEnumerable<ITweetEntity>)Hashtags).Concat(Media).Concat(Urls).Concat(UserMentions); } }

		public static TweetEntities FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			TweetEntities res = new TweetEntities();
			if (!Utils.IsJsonNull(element.Element("hashtags")))
				res.Hashtags = element.Element("hashtags").Elements().Select(x => TweetHashtag.FromXml(x)).ToArray();
			else
				res.Hashtags = new TweetHashtag[0];
			if (!Utils.IsJsonNull(element.Element("media")))
				res.Media = element.Element("media").Elements().Select(x => TweetMedia.FromXml(x)).ToArray();
			else
				res.Media = new TweetMedia[0];
			if (!Utils.IsJsonNull(element.Element("urls")))
				res.Urls = element.Element("urls").Elements().Select(x => TweetUrl.FromXml(x)).ToArray();
			else
				res.Urls = new TweetUrl[0];
			if (!Utils.IsJsonNull(element.Element("user_mentions")))
				res.UserMentions = element.Element("user_mentions").Elements().Select(x => TweetUserMention.FromXml(x)).ToArray();
			else
				res.UserMentions = new TweetUserMention[0];
			return res;
		}
	}

	public interface ITweetEntity
	{
		int Start { get; }
		int End { get; }
	}

	public interface INavigableTweetEntity : ITweetEntity
	{
		string DisplayUrl { get; }
		Uri ExpandedUrl { get; }
		Uri Url { get; }
	}

	public class TweetHashtag : ITweetEntity
	{
		public int Start { get; private set; }

		public int End { get; private set; }

		public string Text { get; private set; }

		public override string ToString() { return Text; }

		public static TweetHashtag FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new TweetHashtag();
			if (!Utils.IsJsonNull(element.Element("indices")))
			{
				res.Start = element.Element("indices").Elements().ElementAt(0).Cast<int>();
				res.End = element.Element("indices").Elements().ElementAt(1).Cast<int>();
			}
			res.Text = element.Element("text").Cast<string>();
			return res;
		}
	}

	public class TweetUserMention : ITweetEntity
	{
		public IUserIdentifier Id { get; private set; }

		public int Start { get; private set; }

		public int End { get; private set; }

		public string Name { get; private set; }

		public string ScreenName { get { return Id.ScreenName; } }

		public static TweetUserMention FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new TweetUserMention();
			if (!Utils.IsJsonNull(element.Element("indices")))
			{
				res.Start = element.Element("indices").Elements().ElementAt(0).Cast<int>();
				res.End = element.Element("indices").Elements().ElementAt(1).Cast<int>();
			}
			res.Id = Identifiers.CreateUser(element.Element("screen_name").Cast<string>(), element.Element("id").Cast<long>());
			res.Name = element.Element("name").Cast<string>();
			return res;
		}

		public override string ToString() { return Name; }
	}

	public class TweetMedia : INavigableTweetEntity
	{
		public string DisplayUrl { get; private set; }

		public Uri ExpandedUrl { get; private set; }

		public long Id { get; private set; }

		public int Start { get; private set; }

		public int End { get; private set; }

		public Uri MediaUrl { get; private set; }

		public Uri SecureMediaUrl { get; private set; }

		public AvailableMediaSizes AvailableSizes { get; private set; }

		public ITweetIdentifier SourceTweetId { get; private set; }

		public string Type { get; private set; }

		public Uri Url { get; private set; }

		public override string ToString() { return Url.ToString(); }

		public static TweetMedia FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new TweetMedia();
			if (!Utils.IsJsonNull(element.Element("indices")))
			{
				res.Start = element.Element("indices").Elements().ElementAt(0).Cast<int>();
				res.End = element.Element("indices").Elements().ElementAt(1).Cast<int>();
			}
			res.DisplayUrl = element.Element("display_url").Cast<string>();
			var expandedUrl = element.Element("expanded_url").Cast<string>();
			if (!string.IsNullOrEmpty(expandedUrl))
				res.ExpandedUrl = new Uri(expandedUrl);
			res.Id = element.Element("id").Cast<long>();
			var mediaUrl = element.Element("media_url").Cast<string>();
			if (!string.IsNullOrEmpty(mediaUrl))
				res.MediaUrl = new Uri(mediaUrl);
			var mediaUrlHttps = element.Element("media_url_https").Cast<string>();
			if (!string.IsNullOrEmpty(mediaUrlHttps))
				res.SecureMediaUrl = new Uri(mediaUrlHttps);
			res.AvailableSizes = AvailableMediaSizes.FromXml(element.Element("sizes"));
			res.SourceTweetId = Identifiers.CreateTweet(element.Element("source_status_id").Cast<long?>());
			res.Type = element.Element("type").Cast<string>();
			var url = element.Element("url").Cast<string>();
			if (!string.IsNullOrEmpty(url))
				res.Url = new Uri(url);
			return res;
		}
	}

	public class AvailableMediaSizes
	{
		public MediaSize Thumbnail { get; private set; }

		public MediaSize Large { get; private set; }

		public MediaSize Medium { get; private set; }

		public MediaSize Small { get; private set; }

		public static AvailableMediaSizes FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new AvailableMediaSizes();
			res.Thumbnail = MediaSize.FromXml(element.Element("thumb"));
			res.Large = MediaSize.FromXml(element.Element("large"));
			res.Medium = MediaSize.FromXml(element.Element("medium"));
			res.Small = MediaSize.FromXml(element.Element("small"));
			return res;
		}
	}

	public class MediaSize
	{
		public int Height { get; private set; }

		public string ResizingMethod { get; private set; }

		public int Width { get; private set; }

		public static MediaSize FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new MediaSize();
			res.Height = element.Element("h").Cast<int>();
			res.ResizingMethod = element.Element("resize").Cast<string>();
			res.Width = element.Element("w").Cast<int>();
			return res;
		}

		public override string ToString() { return Width.ToString() + ", " + Height.ToString(); }
	}

	public class TweetUrl : INavigableTweetEntity
	{
		public string DisplayUrl { get; private set; }

		public Uri ExpandedUrl { get; private set; }

		public int Start { get; private set; }

		public int End { get; private set; }

		public Uri Url { get; private set; }

		public static TweetUrl FromXml(XElement element)
		{
			if (Utils.IsJsonNull(element))
				return null;
			var res = new TweetUrl();
			if (!Utils.IsJsonNull(element.Element("indices")))
			{
				res.Start = element.Element("indices").Elements().ElementAt(0).Cast<int>();
				res.End = element.Element("indices").Elements().ElementAt(1).Cast<int>();
			}
			res.DisplayUrl = element.Element("display_url").Cast<string>();
			var expandedUrl = element.Element("expanded_url").Cast<string>();
			if (!string.IsNullOrEmpty(expandedUrl))
				res.ExpandedUrl = new Uri(expandedUrl);
			var url = element.Element("url").Cast<string>();
			if (!string.IsNullOrEmpty(url))
				res.Url = new Uri(url);
			return res;
		}

		public override string ToString() { return Url.ToString(); }
	}
}
