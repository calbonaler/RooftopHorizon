using System.Text;

namespace Saruna.Infrastructures
{
	public class PercentEncoding
	{
		public static string Encode(string source)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var b in Encoding.UTF8.GetBytes(source))
			{
				if (b >= '0' && b <= '9' || b >= 'A' && b <= 'Z' ||
					b >= 'a' && b <= 'z' || b == '-' || b == '.' ||
					b == '_' || b == '~')
					sb.Append((char)b);
				else
					sb.AppendFormat("%{0:X2}", b);
			}
			return sb.ToString();
		}
	}
}
