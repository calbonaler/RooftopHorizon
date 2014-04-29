using System;
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
				Timeline = new TimelineWithSelectedIndex(model.Twitter.GetUserTimeline(Identifiers.CreateUser(parameter.OwnerScreenName)));
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
			SubCommands = parameter.SubCommands.ToArray();
		}

		public TimelineWithSelectedIndex Timeline { get; private set; }

		public PositionTracker Position { get; protected set; }

		public TimelineType DestinationTimeline { get; private set; }

		public int Count { get; private set; }

		public Func<Tweet, bool> Predicate { get; private set; }

		public IReadOnlyList<Func<Tweet, Task>> SubCommands { get; private set; }
		
		public virtual void StartTracking() { if (Position != null) Timeline.Timeline.AddTracker(Position); }

		public virtual void EndTracking() { if (Position != null) Timeline.Timeline.RemoveTracker(Position); }

		public async Task PerformAsync(Model model)
		{
			model.SelectedTimelineIndex = DestinationTimeline;
			int count = Count;
			int offset = 0;
			while (count > 0 && Position.Value + offset < Timeline.Timeline.Count)
			{
				var tweet = Timeline.Timeline[Position.Value + offset];
				if (tweet.RetweetSource != null)
					tweet = tweet.RetweetSource;
				if (Predicate(tweet))
				{
					Timeline.SelectedIndex = Position.Value + offset;
					foreach (var command in SubCommands)
					{
						try { await command(tweet); }
						catch (TwitterException) { }
					}
				}
				offset++;
				count--;
			}
			try
			{
				while (count > 0)
				{
					await Timeline.Timeline.InsertBetweenAsync(-1, Timeline.Timeline.Count - 1, Math.Min(count, 200));
					while (count > 0 && Position.Value + offset < Timeline.Timeline.Count)
					{
						var tweet = Timeline.Timeline[Position.Value + offset];
						if (tweet.RetweetSource != null)
							tweet = tweet.RetweetSource;
						if (Predicate(tweet))
						{
							Timeline.SelectedIndex = Position.Value + offset;
							foreach (var command in SubCommands)
							{
								try { await command(tweet); }
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

	public class CommandParameter
	{
		IList<Func<Tweet, Task>> m_SubCommands = new List<Func<Tweet, Task>>();

		public int? Position { get; set; }

		public bool Relative { get; set; }

		public TimelineType Timeline { get; set; }

		public int Count { get; set; }

		public Func<Tweet, bool> Predicate { get; set; }

		public string OwnerScreenName { get; set; }

		public IList<Func<Tweet, Task>> SubCommands { get { return m_SubCommands; } }
	}
}
