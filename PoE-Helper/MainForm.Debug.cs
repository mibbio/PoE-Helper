using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PoE_Helper {
	public partial class MainForm : Form {
		[Conditional("DEBUG")]
		private void EnableDebugMenu() {
			this.debugMenuEntry01.Click += new EventHandler(this.debugMenuEntry01_Click);
			debugMenu.Visible = true;
		}

		private void debugMenuEntry01_Click( object sender, EventArgs e ) {
#if DEBUG
			if (tabControlFeatures.SelectedTab == tabPageSettings) {
				config.RandomizeCurrency();
				InitializeSettingsTab();
			}
#endif
		}
	}
}
