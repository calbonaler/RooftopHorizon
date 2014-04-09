using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saruna
{
	public class SearchTimeline : Timeline<Tweet>
	{
		public SearchTimeline(Infrastructures.TwitterRequest request, Infrastructures.TwitterRequestContent content) : base(request, content) { }

		SearchMetadata data = null;
		public string Query { get { return RefreshContent.GetParameter("q"); } }

		public async new Task<SearchMetadata> InsertBetweenAsync(int newerThan, int olderThan, int count)
		{
			await base.InsertBetweenAsync(newerThan, olderThan, count);
			return data;
		}

		protected override async Task<IReadOnlyList<Tweet>> GetResultsAsync()
		{
			var result = await RefreshRequest.GetXmlAsync(RefreshContent);
			data = SearchMetadata.FromXml(result.Element("search_metadata"));
			return result.Element("statuses").Elements().Select(x => Tweet.FromXml(x)).ToArray();
		}
	}
}
