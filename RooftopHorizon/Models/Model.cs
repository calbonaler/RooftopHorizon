﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Livet;
using Saruna;

namespace RooftopHorizon.Models
{
	public class Model : NotificationObject
	{
		public Model()
		{
			Twitter = new Twitter();
			var root = System.Xml.Linq.XDocument.Load("DefaultKeys.xml").Root;
			// 下記はコンシューマーキーです。変更不要
			Twitter.ConsumerKey = new TokenSecretPair((string)root.Element("ConsumerKey"), (string)root.Element("ConsumerSecret"));
			// 下記はアクセストークンです。異なるユーザーをデフォルトで使用する場合は変更してください。
			Twitter.AccessToken = new TokenSecretPair((string)root.Element("AccessToken"), (string)root.Element("AccessTokenSecret"));
			Twitter.UserStream.MessageReceived += UserStream_MessageReceived;
			StreamNotices = new ObservableCollection<Saruna.Streams.IStreamNotice>();
			DirectMessageTimeline = new CompositeTimeline<DirectMessage>(
				Twitter.SentDirectMessageTimeline,
				Twitter.ReceivedDirectMessageTimeline,
				t => t.Sender.ScreenName == AuthenticatingUser.ScreenName ? Twitter.SentDirectMessageTimeline : Twitter.ReceivedDirectMessageTimeline
			);
			HomeTimeline = new TimelineWithSelectedIndex(Twitter.HomeTimeline);
			MentionsTimeline = new TimelineWithSelectedIndex(Twitter.MentionsTimeline);
			Tweeting = new Tweet();
		}

		TwitterSignInServer signinServer;

		public async Task Initialize() { AuthenticatingUser = await Twitter.VerifyCredentialsAsync(); }

		#region AuthenticatingUser変更通知プロパティ
		private User _AuthenticatingUser;

