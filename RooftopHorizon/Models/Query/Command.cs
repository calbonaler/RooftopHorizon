﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saruna;

namespace RooftopHorizon.Query
{
	public class Command
	{
		public Command(CommandParameter parameter, Model model)
		{
			if (parameter.Timeline == TimelineType.Default)
				parameter.Timeline = model.SelectedTimelineIndex;
			if (parameter.Timeline == TimelineType.Home)
				Timeline = model.HomeTimeline;
			else if (parameter.Timeline == TimelineType.Mention)
				Timeline = model.MentionsTimeline;
			else if (parameter.Timeline == TimelineType.User)
				Timeline = new TimelineWithSelectedIndex(Identifiers.CreateUser(parameter.OwnerScreenName).GetTimeline(model.Twitter));
			if (parameter.Position != null)
			{
				if (parameter.Relative)
					Position = new PositionTracker(Timeline.SelectedIndex + (int)parameter.Position);
				else
					Position = new PositionTracker((int)parameter.Position);
			}
			else
				Position = new PositionTracker(0);
			DestinationTimeline = parameter.Timeline;
			Count = parameter.Count;
			Predicate = parameter.Predicate;
			Executors = parameter.Executors.ToArray();
		}

		public TimelineWithSelectedIndex Timeline { get; private set; }

		public PositionTracker Position { get; protected set; }

		public TimelineType DestinationTimeline { get; private set; }

		public int? Count { get; private set; }

		public Func<Tweet, bool> Predicate { get; private set; }

		public IReadOnlyList<Func<Tweet, IAccount, Task>> Executors { get; private set; }
		
		public virtual void StartTracking() { if (Position != null) Timeline.Timeline.AddTracker(Position); }

		public virtual void EndTracking() { if (Position != null) Timeline.Timeline.RemoveTracker(Position); }

		public async Task PerformAsync(Model model)
		{
			model.SelectedTimelineIndex = DestinationTimeline;
			int? count = Count;
			int offset = 0;
			while ((count == null || count > 0) && Position.Value + offset < Timeline.Timeline.Count)
			{
				var tweet = Timeline.Timeline[Position.Value + offset];
				if (tweet.RetweetSource != null)
					tweet = tweet.RetweetSource;
				if (Predicate(tweet))
				{
					Timeline.SelectedIndex = Position.Value + offset;
					foreach (var command in Executors)
					{
						try { await command(tweet, model.Twitter); }
						catch (TwitterException) { }
					}
				}
				offset++;
				if (count != null)
					count--;
			}
			if (count != null)
			{
				try
				{
					while (count > 0)
					{
						await Timeline.Timeline.InsertBetweenAsync(-1, Timeline.Timeline.Count - 1, Math.Min((int)count, 200));
						while (count > 0 && Position.Value + offset < Timeline.Timeline.Count)
						{
							var tweet = Timeline.Timeline[Position.Value + offset];
							if (tweet.RetweetSource != null)
								tweet = tweet.RetweetSource;
							if (Predicate(tweet))
							{
								Timeline.SelectedIndex = Position.Value + offset;
								foreach (var command in Executors)
								{
									try { await command(tweet, model.Twitter); }
									catch (TwitterException) { }
								}
							}
							offset++;
							count--;
						}
					}
				}
				catch (TwitterException) { }
			}
		}
	}

	public class CommandParameter
	{
		IList<Func<Tweet, IAccount, Task>> m_Executors = new List<Func<Tweet, IAccount, Task>>();

		public int? Position { get; set; }

		public bool Relative { get; set; }

		public TimelineType Timeline { get; set; }

		public int? Count { get; set; }

		public Func<Tweet, bool> Predicate { get; set; }

		public string OwnerScreenName { get; set; }

		public IList<Func<Tweet, IAccount, Task>> Executors { get { return m_Executors; } }
	}
}
