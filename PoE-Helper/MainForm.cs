using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PoE_Helper {
	public partial class MainForm : Form {
		#region Variables
		// colors
		private readonly Color colorDefault = Color.FromKnownColor(KnownColor.ControlText);
		private readonly Color colorAdvice = Color.OrangeRed;
		private readonly Color colorSuccess = Color.LimeGreen;

		// main
		private readonly Properties.Settings settings = Properties.Settings.Default;
		private readonly Dictionary<ButtonType, Button> selectedCurrencyButton;
		private readonly Version applicationVersion;
		private readonly Updater updater;
		private readonly AppConfig config;

		private string InstallerFileName = string.Empty;
		private bool initialized = false;

		// talisman
		private static readonly int MIN_LEVEL = 1;
		private static readonly int MAX_LEVEL = 84;
		private TalismanCalculator calculator;
		#endregion

		public MainForm() {
			InitializeComponent();

			this.toolStripTop.Renderer = new FixedToolStripRenderer();
			this.toolStripComboLeague.SelectedIndex = 2;

			this.updater = new Updater();

			// getting assembly version number
			this.applicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			this.Text = string.Format("{0} v{1}", this.Text, VersionString(applicationVersion));

			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE-Helper");
			Directory.CreateDirectory(path);
			config = new AppConfig(Path.Combine(path, "currency.conf"));

			EnableDebugMenu();
			InitializeCurrencyTab();
			InitializeTalismanTab();

			// preselect Buttons
			Button defaultInput = tableButtonsInput.Controls.OfType<Button>().First();
			Button defaultOutput = tableButtonsOutput.Controls.OfType<Button>().Where(item => ((TagData) item.Tag).Currency != ((TagData) defaultInput.Tag).Currency).First();
			selectedCurrencyButton = new Dictionary<ButtonType, Button>() {
				{ ButtonType.Input, defaultInput },
				{ ButtonType.Output, defaultOutput },
			};

			// assigning event handler
			this.debugMenuEntry01.Click += new EventHandler(this.debugMenuEntry01_Click);
			this.updater.VersionCheckDone += new Updater.VersionCheckDoneEventHandler(UpdateCheck_VersionCheckDone);
			this.statusBar.HandleCreated += new EventHandler(updater.CheckRemoteVersion);
			this.remoteDataTimer.Tick += new EventHandler(config.LoadExternData);
			this.config.ExternDataLoaded += new AppConfig.ExternDataLoadedEventHandler(Config_ExternDataLoaded);
			this.saveTimer.Tick += new EventHandler(( sender, e ) => {
				config.Save(tabPageSettings.Controls.OfType<DecimalTextBox>().ToList());
			});

			// set saved ui states
			if (Properties.Settings.Default.requestOnline) {
				toolStripExternSwitch.PerformClick();
			}
			this.initialized = true;
		}

		#region init methods
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

		[Conditional("DEBUG")]
		private void EnableDebugMenu() {
			this.debugMenuEntry01.Click += new EventHandler(this.debugMenuEntry01_Click);
			debugMenu.Visible = true;
		}

		#endregion

		#region Update handling
		private void UpdateCheck_VersionCheckDone() {
			int newerAvail = updater.LatestVersion.CompareTo(applicationVersion);
			if (newerAvail > 0) {
				statusBar.Invoke(() => {
					statusLabelLeft.IsLink = true;
					statusLabelLeft.LinkColor = colorAdvice;
					statusLabelLeft.Image = IconsGeneral.fa_info_circle_16;
					statusLabelLeft.Text = string.Format("Version '{0}' available. Click to start download. ", updater.LatestVersion);
				});
				statusLabelLeft.Click += StatusLabelLeft_StartDownload;
			}
		}

		private void Downloader_ProgressChangedEvent( int progress, Downloader.DownloadEventArgs e ) {
			string text = string.Format("Download {0}{1}",
				(progress < 100) ? progress.ToString() : "complete. Click to start Update.",
				(progress < 100) ? "%" : ""
			);
			statusBar.Invoke(() => {
				statusLabelLeft.Text = text;
				statusLabelLeft.LinkColor = colorDefault;
				statusLabelLeft.Image = IconsGeneral.fa_download_16;
			});
			if (progress == 100) {
				InstallerFileName = e.LocalFile;
				statusBar.Invoke(() => {
					statusLabelLeft.IsLink = true;
					statusLabelLeft.LinkColor = colorSuccess;
					statusLabelLeft.Image = IconsGeneral.fa_check_square_o_16;
				});
				statusLabelLeft.Click -= StatusLabelLeft_StartDownload;
				statusLabelLeft.Click += StatusLabelLeft_ExecuteUpdate;
			}
		}

		private void StatusLabelLeft_StartDownload( object sender, EventArgs e ) {
			statusBar.Invoke(() => {
				statusLabelLeft.IsLink = false;
			});
			Downloader downloader = new Downloader();
			downloader.UpdateDownloadEvent += Downloader_ProgressChangedEvent;
			downloader.Download(updater.LatestVersion);
		}

		private void StatusLabelLeft_ExecuteUpdate( object sender, EventArgs e ) {
			this.FormClosed += MainForm_FormClosed;
			if (!this.IsDisposed && this.InvokeRequired) {
				this.Invoke(() => this.Close());
			} else {
				this.Close();
			}
		}

		private void MainForm_FormClosed( object sender, FormClosedEventArgs e ) {
			if (!string.IsNullOrEmpty(InstallerFileName)) {
				Process.Start(InstallerFileName);
			}
		}
		#endregion

		#region General event handling
		private void MainForm_Load( object sender, EventArgs e ) {
			selectedCurrencyButton[ButtonType.Input]?.PerformClick();
			selectedCurrencyButton[ButtonType.Output]?.PerformClick();
		}

		private void tabControlFeatures_Selecting( object sender, TabControlCancelEventArgs e ) {
			TabPage page = ((TabControl) sender).SelectedTab;
			if (page == tabPageSettings) {
				InitializeSettingsTab();
				if (!saveTimer.Enabled) { saveTimer.Start(); }
			} else { saveTimer.Stop(); }
		}

		private void toolStripExternSwitch_Click( object sender, EventArgs e ) {
			if (sender is ToolStripButton) {
				ToolStripButton btn = (ToolStripButton) sender;
				if (btn.CheckState == CheckState.Checked) {
					Properties.Settings.Default.requestOnline = true;
					Properties.Settings.Default.Save();
					btn.Image = IconsGeneral.fa_check_16;
					remoteDataTimer.Start();
				} else {
					Properties.Settings.Default.requestOnline = false;
					Properties.Settings.Default.Save();
					btn.Image = IconsGeneral.fa_close_16;
					remoteDataTimer.Stop();
				}
			}
		}

		private void debugMenuEntry01_Click( object sender, EventArgs e ) {
#if DEBUG
			if (tabControlFeatures.SelectedTab == tabPageSettings) {
				config.RandomizeCurrency();
				InitializeSettingsTab();
			}
#endif
		}

		private void Config_ExternDataLoaded() {
			remoteDataTimer.Interval = 900000;
			InitializeSettingsTab();
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
				selectedCurrencyButton[tagData.ButtonType].BackColor = settings.ColorUnselected;
			}
			selectedCurrencyButton[tagData.ButtonType] = btn;
			selectedCurrencyButton[tagData.ButtonType].BackColor = settings.ColorSelected;
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

		#region 'Talisman' tab event handling
		private void inputLevelBounds_ValueChanged( object sender, EventArgs e ) {
			// ensure lower and upper bound don't overlap
			if (sender == inputLowerBound) {
				inputUpperBound.Minimum = inputLowerBound.Value + 1;
			} else {
				inputLowerBound.Maximum = inputUpperBound.Value - 1;
			}
			int count = (int) inputUpperBound.Value - (int) inputLowerBound.Value + 1;
			foreach (LevelComboBox lcb in tabPageTalisman.Controls.OfType<LevelComboBox>()) {
				lcb.DataSource = Enumerable.Range((int) inputLowerBound.Value, count).ToList();
			}
			Properties.Settings.Default.lowerBound = Convert.ToInt32(inputLowerBound.Value);
			Properties.Settings.Default.upperBound = Convert.ToInt32(inputUpperBound.Value);
			Properties.Settings.Default.Save();
		}

		private void levelTalisman_SelectedIndexChanged( object sender, EventArgs e ) {
			if (this.calculator == null) {
				this.calculator = new TalismanCalculator(levelTalisman1.SelectedItem,
					levelTalisman2.SelectedItem, levelTalisman3.SelectedItem,
					levelTalisman4.SelectedItem, levelTalisman5.SelectedItem);
			}
			LevelComboBox lcb = sender as LevelComboBox;
			this.calculator.SetTalismanLevel(lcb.SelectedItem, Convert.ToInt32(lcb.Tag));
			int[] medium = this.calculator.MediumTalismans;
			labelLevelT1.Text = this.calculator.HighestTalisman.ToString();
			labelLevelT2.Text = medium[0].ToString();
			labelLevelT3.Text = medium[1].ToString();
			labelLevelT4.Text = medium[2].ToString();
			labelLevelT5.Text = this.calculator.LowestTalisman.ToString();
			labelLevelResult.Text = this.calculator.CombinedTalisman.ToString();

			if (this.initialized) {
				Properties.Settings.Default.talis1 = levelTalisman1.SelectedIndex;
				Properties.Settings.Default.talis2 = levelTalisman2.SelectedIndex;
				Properties.Settings.Default.talis3 = levelTalisman3.SelectedIndex;
				Properties.Settings.Default.talis4 = levelTalisman4.SelectedIndex;
				Properties.Settings.Default.talis5 = levelTalisman5.SelectedIndex;
				Properties.Settings.Default.Save();
			}
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
