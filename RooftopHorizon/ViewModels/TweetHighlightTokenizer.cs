using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Controls;

namespace RooftopHorizon.ViewModels
{
	public class TweetHighlightTokenizer : CompositeHighlightTokenizer
	{
		public override IEnumerable<HighlightToken> GetTokens(string text)
		{
			if (text.StartsWith("@!"))
				return base.GetTokens(text);
			return Enumerable.Empty<HighlightToken>();
		}
	}
}
