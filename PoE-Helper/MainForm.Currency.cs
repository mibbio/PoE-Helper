using System;
using System.Linq;
using System.Threading;
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
						tooltipButtons.SetToolTip(item, config.GetCurrency(index - 1).Label);
					}
					item.Tag = new TagData() {
						ButtonType = ButtonType.Input,
						Currency = index >= 0 ? config.GetCurrency(index - 1) : null
					};
				}
				catch (IndexOutOfRangeException) { continue; }

			}
			foreach (Control item in tableButtonsOutput.Controls) {
				if (!(item is Button)) { continue; }
				int index = 0;
				try {
					if (int.TryParse(item.Name.Substring(item.Name.Length - 2), out index)) {
						tooltipButtons.SetToolTip(item, config.GetCurrency(index - 1).Label);
					}
					item.Tag = new TagData() {
						ButtonType = ButtonType.Output,
						Currency = index >= 0 ? config.GetCurrency(index - 1) : null
					};
				}
				catch (IndexOutOfRangeException) { continue; }
			}
		}

		public void InitializeSettingsTab() {
			ThreadPool.QueueUserWorkItem(new WaitCallback(state => {
				foreach (DecimalTextBox dtb in tabPageSettings.Controls.OfType<DecimalTextBox>()) {
					while (!dtb.IsHandleCreated) { Thread.Sleep(100); }
					int index = -1;
					int.TryParse(dtb.Name.Substring(dtb.Name.Length - 2), out index);
					if (index >= 0) {
						try {
							dtb.Invoke(new Action(() => {
								dtb.Value = dtb.Value = config.GetCurrency(index - 1).Value;
								dtb.Tag = index - 1;
								dtb.Refresh();
							}));
						}
						catch (ArgumentOutOfRangeException) { }
					}
				}
			}));
		}
	}
}
