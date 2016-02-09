using System;
using System.Windows.Forms;

namespace PoE_Helper {
	public static class ControlExtensions {
		public static void Invoke(this Control control, Action action) {
			control.Invoke(action);
		}
	}
}
