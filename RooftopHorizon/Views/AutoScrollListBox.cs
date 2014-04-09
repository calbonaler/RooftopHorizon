using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RooftopHorizon.Views
{
	public class AutoScrollListBox : ListBox
	{
		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			this.ScrollIntoView(SelectedItem);
			base.OnSelectionChanged(e);
		}
	}
}
