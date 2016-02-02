using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PoE_Helper.Enum;
using SharpConfig;

namespace PoE_Helper {
	public partial class MainForm : Form {
		#region Variables
		// main
		private readonly Properties.Internal permCfg = Properties.Internal.Default;
		private readonly Dictionary<ButtonType, Button> selectedCurrencyButton;
		private readonly List<Currency> currencies;

		// talisman
		private static readonly int MIN_LEVEL = 1;
		private static readonly int MAX_LEVEL = 84;

		// settings
		private readonly string CONFIG_FILE = "currency.conf";
		private const string SECTION_CURRENCY = "Currency";
		private Configuration config;
		private ConfigState configState = ConfigState.Loading;
		#endregion

		public MainForm() {
			InitializeComponent();
			EnableDebugMenu();

			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE-Helper");
			Directory.CreateDirectory(path);
			CONFIG_FILE = Path.Combine(path, "currency.conf");
			this.currencies = new List<Currency>(24);

			InitializeConfig();
			InitializeCurrencyTab();
			InitializeTalismanTab();

			// preselect Buttons
			Button defaultInput = tableButtonsInput.Controls.OfType<Button>().First();
			Button defaultOutput = tableButtonsOutput.Controls.OfType<Button>().Where(item => ((TagData) item.Tag).Currency != ((TagData) defaultInput.Tag).Currency).First();
			selectedCurrencyButton = new Dictionary<ButtonType, Button>() {
				{ ButtonType.Input, defaultInput },
				{ ButtonType.Output, defaultOutput },
			};
		}

		#region General event handling
		private void MainForm_Load( object sender, EventArgs e ) {
			selectedCurrencyButton[ButtonType.Input]?.PerformClick();
			selectedCurrencyButton[ButtonType.Output]?.PerformClick();
		}

		private void tabControlFeatures_Selecting( object sender, TabControlCancelEventArgs e ) {
			TabPage page = ((TabControl) sender).SelectedTab;
			if (page == tabPageTalisman) {
				e.Cancel = true;
			}
			if (page == tabPageSettings) {
				PopulateCurrencyValues();
				if (!saveTimer.Enabled) { saveTimer.Start(); }
			} else { saveTimer.Stop(); }
		}
		#endregion

		#region 'Currency' tab event handling
		/// <summary>
		/// Set selected currency
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonCurrency_Click( object sender, EventArgs e ) {
			if (!(sender is Button)) { return; }

			// change active button
			if ((sender as Button)?.Tag == null) { return; }
			Button btn = sender as Button;

			var tagData = (TagData) btn.Tag;
			if (selectedCurrencyButton[tagData.ButtonType] != null) {
				selectedCurrencyButton[tagData.ButtonType].BackColor = permCfg.ColorUnselected;
			}
			selectedCurrencyButton[tagData.ButtonType] = btn;
			selectedCurrencyButton[tagData.ButtonType].BackColor = permCfg.ColorSelected;
			if (selectedCurrencyButton[ButtonType.Input] != null && selectedCurrencyButton[ButtonType.Output] != null) {
				ConvertCurrency(selectedCurrencyButton[ButtonType.Input], selectedCurrencyButton[ButtonType.Output]);
			}
		}

		/// <summary>
		/// Swap selected source and target currency
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSwapCurrency_Click( object sender, EventArgs e ) {
			var b1 = GetCounterpart(selectedCurrencyButton[ButtonType.Input]);
			var b2 = GetCounterpart(selectedCurrencyButton[ButtonType.Output]);
			if (b1 == b2) { return; }
			b1?.PerformClick();
			b2?.PerformClick();
		}

		private void txtTab1Input_TextChanged( object sender, EventArgs e ) {
			ConvertCurrency(selectedCurrencyButton[ButtonType.Input], selectedCurrencyButton[ButtonType.Output]);
			txtTab1Output.Refresh();
		}

		private void txtTab1Output_Click( object sender, EventArgs e ) {
			ActiveControl = tabControlFeatures;
		}

		private void currencyTextBox_Validated( object sender, EventArgs e ) {
			if (configState == ConfigState.Clean) { configState = ConfigState.Tainted; }
		}
		#endregion

		#region Utility functions
		/// <summary>
		/// Get corresponding currency button
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		private Button GetCounterpart( Button button ) {
			var tag = (TagData) button?.Tag;
			IEnumerable<Button> candidates;
			if (tag.ButtonType == ButtonType.Input) {
				candidates = tableButtonsOutput.Controls.OfType<Button>();
			} else {
				candidates = tableButtonsInput.Controls.OfType<Button>();
			}
			if (candidates.Count() <= 0) { return null; }
			return candidates.Where(btn => ((TagData) btn.Tag).Currency == tag.Currency).DefaultIfEmpty(null).First();
		}

		private void ConvertCurrency( Control source, Control target ) {
			if (source?.Tag == null || target?.Tag == null) { return; }
			Currency c1 = ((TagData) source.Tag).Currency;
			Currency c2 = ((TagData) target.Tag).Currency;
			if (c1 != null && c2 != null) {
				decimal result = new decimal(0.0);
				try {
					result = c1.Value / c2.Value * txtTab1Input.Value;
				}
				catch (DivideByZeroException) { }
				txtTab1Output.Value = result;
			}
		}
		#endregion
	}
}