		public User AuthenticatingUser
		{
			get { return _AuthenticatingUser; }
			private set
			{
				if (_AuthenticatingUser == value)
					return;
				_AuthenticatingUser = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		public ObservableCollection<Saruna.Streams.IStreamNotice> StreamNotices { get; private set; }

		public ITimeline<DirectMessage> DirectMessageTimeline { get; private set; }

		void UserStream_MessageReceived(object sender, Saruna.Streams.MessageReceivedEventArgs e)
		{
			var tweet = e.Value as Tweet;
			if (tweet != null)
			{
				if (tweet.Entities.UserMentions.Any(mention => mention.ScreenName == AuthenticatingUser.ScreenName))
					Twitter.MentionsTimeline.InsertTop(tweet);
				Twitter.HomeTimeline.InsertTop(tweet);
			}
			else
			{
				var directMessage = e.Value as DirectMessage;
				if (directMessage != null)
					DirectMessageTimeline.InsertTop(directMessage);
				else
					StreamNotices.Insert(0, (Saruna.Streams.IStreamNotice)e.Value);
			}
		}

		public Tweet Tweeting { get; private set; }

		public Twitter Twitter { get; private set; }

		public TimelineWithSelectedIndex HomeTimeline { get; private set; }

		public TimelineWithSelectedIndex MentionsTimeline { get; private set; }

		#region SelectedTimelineIndex変更通知プロパティ
		private TimelineType _SelectedTimelineIndex = TimelineType.Home;

		public TimelineType SelectedTimelineIndex
		{
			get { return _SelectedTimelineIndex; }
			set
			{ 
				if (_SelectedTimelineIndex == value)
					return;
				_SelectedTimelineIndex = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region UploadingMedia変更通知プロパティ
		private Uri _UploadingMedia = null;

		public Uri UploadingMedia
		{
			get { return _UploadingMedia; }
			private set
			{
				if (_UploadingMedia == value)
					return;
				_UploadingMedia = value;
				RaisePropertyChanged();
			}
		}
		#endregion

		#region TweetSelectedIndex
		int _TweetSelectedIndex;

		public int TweetSelectedIndex
		{
			get { return _TweetSelectedIndex; }
			set
			{
				if (_TweetSelectedIndex != value)
				{
					_TweetSelectedIndex = value;
					RaisePropertyChanged();
				}
			}
		}
		#endregion

		public void SelectUploadingMedia()
		{
			Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
			dialog.CheckFileExists = true;
			dialog.CheckPathExists = true;
			dialog.DereferenceLinks = true;
			dialog.Filter = "画像ファイル|*.bmp;*.dib;*.jpg;*.jpe;*.jpeg;*.jfif;*.png;*.tif;*.tiff";
			if (dialog.ShowDialog() == true)
				UploadingMedia = new Uri(dialog.FileName);
		}

		public void ClearUploadingMedia() { UploadingMedia = null; }

		public void SetInReplyTo(Tweet tweet)
		{
			Tweeting.InReplyToTweetId = tweet;
			Tweeting.InReplyToUserId = tweet.User;
			Tweeting.Text = "@" + tweet.User.ScreenName + " ";
			TweetSelectedIndex = Tweeting.Text.Length - 1;
		}

		public void Quote(Tweet tweet)
		{
			if (tweet.RetweetSource != null)
				tweet = tweet.RetweetSource;
			Tweeting.InReplyToTweetId = tweet;
			Tweeting.InReplyToUserId = tweet.User;
			Tweeting.Text = "RT @" + tweet.User.ScreenName + ": " + tweet.Text;
		}

		public async Task SignInWithTwitter()
		{
			await Twitter.ApplyRequestTokenAsync("http://localhost:8000/rooftop-horizon/sign-in");
			if (signinServer == null)
			{
				signinServer = new TwitterSignInServer();
				signinServer.SignInRequested += async (s, ev) =>
				{
					await Twitter.ApplyAccessTokenAsync(ev.Verifier);
					AuthenticatingUser = await Twitter.VerifyCredentialsAsync();
					System.Windows.Input.CommandManager.InvalidateRequerySuggested();
				};
				signinServer.Start(
					"http://localhost:8000/rooftop-horizon/",
					"<html><head><title>Rooftop Horizon</title></head><body><h2>認証が完了しました</h2>このページを閉じて引き続きRooftopHorizonをお楽しみください。</body></html>",
					"<html><head><title>Rooftop Horizon</title></head><body><h2>認証できませんでした</h2>認証をやり直すにはRooftopHorizonを再起動してください。</body></html>"
				);
			}
			System.Diagnostics.Process.Start(Twitter.CreateSignInAuthorizationUrl(Twitter.AccessToken));
		}

		public async Task RunCommand(string commandLine)
		{
			using (StringReader reader = new StringReader(commandLine))
			using (Parser parser = new Parser(reader, this))
			{
				var commands = parser.Parse();
				foreach (var item in commands)
					item.StartTracking();
				foreach (var item in commands)
				{
					await item.PerformAsync(this);
					item.EndTracking();
				}
			}
		}

		public async Task Tweet()
		{
			if (Tweeting.Text.StartsWith("@!") && Tweeting.InReplyToTweetId == null)
			{
				try
				{
					var temp = Tweeting.Text;
					await RunCommand(Tweeting.Text.Remove(0, 2));
					if (temp == Tweeting.Text)
						Tweeting.Text = string.Empty;
					return;
				}
				catch (ArgumentException) { }
			}
			if (UploadingMedia != null)
				await Twitter.TweetAsync(Tweeting, Enumerable.Repeat(File.ReadAllBytes(UploadingMedia.LocalPath), 1));
			else
				await Twitter.TweetAsync(Tweeting);
			Tweeting.Text = string.Empty;
			Tweeting.InReplyToTweetId = null;
			Tweeting.InReplyToUserId = null;
			ClearUploadingMedia();
		}
	}

	public class TimelineWithSelectedIndex : NotificationObject
	{
		public TimelineWithSelectedIndex(Timeline<Tweet> timeline) { Timeline = timeline; }

		public Timeline<Tweet> Timeline { get; private set; }

		#region SelectedIndex変更通知プロパティ
		private int _SelectedIndex;

		public int SelectedIndex
		{
			get { return _SelectedIndex; }
			set
			{ 
				if (_SelectedIndex == value)
					return;
				_SelectedIndex = value;
				RaisePropertyChanged();
			}
		}
		#endregion
	}

	public enum TimelineType
	{
		Default,
		Home,
		Mention,
		User,
	}
}
