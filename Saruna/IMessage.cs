using System;

namespace Saruna
{
	public interface IMessage
	{
		DateTime CreatedTime { get; }

		TweetEntities Entities { get; }

		long Id { get; }

		string Text { get; }

		User User { get; }
	}
}
