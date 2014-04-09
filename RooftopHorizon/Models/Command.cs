using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saruna;

namespace RooftopHorizon.Models
{
	public abstract class Command
	{
		protected Command(CommandParameter parameter, Model model)
		{
			if (parameter.Timeline == TimelineType.Default)
				parameter.Timeline = model.SelectedTimelineIndex;
			if (parameter.Timeline == TimelineType.Home)
				Timeline = model.HomeTimeline;
			else if (parameter.Timeline == TimelineType.Mention)
				Timeline = model.MentionsTimeline;
			else if (parameter.Timeline == TimelineType.User)
				Timeline = new TimelineWithSelectedIndex(model.Twitter.GetUserTimeline(Identifiers.CreateUser(parameter.OwnerScreenName)));
		}

		public TimelineWithSelectedIndex Timeline { get; private set; }

		public PositionTracker Position { get; protected set; }
		
		public virtual void StartTracking() { if (Position != null) Timeline.Timeline.AddTracker(Position); }

		public virtual void EndTracking() { if (Position != null) Timeline.Timeline.RemoveTracker(Position); }

		public abstract Task PerformAsync(Model model);
	}

	public class SelectCommand : Command
	{
		public SelectCommand(CommandParameter parameter, Model model) : base(parameter, model)
		{
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

		public TimelineType DestinationTimeline { get; private set; }

		public int Count { get; private set; }

		public Func<Tweet, bool> Predicate { get; private set; }

		public IReadOnlyList<Command> SubCommands { get; private set; }

		public override void StartTracking()
		{
			base.StartTracking();
			foreach (var command in SubCommands)
				command.StartTracking();
		}

		public override void EndTracking()
		{
			base.EndTracking();
			foreach (var command in SubCommands)
				command.EndTracking();
		}

		public override async Task PerformAsync(Model model)
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
						await command.PerformAsync(model);
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
						if (Predicate(Timeline.Timeline[Position.Value + offset]))
						{
							Timeline.SelectedIndex = Position.Value + offset;
							foreach (var command in SubCommands)
								await command.PerformAsync(model);
						}
						offset++;
						count--;
					}
				}
			}
			catch (TwitterException) { }
		}
	}

	public class TweetRelatedCommand : Command
	{
		public TweetRelatedCommand(CommandParameter parameter, Model model, Func<Tweet, Task> process) : base(parameter, model)
		{
			if (parameter.Position != null)
			{
				if (parameter.Relative)
				{
					offset = (int)parameter.Position;
					Position = null;
				}
				else
					Position = new PositionTracker((int)parameter.Position);
			}
			else
				Position = null;
			m_Process = process;
		}

		Func<Tweet, Task> m_Process;
		int offset = 0;

		public override async Task PerformAsync(Model model)
		{
			try
			{
				if (Position != null)
					await m_Process(Timeline.Timeline[Position.Value]);
				else
					await m_Process(Timeline.Timeline[Timeline.SelectedIndex + offset]);
			}
			catch (TwitterException) { }
		}
	}

	public class CommandParameter
	{
		IList<Command> m_SubCommands = new List<Command>();

		public int? Position { get; set; }

		public bool Relative { get; set; }

		public TimelineType Timeline { get; set; }

		public int Count { get; set; }

		public Func<Tweet, bool> Predicate { get; set; }

		public string OwnerScreenName { get; set; }

		public IList<Command> SubCommands { get { return m_SubCommands; } }
	}
}
