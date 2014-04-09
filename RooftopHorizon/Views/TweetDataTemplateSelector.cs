using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Saruna;

namespace RooftopHorizon.Views
{
	public class TweetDataTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;
			Tweet tweet = item as Tweet;
			if (element != null && tweet != null)
			{
				if (tweet.RetweetSource != null)
					return element.FindResource("RetweetDataTemplate") as DataTemplate;
				else
					return element.FindResource("TweetDataTemplate") as DataTemplate;
			}
			return base.SelectTemplate(item, container);
		}
	}
}
