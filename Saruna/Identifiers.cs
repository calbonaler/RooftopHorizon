using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saruna
{
	public static class Identifiers
	{
		internal static ITweetIdentifiable CreateTweet(long? id) { return id == null ? null : new TweetIdentifier((long)id); }
		internal static IDirectMessageIdentifiable CreateDirectMessage(long? id) { return id == null ? null : new DirectMessageIdentifier((long)id); }
		internal static IUserIdentifiable CreateUser(string screenName, long? id)
		{
			if (string.IsNullOrEmpty(screenName) || id == null)
				return null;
			else
				return new UserIdentifier(screenName, (long)id);
		}
		internal static IUserIdentifiable CreateUser(long? id) { return id == null ? null : new UserIdentifier((long)id); }
		public static IUserIdentifiable CreateUser(string screenName) { return string.IsNullOrEmpty(screenName) ? null : new UserIdentifier(screenName); }
		public static IListIdentifiable CreateList(IUserIdentifiable user, string slug)
		{
			if (user == null || string.IsNullOrEmpty(slug))
				return null;
			else
				return new ListIdentifier(user, slug);
		}

		class TweetIdentifier : ITweetIdentifiable
		{
			public TweetIdentifier(long id) { Id = id; }
			public long Id { get; private set; }
		}

		class DirectMessageIdentifier : IDirectMessageIdentifiable
		{
			public DirectMessageIdentifier(long id) { Id = id; }
			public long Id { get; private set; }
		}

		class UserIdentifier : IUserIdentifiable
		{
			public UserIdentifier(string screenName)
			{
				ScreenName = screenName;
				HasScreenName = true;
			}
			public UserIdentifier(long id)
			{
				Id = id;
				HasId = true;
			}
			public UserIdentifier(string screenName, long id)
			{
				Id = id;
				ScreenName = screenName;
				HasId = HasScreenName = true;
			}
			public string ScreenName { get; private set; }
			public long Id { get; private set; }
			public bool HasId { get; private set; }
			public bool HasScreenName { get; private set; }
		}

		class ListIdentifier : IListIdentifiable
		{
			public ListIdentifier(IUserIdentifiable user, string slug)
			{
				User = user;
				Slug = slug;
			}
			public IUserIdentifiable User { get; private set; }
			public string Slug { get; private set; }
			public long Id { get { return 0; } }
			public bool HasId { get { return false; } }
			public bool HasSlugAndUser { get { return true; } }
		}
	}

	public interface ITweetIdentifiable
	{
		long Id { get; }
	}

	public interface IDirectMessageIdentifiable
	{
		long Id { get; }
	}

	public interface IUserIdentifiable
	{
		string ScreenName { get; }
		long Id { get; }
		bool HasId { get; }
		bool HasScreenName { get; }
	}

	public interface IListIdentifiable
	{
		string Slug { get; }
		IUserIdentifiable User { get; }
		long Id { get; }
		bool HasId { get; }
		bool HasSlugAndUser { get; }
	}

	public interface ISavedSearchIdentifiable
	{
		long Id { get; }
	}

	public interface IPlaceIdentifiable
	{
		string Id { get; }
	}
}
