using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saruna
{
	public struct Color : IEquatable<Color>
	{
		public Color(byte alpha, byte red, byte green, byte blue) : this()
		{
			Alpha = alpha;
			Red = red;
			Green = green;
			Blue = blue;
		}

		public Color(int argb) : this((byte)((argb >> 24) & 0xFF), (byte)((argb >> 16) & 0xFF), (byte)((argb >> 8) & 0xFF), (byte)(argb & 0xFF)) { }

		public Color(byte alpha, Color baseColor) : this(alpha, baseColor.Red, baseColor.Green, baseColor.Blue) { }

		public static Color Empty = new Color();

		public bool IsEmpty { get { return ToArgb() == 0; } }

		public byte Alpha { get; private set; }

		public byte Red { get; private set; }

		public byte Green { get; private set; }

		public byte Blue { get; private set; }

		public int ToArgb() { return (Alpha << 24) & (Red << 16) & (Green << 8) & Blue; }

		public static bool operator ==(Color left, Color right) { return left.Equals(right); }

		public static bool operator !=(Color left, Color right) { return !(left == right); }

		public bool Equals(Color other) { return ToArgb() == other.ToArgb(); }

		public override bool Equals(object obj)
		{
			if (obj is Color)
				return Equals((Color)obj);
			return false;
		}

		public override int GetHashCode() { return ToArgb().GetHashCode(); }
	}
}
