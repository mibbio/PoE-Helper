using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PoE_Helper {
	public partial class MainForm : Form {
		[Conditional("DEBUG")]
		private void EnableDebugMenu() {
			debugMenu.Visible = true;
		}

		private void debugMenuEntry01_Click( object sender, EventArgs e ) {
			Random rnd = new Random();
			foreach (Currency c in this.currencies) {
				c.Value = rnd.Next(1, 100);
			}
			if (tabControlFeatures.SelectedTab == tabPageSettings) {
				PopulateCurrencyValues();
			}
		}
	}
}
