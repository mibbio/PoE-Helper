using System;
using System.Linq;
using System.Windows.Forms;

namespace PoE_Helper {
	public partial class MainForm : Form {
		private void InitializeTalismanTab() {
			int lower = Properties.Settings.Default.lowerBound;
			int upper = Properties.Settings.Default.upperBound;

			inputLowerBound.Minimum = MIN_LEVEL;
			inputLowerBound.Maximum = MAX_LEVEL - 1;
			inputLowerBound.Value = (lower < MIN_LEVEL || lower > MAX_LEVEL) ? MIN_LEVEL : lower;
			inputUpperBound.Minimum = MIN_LEVEL + 1;
			inputUpperBound.Maximum = MAX_LEVEL;
			inputUpperBound.Value = (upper < inputLowerBound.Value || upper > MAX_LEVEL) ? MAX_LEVEL : upper;
			int count = (int) inputUpperBound.Value - (int) inputLowerBound.Value + 1;
			foreach (LevelComboBox lcb in tabPageTalisman.Controls.OfType<LevelComboBox>()) {
				lcb.DataSource = Enumerable.Range((int) inputLowerBound.Value, count).ToList();
			}

			levelTalisman1.SelectedIndex = Properties.Settings.Default.talis1;
			levelTalisman2.SelectedIndex = Properties.Settings.Default.talis2;
			levelTalisman3.SelectedIndex = Properties.Settings.Default.talis3;
			levelTalisman4.SelectedIndex = Properties.Settings.Default.talis4;
			levelTalisman5.SelectedIndex = Properties.Settings.Default.talis5;
		}
	}
}
