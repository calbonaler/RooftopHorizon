using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saruna;

namespace RooftopHorizon.Models
{
	public class Conversation
	{
		public Conversation(Tweet content) { Content = content; }

		List<Conversation> successors = new List<Conversation>();

		public Tweet Content { get; private set; }

		public Conversation Predecessor { get; private set; }

		public IReadOnlyList<Conversation> Successors { get { return successors; } }

		public void AddSuccessor(Conversation item)
		{
			if (item == null)
				throw new ArgumentNullException();
			if (item.Predecessor != null)
				throw new ArgumentException();
			successors.Add(item);
			item.Predecessor = this;
		}
	}
}
