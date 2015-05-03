using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Saruna
{
	public class SearchTimeline : Timeline<Tweet>
	{
		public SearchTimeline(IAccount authData, Infrastructures.RequestContent content) : base(authData, content) { }

		SearchMetadata data = null;
		public string Query { get { return RefreshContent.GetParameter("q"); } }

		public async new Task<SearchMetadata> InsertBetweenAsync(int newerThan, int olderThan, int count)
		{
			await base.InsertBetweenAsync(newerThan, olderThan, count).ConfigureAwait(false);
			return data;
		}

		protected override async Task<IReadOnlyList<Tweet>> GetResultsAsync()
		{
			var result = await Infrastructures.RequestSender.GetXmlAsync(AuthorizationData, RefreshContent).ConfigureAwait(false);
			data = SearchMetadata.FromXml(result.Element("search_metadata"));
			return result.Element("statuses").Elements().Select(x => Tweet.FromXml(x)).ToArray();
		}
	}
}
