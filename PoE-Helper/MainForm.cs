using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using PoE_Helper.Enum;

namespace PoE_Helper {
	public partial class MainForm : Form {
		#region Variables
		// colors
		private readonly Color colorDefault = Color.FromKnownColor(KnownColor.ControlText);
		private readonly Color colorAdvice = Color.OrangeRed;
		private readonly Color colorSuccess = Color.LimeGreen;

		// main
		private readonly Properties.Internal defaultCfg = Properties.Internal.Default;
		private readonly Dictionary<ButtonType, Button> selectedCurrencyButton;
		private readonly Version applicationVersion;
		private readonly Updater updater;
		private readonly AppConfig config;

		private string UpdateInstaller = string.Empty;

		// talisman
		private static readonly int MIN_LEVEL = 1;
		private static readonly int MAX_LEVEL = 84;
		#endregion

		public MainForm() {
			InitializeComponent();
			EnableDebugMenu();

			this.updater = new Updater();

			// getting assembly version number
			applicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			this.Text = string.Format("{0} v{1}", this.Text, VersionString(applicationVersion));

			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE-Helper");
			Directory.CreateDirectory(path);
			config = new AppConfig(Path.Combine(path, "currency.conf"));
			InitializeCurrencyTab();
			InitializeTalismanTab();
			this.debugMenuEntry01.Click += new EventHandler(this.debugMenuEntry01_Click);

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

			updater.VersionCheckDone += UpdateCheck_VersionCheckDone;
			statusBar.HandleCreated += updater.CheckRemoteVersion;
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
		#endregion

		#region Update handling
		private void UpdateCheck_VersionCheckDone() {
			int newerAvail = updater.LatestVersion.CompareTo(applicationVersion);
			if (newerAvail > 0) {
				statusBar.Invoke(() => {
					statusLabelLeft.IsLink = true;
					statusLabelLeft.LinkColor = colorAdvice;
					statusLabelLeft.Image = Icons.fa_info_circle_16;
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
				statusLabelLeft.Image = Icons.fa_download_16;
			});
			if (progress == 100) {
				UpdateInstaller = e.LocalFile;
				statusBar.Invoke(() => {
					statusLabelLeft.IsLink = true;
					statusLabelLeft.LinkColor = colorSuccess;
					statusLabelLeft.Image = Icons.fa_check_square_o_16;
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
			downloader.Download(new Version("1.0.1.0"));
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
			if (!string.IsNullOrEmpty(UpdateInstaller)) {
				Process.Start(UpdateInstaller);
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
