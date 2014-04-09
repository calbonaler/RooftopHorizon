using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Saruna.Streams
{
	public class StreamNoticeDistributor
	{
		public StreamNoticeDistributor() { NoticeTypes = new List<Type>(); }

		public IList<Type> NoticeTypes { get; private set; }

		public IStreamNotice Distribute(XElement element)
		{
			IStreamNotice notice = null;
			foreach (var noticeType in NoticeTypes)
			{
				notice = (IStreamNotice)Activator.CreateInstance(noticeType);
				if (notice.Assign(element))
					return notice;
			}
			return null;
		}
	}
}
