using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Saruna
{
	public class Timeline<T> : ITimeline<T> where T : IMessage
	{
		public Timeline(Infrastructures.TwitterRequest request, Infrastructures.TwitterRequestContent content)
		{
			RefreshRequest = request;
			RefreshContent = content;
		}

		ObservableCollection<T> internalCollection = new ObservableCollection<T>();
		List<PositionTracker> trackers = new List<PositionTracker>();

		protected Infrastructures.TwitterRequest RefreshRequest { get; private set; }

		protected Infrastructures.TwitterRequestContent RefreshContent { get; private set; }

		public void AddTracker(PositionTracker tracker) { trackers.Add(tracker); }

		public void RemoveTracker(PositionTracker tracker) { trackers.Remove(tracker); }

		public void InsertTop(T item)
		{
			internalCollection.Insert(0, item);
			foreach (var tracker in trackers)
				tracker.AffectInsertion(0);
		}

		public async Task InsertBetweenAsync(int newerThan, int olderThan, int count)
		{
			RefreshContent.RemoveParameter("count");
			RefreshContent.RemoveParameter("since_id");
			RefreshContent.RemoveParameter("max_id");
			if (newerThan < 0 || newerThan >= Count)
				newerThan = Count;
			if (olderThan < 0 || olderThan >= Count)
				olderThan = -1;
			if (newerThan <= olderThan)
				throw new ArgumentException();
			if (newerThan < Count)
				RefreshContent.SetParameter("since_id", this[newerThan].Id.ToString());
			if (olderThan > -1)
				RefreshContent.SetParameter("max_id", (this[olderThan].Id - 1).ToString());
			if (count > 0)
				RefreshContent.SetParameter("count", count.ToString());
			var items = await GetResultsAsync();
			for (int i = newerThan - 1; i > olderThan; i--)
			{
				internalCollection.RemoveAt(i);
				foreach (var tracker in trackers)
					tracker.AffectRemoval(i);
			}
			for (int i = olderThan + 1; i < items.Count; i++)
			{
				internalCollection.Insert(i, items[i - olderThan - 1]);
				foreach (var tracker in trackers)
					tracker.AffectInsertion(i);
			}
		}

		protected virtual async Task<IReadOnlyList<T>> GetResultsAsync()
		{
			if (typeof(T) == typeof(Tweet))
				return (IReadOnlyList<T>)(await RefreshRequest.GetXmlAsync(RefreshContent)).Elements().Select(x => Tweet.FromXml(x)).ToList();
			else if (typeof(T) == typeof(DirectMessage))
				return (IReadOnlyList<T>)(await RefreshRequest.GetXmlAsync(RefreshContent)).Elements().Select(x => DirectMessage.FromXml(x)).ToList();
			else
				throw new InvalidOperationException();
		}

		public T this[int index] { get { return internalCollection[index]; } }

		public int Count { get { return internalCollection.Count; } }

		public IEnumerator<T> GetEnumerator() { return internalCollection.GetEnumerator(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add { internalCollection.CollectionChanged += value; }
			remove { internalCollection.CollectionChanged -= value; }
		}
	}

	public interface ITimeline<T> : IReadOnlyList<T>, INotifyCollectionChanged
	{
		void InsertTop(T item);

		Task InsertBetweenAsync(int newerThan, int olderThan, int count);
	}

	public class CompositeTimeline<T> : ITimeline<T> where T : IMessage
	{
		public CompositeTimeline(ITimeline<T> timeline1, ITimeline<T> timeline2, Func<T, ITimeline<T>> timelineSelector)
		{
			m_Timeline1 = timeline1;
			m_Timeline2 = timeline2;
			m_Timeline1.CollectionChanged += Timelines_CollectionChanged;
			m_Timeline2.CollectionChanged += Timelines_CollectionChanged;
			m_MasterTimeline = timeline1.Concat(timeline2).OrderByDescending(x => x.CreatedTime);
			m_TimelineSelector = timelineSelector;
		}

		ITimeline<T> m_Timeline1;
		ITimeline<T> m_Timeline2;
		IEnumerable<T> m_MasterTimeline;
		Func<T, ITimeline<T>> m_TimelineSelector;

		public void InsertTop(T item) { m_TimelineSelector(item).InsertTop(item); }

		public async Task InsertBetweenAsync(int newerThan, int olderThan, int count)
		{
			await m_Timeline1.InsertBetweenAsync(newerThan, olderThan, count);
			await m_Timeline2.InsertBetweenAsync(newerThan, olderThan, count);
		}

		public T this[int index] { get { return m_MasterTimeline.ElementAt(index); } }

		public int Count { get { return m_Timeline1.Count + m_Timeline2.Count; } }

		public IEnumerator<T> GetEnumerator() { return m_MasterTimeline.GetEnumerator(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		void Timelines_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); ;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;
	}

	public class PositionTracker
	{
		public PositionTracker(int index) { Value = index; }

		public int Value { get; private set; }

		internal void AffectInsertion(int insertedIndex)
		{
			if (insertedIndex <= Value)
				Value++;
		}

		internal void AffectRemoval(int removedIndex)
		{
			if (removedIndex < Value)
				Value--;
			if (removedIndex == Value)
				Value = -1;
		}
	}
}
