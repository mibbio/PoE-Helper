using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PoE_Helper.Enum;
using SharpConfig;

namespace PoE_Helper {
	public partial class MainForm : Form {
		#region Variables
		// main
		private readonly Properties.Internal defaultCfg = Properties.Internal.Default;
		private readonly Dictionary<ButtonType, Button> selectedCurrencyButton;
		private readonly Version applicationVersion;
		private readonly Updater updater;
		private readonly AppConfig config;

		// talisman
		private static readonly int MIN_LEVEL = 1;
		private static readonly int MAX_LEVEL = 84;
		#endregion

		public MainForm() {
			InitializeComponent();
			EnableDebugMenu();

			// getting assembly version number
			applicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			this.Text = string.Format("{0} v{1}", this.Text, VersionString(applicationVersion));

			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE-Helper");
			Directory.CreateDirectory(path);
			config = new AppConfig(Path.Combine(path, "currency.conf"));
			InitializeCurrencyTab();
			InitializeTalismanTab();this.debugMenuEntry01.Click += new System.EventHandler(this.debugMenuEntry01_Click);

			saveTimer.Tick += new EventHandler(( sender, e ) => {
				config.Save(tabPageSettings.Controls.OfType<DecimalTextBox>().ToList());
			});

			// preselect Buttons
			Button defaultInput = tableButtonsInput.Controls.OfType<Button>().First();
			Button defaultOutput = tableButtonsOutput.Controls.OfType<Button>().Where(item => ((TagData) item.Tag).Currency != ((TagData) defaultInput.Tag).Currency).First();
			selectedCurrencyButton = new Dictionary<ButtonType, Button>() {
				{ ButtonType.Input, defaultInput },
				{ ButtonType.Output, defaultOutput },
			};

			this.updater = new Updater();
			updater.VersionCheckDone += UpdateCheck_VersionCheckDone;
			this.Shown += updater.CheckRemoteVersion;
			Downloader downloader = new Downloader(new Version("1.0.1.0"));
			//downloader.Download();
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
				InitializeSettingsTab();
				if (!saveTimer.Enabled) { saveTimer.Start(); }
			} else { saveTimer.Stop(); }
		}

		private void UpdateCheck_VersionCheckDone() {
			int newerAvail = updater.LatestVersion.CompareTo(applicationVersion);
			if (newerAvail > 0) {
				// TODO start download process
				statusNewerVersion.Visible = true;
				statusNewerVersion.Text = string.Format("[Click to get version {0}]", updater.LatestVersion);
			}
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
				selectedCurrencyButton[tagData.ButtonType].BackColor = defaultCfg.ColorUnselected;
			}
			selectedCurrencyButton[tagData.ButtonType] = btn;
			selectedCurrencyButton[tagData.ButtonType].BackColor = defaultCfg.ColorSelected;
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
			if (config.State == ConfigState.Clean) { config.State = ConfigState.Tainted; }
			ConvertCurrency(selectedCurrencyButton[ButtonType.Input], selectedCurrencyButton[ButtonType.Output]);
			txtTab1Output.Refresh();
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

		public string VersionString( Version version ) {
			return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
		}
		#endregion
	}
}
