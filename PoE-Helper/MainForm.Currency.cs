using System;
using System.Windows.Forms;
using PoE_Helper.Enum;

namespace PoE_Helper {
	public partial class MainForm : Form {
		private void InitializeCurrencyTab() {
			foreach (Control item in tableButtonsInput.Controls) {
				if (!(item is Button)) { continue; }
				int index = 0;
				try {
					if (int.TryParse(item.Name.Substring(item.Name.Length - 2), out index)) {
						tooltipButtons.SetToolTip(item, currencies[index - 1].Label);
					}
					item.Tag = new TagData() {
						ButtonType = ButtonType.Input,
						Currency = index >= 0 ? currencies[index - 1] : null
					};
				}
				catch (IndexOutOfRangeException) { continue; }

			}
			foreach (Control item in tableButtonsOutput.Controls) {
				if (!(item is Button)) { continue; }
				int index = 0;
				try {
					if (int.TryParse(item.Name.Substring(item.Name.Length - 2), out index)) {
						tooltipButtons.SetToolTip(item, currencies[index - 1].Label);
					}
					item.Tag = new TagData() {
						ButtonType = ButtonType.Output,
						Currency = index >= 0 ? currencies[index - 1] : null
					};
				}
				catch (IndexOutOfRangeException) { continue; }
			}
		}
	}
}
