using System;

namespace Saruna
{
	public class RateLimitEntry
	{
		public RateLimitEntry(string resource, int remaining, int limit, long reset)
		{
			Resource = resource;
			Remaining = remaining;
			Limit = limit;
			Reset = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reset).ToLocalTime();
		}

		public string Resource { get; private set; }

		public int Remaining { get; private set; }

		public DateTime Reset { get; private set; }

		public int Limit { get; private set; }
	}
}
