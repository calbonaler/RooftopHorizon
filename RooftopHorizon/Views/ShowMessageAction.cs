using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Livet.Messaging;

namespace RooftopHorizon.Views
{
	public class ShowMessageAction : System.Windows.Interactivity.TriggerAction<UIElement>
	{
		protected override async void Invoke(object parameter)
		{
			var informationMessage = parameter as InformationMessage;
			if (informationMessage != null)
			{
				var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
				var adorners = layer.GetAdorners(AssociatedObject);
				MessageAdorner adorner;
				if (adorners == null)
				{
					layer.Add(adorner = new MessageAdorner(AssociatedObject));
				}
				else
				{
					adorner = (MessageAdorner)adorners.FirstOrDefault(x => x is MessageAdorner);
					if (adorner == null)
						layer.Add(adorner = new MessageAdorner(AssociatedObject));
				}
				adorner.AddMessage(informationMessage);
				layer.Update(AssociatedObject);
				await Task.Delay(2000);
				adorner.RemoveMessage(informationMessage);
				layer.Update(AssociatedObject);
			}
		}
	}

	public class MessageAdorner : Adorner
	{
		public MessageAdorner(UIElement adornedElement) : base(adornedElement) { }

		List<InformationMessage> messages = new List<InformationMessage>();

		public void AddMessage(InformationMessage message) { messages.Add(message); }

		public void RemoveMessage(InformationMessage message) { messages.Remove(message); }

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			var window = Window.GetWindow(AdornedElement);
			double prevY = AdornedElement.RenderSize.Height;
			foreach (var message in messages)
			{
				var ft = new FormattedText(
					message.Caption + "\r\n" + message.Text,
					System.Globalization.CultureInfo.CurrentCulture,
					FlowDirection,
					new Typeface(window.FontFamily, window.FontStyle, window.FontWeight, window.FontStretch),
					window.FontSize,
					Brushes.Black);
				ft.SetFontWeight(FontWeight.FromOpenTypeWeight((int)(window.FontWeight.ToOpenTypeWeight() * 1.5)), 0, message.Caption.Length);
				ft.SetFontSize(window.FontSize * 1.2, 0, message.Caption.Length);
				drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(0, prevY - ft.Height - 4, ft.Width + 4, ft.Height + 4));
				prevY = prevY - ft.Height - 2;
				drawingContext.DrawText(ft, new Point(2, prevY));
				prevY -= 10;
			}
		}
	}
}
