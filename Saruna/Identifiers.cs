using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saruna
{
	public static class Identifiers
	{
		internal static ITweetIdentifier CreateTweet(long? id) { return id == null ? null : new TweetIdentifier((long)id); }
		internal static IDirectMessageIdentifier CreateDirectMessage(long? id) { return id == null ? null : new DirectMessageIdentifier((long)id); }
		internal static IUserIdentifier CreateUser(string screenName, long? id)
		{
			if (string.IsNullOrEmpty(screenName) || id == null)
				return null;
			else
				return new UserIdentifier(screenName, (long)id);
		}
		internal static IUserIdentifier CreateUser(long? id) { return id == null ? null : new UserIdentifier((long)id); }
		public static IUserIdentifier CreateUser(string screenName) { return string.IsNullOrEmpty(screenName) ? null : new UserIdentifier(screenName); }
		public static IListIdentifier CreateList(IUserIdentifier user, string slug)
		{
			if (user == null || string.IsNullOrEmpty(slug))
				return null;
			else
				return new ListIdentifier(user, slug);
		}

		class TweetIdentifier : ITweetIdentifier
		{
			public TweetIdentifier(long id) { Id = id; }
			public long Id { get; private set; }
		}

		class DirectMessageIdentifier : IDirectMessageIdentifier
		{
			public DirectMessageIdentifier(long id) { Id = id; }
			public long Id { get; private set; }
		}

		class UserIdentifier : IUserIdentifier
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

		class ListIdentifier : IListIdentifier
		{
			public ListIdentifier(IUserIdentifier user, string slug)
			{
				User = user;
				Slug = slug;
			}
			public IUserIdentifier User { get; private set; }
			public string Slug { get; private set; }
			public long Id { get { return 0; } }
			public bool HasId { get { return false; } }
			public bool HasSlugAndUser { get { return true; } }
		}
	}

	public interface ITweetIdentifier
	{
		long Id { get; }
	}

	public interface IDirectMessageIdentifier
	{
		long Id { get; }
	}

	public interface IUserIdentifier
	{
		string ScreenName { get; }
		long Id { get; }
		bool HasId { get; }
		bool HasScreenName { get; }
	}

	public interface IListIdentifier
	{
		string Slug { get; }
		IUserIdentifier User { get; }
		long Id { get; }
		bool HasId { get; }
		bool HasSlugAndUser { get; }
	}

	public interface ISavedSearchIdentifier
	{
		long Id { get; }
	}

	public interface IPlaceIdentifier
	{
		string Id { get; }
	}
}
