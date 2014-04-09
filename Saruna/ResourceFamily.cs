using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saruna
{
	[Flags]
	public enum ResourceFamilies
	{
		None			= 0x0000,
		Account			= 0x0001,
		Application		= 0x0002,
		Blocks			= 0x0004,
		DirectMessages	= 0x0008,
		Favorites		= 0x0010,
		Followers		= 0x0020,
		Friends			= 0x0040,
		Friendships		= 0x0080,
		Geo				= 0x0100,
		Help			= 0x0200,
		Lists			= 0x0400,
		SavedSearches	= 0x0800,
		Search			= 0x1000,
		Statuses		= 0x2000,
		Trends			= 0x4000,
		Users			= 0x8000,
	}
}
