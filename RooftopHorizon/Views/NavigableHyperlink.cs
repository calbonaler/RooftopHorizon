using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace RooftopHorizon.Views
{
	public class NavigableHyperlink : Hyperlink
	{
		public NavigableHyperlink()
		{
			MouseEnter += OnMouseEnter;
			MouseLeave += OnMouseLeave;
			RequestNavigate += OnRequestNavigate;
			TextDecorations = null;
		}

		void OnRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			if (NavigateUri != null)
				System.Diagnostics.Process.Start(NavigateUri.AbsoluteUri);
		}

		void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) { TextDecorations = null; }

		void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e) { TextDecorations = System.Windows.TextDecorations.Underline; }
	}
}
