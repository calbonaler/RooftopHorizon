using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Saruna;

namespace RooftopHorizon.Views
{
	public class TweetTextBlock : TextBlock
	{
		public static readonly DependencyProperty TweetProperty = DependencyProperty.Register("Tweet", typeof(Tweet), typeof(TweetTextBlock), new PropertyMetadata(OnTweetPropertyChanged));

		public static Tweet GetTweet(DependencyObject element) { return element.GetValue(TweetProperty) as Tweet; }

		public static void SetTweet(DependencyObject element, Tweet value) { element.SetValue(TweetProperty, value); }

		public Tweet Tweet
		{
			get { return GetTweet(this); }
			set { SetTweet(this, value); }
		}

		static void OnTweetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var owner = d as TweetTextBlock;
			var tweet = e.NewValue as Tweet;
			if (owner == null || tweet == null)
				return;
			List<Uri> medias = new List<Uri>();
			owner.Inlines.Clear();
			int start = 0;
			foreach (var entity in tweet.Entities.All.OrderBy(x => x.Start))
			{
				if (entity.Start > start)
					owner.Inlines.Add(tweet.Text.Substring(start, entity.Start - start));
				var link = new NavigableHyperlink();
				var navigable = entity as INavigableTweetEntity;
				if (navigable != null)
				{
					link.Inlines.Add(navigable.DisplayUrl.ToString());
					link.NavigateUri = navigable.Url;
					// p.twipple.jp
					if (navigable.ExpandedUrl.Host.Equals("p.twipple.jp", StringComparison.Ordinal))
						medias.Add(new Uri("http://p.twpl.jp/show/orig" + navigable.ExpandedUrl.AbsolutePath));
					// pic.twitter.com
					var media = navigable as TweetMedia;
					if (media != null)
						medias.Add(media.SecureMediaUrl);
				}
				else
				{
					TweetUserMention mention;
					TweetHashtag hashtag;
					if ((mention = entity as TweetUserMention) != null)
						link.NavigateUri = new Uri("https://twitter.com/" + mention.ScreenName);
					else if ((hashtag = entity as TweetHashtag) != null)
						link.NavigateUri = new Uri("https://twitter.com/search?q=" + Saruna.Infrastructures.PercentEncoding.Encode("#" + hashtag.Text));
					try { link.Inlines.Add(tweet.Text.Substring(entity.Start, entity.End - entity.Start)); }
					catch (ArgumentOutOfRangeException) { }
				}
				owner.Inlines.Add(link);
				start = entity.End;
			}
			if (tweet.Text.Length > start)
				owner.Inlines.Add(tweet.Text.Substring(start));
			if (medias.Count > 0)
				owner.Inlines.Add(Environment.NewLine);
			foreach (var media in medias)
			{
				var image = new Image();
				image.Source = new System.Windows.Media.Imaging.BitmapImage(media);
				image.StretchDirection = StretchDirection.DownOnly;
				image.MaxWidth = 400;
				image.MaxHeight = 200;
				image.UseLayoutRounding = image.SnapsToDevicePixels = true;
				System.Windows.Media.RenderOptions.SetBitmapScalingMode(image, System.Windows.Media.BitmapScalingMode.HighQuality);
				owner.Inlines.Add(image);
			}
		}
	}
}
