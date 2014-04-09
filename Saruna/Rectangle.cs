using System;

namespace Saruna
{
	public struct Rectangle : IEquatable<Rectangle>
	{
		public Rectangle(int x, int y, int width, int height) : this()
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public static Rectangle Empty = new Rectangle();

		public int X { get; private set; }

		public int Y { get; private set; }

		public int Width { get; private set; }

		public int Height { get; private set; }

		public bool IsEmpty { get { return X == 0 && Y == 0 && Width == 0 && Height == 0; } }

		public static bool operator ==(Rectangle left, Rectangle right) { return left.Equals(right); }

		public static bool operator !=(Rectangle left, Rectangle right) { return !(left == right); }

		public bool Equals(Rectangle other) { return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height; }

		public override bool Equals(object obj)
		{
			if (obj is Rectangle)
				return Equals((Rectangle)obj);
			return false;
		}

		public override int GetHashCode() { return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode(); }
	}
}
