using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace RooftopHorizon.Views
{
	public class ControlEnterKeyDownEventTrigger : EventTrigger
	{
		public ControlEnterKeyDownEventTrigger() : base("KeyDown") { }

		protected override void OnEvent(EventArgs eventArgs)
		{
			var e = eventArgs as KeyEventArgs;
			if (e != null && e.KeyboardDevice.IsKeyDown(Key.Enter) &&
				(e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
				this.InvokeActions(eventArgs);
		}
	}
}
