using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Saruna
{
	public class CursorNavigable<T> : IEnumerable<T>
	{
		public static CursorNavigable<T> FromXml(XElement element, string memberName, Func<XElement, T> selector)
		{
			if (Utils.IsJsonNull(element))
				return null;
			CursorNavigable<T> res = new CursorNavigable<T>();
			res.PreviousCursor = new Cursor(element.Element("previous_cursor").Cast<long>());
			res.NextCursor = new Cursor(element.Element("next_cursor").Cast<long>());
			res.Items = element.Element(memberName).Elements().Select(selector).ToArray();
			return res;
		}

		public Cursor PreviousCursor { get; private set; }

		public Cursor NextCursor { get; private set; }

		public IReadOnlyList<T> Items { get; private set; }

		public IEnumerator<T> GetEnumerator() { return Items.GetEnumerator(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public struct Cursor : IEquatable<Cursor>
	{
		internal Cursor(long value) : this() { Value = value; }

		public static readonly Cursor Last = new Cursor();
		public static readonly Cursor First = new Cursor(-1);

		internal long Value { get; private set; }

		public bool IsLastPage { get { return Value == 0; } }

		public bool Equals(Cursor other) { return Value == other.Value; }

		public override bool Equals(object obj)
		{
			if (obj is Cursor)
				return Equals((Cursor)obj);
			return false;
		}

		public override int GetHashCode() { return Value.GetHashCode(); }

		public static bool operator ==(Cursor left, Cursor right) { return left.Equals(right); }

		public static bool operator !=(Cursor left, Cursor right) { return !(left == right); }
	}
}
