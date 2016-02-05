using System;
using System.Linq;
using System.Windows.Forms;

namespace PoE_Helper {
	public partial class MainForm : Form {
		private void InitializeTalismanTab() {
			inputLowerBound.Minimum = MIN_LEVEL;
			inputLowerBound.Maximum = MAX_LEVEL - 1;
			inputLowerBound.Value = MIN_LEVEL;
			inputUpperBound.Minimum = MIN_LEVEL + 1;
			inputUpperBound.Maximum = MAX_LEVEL;
			inputUpperBound.Value = MAX_LEVEL;
			int count = (int) inputUpperBound.Value - (int) inputLowerBound.Value + 1;
			foreach (LevelComboBox lcb in tabPageTalisman.Controls.OfType<LevelComboBox>()) {
				lcb.DataSource = Enumerable.Range((int) inputLowerBound.Value, count).ToList();
			}
		}
	}
}
