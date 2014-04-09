using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using Livet;
using Livet.Commands;
using RooftopHorizon.Models;
using Saruna;
using Livet.EventListeners;

namespace RooftopHorizon.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		/* コマンド、プロパティの定義にはそれぞれ 
		 * 
		 *  lvcom   : ViewModelCommand
		 *  lvcomn  : ViewModelCommand(CanExecute無)
		 *  llcom   : ListenerCommand(パラメータ有のコマンド)
		 *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
		 *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
		 *  
		 * を使用してください。
		 * 
		 * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
		 * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
		 * LivetCallMethodActionなどから直接メソッドを呼び出してください。
		 * 
		 * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
		 * 同様に直接ViewModelのメソッドを呼び出し可能です。
		 */

		/* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
		 * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
		 */

		/* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
		 * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
		 * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
		 * 
		 * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
		 * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
		 * 
		 * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
		 * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
		 * 
		 * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
		 */

		/* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
		 * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
		 * 
		 * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
		 * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
		 */

		public MainWindowViewModel()
		{
			Tweeting.PropertyChanged += (s, ev) =>
			{
				if (ev.PropertyName == "Text")
					TweetCommand.RaiseCanExecuteChanged();
			};
			CompositeDisposable.Add(new PropertyChangedEventListener(model, (s, ev) =>
			{
				if (ev.PropertyName == "SelectedTimelineIndex")
					RaisePropertyChanged(ev.PropertyName);
				else if (ev.PropertyName == "AuthenticatingUser")
					RaisePropertyChanged(ev.PropertyName);
				else if (ev.PropertyName == "UploadingMedia")
					RaisePropertyChanged(ev.PropertyName);
				else if (ev.PropertyName == "TweetSelectedIndex")
					RaisePropertyChanged(ev.PropertyName);
			}));
			CompositeDisposable.Add(new PropertyChangedEventListener(model.HomeTimeline, (s, ev) =>
			{
				if (ev.PropertyName == "SelectedIndex")
					RaisePropertyChanged("HomeTimelineSelectedIndex");
			}));
			CompositeDisposable.Add(new PropertyChangedEventListener(model.MentionsTimeline, (s, ev) =>
			{
				if (ev.PropertyName == "SelectedIndex")
					RaisePropertyChanged("MentionsTimelineSelectedIndex");
			}));
		}

		Model model = new Model();

		public async void Initialize()
		{
			try
			{
				await model.Initialize();
			}
			catch (System.Net.Http.HttpRequestException)
			{
				Messenger.Raise(new Livet.Messaging.InformationMessage("認証ユーザーの情報を取得できません。", "接続に失敗しました", "ShowErrorMessage"));
			}
			catch (TwitterException)
			{
				Messenger.Raise(new Livet.Messaging.InformationMessage("ログイン認証に失敗しました。", "認証に失敗しました。", "ShowErrorMessage"));
			}
		}

		public User AuthenticatingUser { get { return model.AuthenticatingUser; } }

		public async void GetReply(Tweet tweet) { await tweet.UpdateInReplyToTweetAsync(model.Twitter); }

		public async void Favorite(Tweet tweet)
		{
			try
			{
				await model.Twitter.FavoriteAsync(tweet);
			}
			catch (TwitterException)
			{
				Messenger.Raise(new Livet.Messaging.InformationMessage("ツイートをお気に入りに登録できませんでした。", "お気に入りへの登録", "ShowErrorMessage"));
			}
		}

		public void SetInReplyTo(Tweet tweet) { model.SetInReplyTo(tweet); }

		public async void Retweet(Tweet tweet)
		{
			try
			{
				await model.Twitter.RetweetAsync(tweet);
			}
			catch (TwitterException)
			{
				Messenger.Raise(new Livet.Messaging.InformationMessage("ツイートをリツイートできませんでした。", "リツイート", "ShowErrorMessage"));
			}
		}

		public void Quote(Tweet tweet) { model.Quote(tweet); }

		public Tweet Tweeting { get { return model.Tweeting; } }

		public Timeline<Tweet> HomeTimeline { get { return model.Twitter.HomeTimeline; } }

		public Timeline<Tweet> MentionsTimeline { get { return model.Twitter.MentionsTimeline; } }

		public ITimeline<DirectMessage> DirectMessageTimeline { get { return model.DirectMessageTimeline; } }

		public ObservableCollection<Saruna.Streams.IStreamNotice> StreamNotices { get { return model.StreamNotices; } }

		public int HomeTimelineSelectedIndex
		{
			get { return model.HomeTimeline.SelectedIndex; }
			set { model.HomeTimeline.SelectedIndex = value; }
		}

		public int MentionsTimelineSelectedIndex
		{
			get { return model.MentionsTimeline.SelectedIndex; }
			set { model.MentionsTimeline.SelectedIndex = value; }
		}

		public int SelectedTimelineIndex
		{
			get
			{
				if (model.SelectedTimelineIndex == TimelineType.Mention)
					return 1;
				else
					return 0;
			}
			set
			{
				if (value == 1)
					model.SelectedTimelineIndex = TimelineType.Mention;
				else
					model.SelectedTimelineIndex = TimelineType.Home;
			}
		}

		public int TweetSelectedIndex
		{
			get { return model.TweetSelectedIndex; }
			set { model.TweetSelectedIndex = value; }
		}

		public Uri UploadingMedia { get { return model.UploadingMedia; } }

		#region SignInWithTwitterCommand
		private ViewModelCommand _SignInWithTwitterCommand;

		public ViewModelCommand SignInWithTwitterCommand
		{
			get
			{
				if (_SignInWithTwitterCommand == null)
					_SignInWithTwitterCommand = new ViewModelCommand(SignInWithTwitter);
				return _SignInWithTwitterCommand;
			}
		}

		public async void SignInWithTwitter() { await model.SignInWithTwitter(); }
		#endregion

		#region RefreshHomeTimelineCommand
		private ViewModelCommand _RefreshHomeTimelineCommand;

		public ViewModelCommand RefreshHomeTimelineCommand
		{
			get
			{
				if (_RefreshHomeTimelineCommand == null)
					_RefreshHomeTimelineCommand = new ViewModelCommand(RefreshHomeTimeline);
				return _RefreshHomeTimelineCommand;
			}
		}

		public async void RefreshHomeTimeline()
		{
			try
			{
				await HomeTimeline.InsertBetweenAsync(0, -1, -1);
			}
			catch (TwitterException ex)
			{
				var desc = ex.GetDescriptions().First();
				Messenger.Raise(new Livet.Messaging.InformationMessage(desc.Code + ": " + desc.Message, "タイムラインの読み込みに失敗しました。", "ShowErrorMessage"));
			}
		}
		#endregion

		#region RefreshMentionsTimelineCommand
		private ViewModelCommand _RefreshMentionsTimelineCommand;

		public ViewModelCommand RefreshMentionsTimelineCommand
		{
			get
			{
				if (_RefreshMentionsTimelineCommand == null)
					_RefreshMentionsTimelineCommand = new ViewModelCommand(RefreshMentionsTimeline);
				return _RefreshMentionsTimelineCommand;
			}
		}

		public async void RefreshMentionsTimeline()
		{
			try
			{
				await MentionsTimeline.InsertBetweenAsync(0, -1, -1);
			}
			catch (TwitterException ex)
			{
				var desc = ex.GetDescriptions().First();
				Messenger.Raise(new Livet.Messaging.InformationMessage(desc.Code + ": " + desc.Message, "タイムラインの読み込みに失敗しました。", "ShowErrorMessage"));
			}
		}
		#endregion

		#region TweetCommand
		private ViewModelCommand _TweetCommand;

		public ViewModelCommand TweetCommand
		{
			get
			{
				if (_TweetCommand == null)
					_TweetCommand = new ViewModelCommand(Tweet, () => !string.IsNullOrEmpty(Tweeting.Text));
				return _TweetCommand;
			}
		}

		public async void Tweet()
		{
			try
			{
				await model.Tweet();
			}
			catch (TwitterException ex)
			{
				var desc = ex.GetDescriptions().First();
				Messenger.Raise(new Livet.Messaging.InformationMessage(desc.Code + ": " + desc.Message, "ツイートの送信に失敗しました", "ShowErrorMessage"));
			}
		}

		#endregion

		#region RefreshDirectMessageTimelineCommand
		private ViewModelCommand _RefreshDirectMessageTimelineCommand;

		public ViewModelCommand RefreshDirectMessageTimelineCommand
		{
			get
			{
				if (_RefreshDirectMessageTimelineCommand == null)
					_RefreshDirectMessageTimelineCommand = new ViewModelCommand(RefreshDirectMessageTimeline);
				return _RefreshDirectMessageTimelineCommand;
			}
		}

		public async void RefreshDirectMessageTimeline()
		{
			try
			{
				await DirectMessageTimeline.InsertBetweenAsync(0, -1, -1);
			}
			catch (TwitterException ex)
			{
				var desc = ex.GetDescriptions().First();
				Messenger.Raise(new Livet.Messaging.InformationMessage(desc.Code + ": " + desc.Message, "タイムラインの読み込みに失敗しました。", "ShowErrorMessage"));
			}
		}
		#endregion

		#region SelectUploadingMediaCommand

		private ViewModelCommand _SelectUploadingMediaCommand;

		public ViewModelCommand SelectUploadingMediaCommand
		{
			get
			{
				if (_SelectUploadingMediaCommand == null)
					_SelectUploadingMediaCommand = new ViewModelCommand(model.SelectUploadingMedia);
				return _SelectUploadingMediaCommand;
			}
		}

		#endregion

		public bool IsUserStreamConnected
		{
			get { return model.Twitter.UserStream.IsConnected; }
			set
			{
				if (value != model.Twitter.UserStream.IsConnected)
				{
					if (value)
						model.Twitter.UserStream.ConnectAsync();
					else
						model.Twitter.UserStream.Discoonect();
				}
			}
		}
	}
}
