using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using PoE_Helper.Enum;
using SharpConfig;

namespace PoE_Helper {
	public partial class MainForm : Form {
		private void InitializeConfig(string configFile) {
			Configuration.ValidCommentChars = new char[] { '#' };
			configState = ConfigState.Loading;
			try {
				config = Configuration.LoadFromFile(configFile, Encoding.UTF8);
				foreach (Section sec in config) {
					Currency c = sec.CreateObject<Currency>();
					if (sec["Name"].StringValue.Contains("Chaos")) { c.Value = 1; }
					this.currencies.Add(c);
				}
			}
			catch (FileNotFoundException) {
				InitializeCurrency();
				config = new Configuration();
				byte id = 0;
				var currencyData = currencies.OfType<Currency>().OrderBy(item => item.Name).GetEnumerator();
				while (currencyData.MoveNext()) {
					config.Add(Section.FromObject(SECTION_CURRENCY + id, currencyData.Current));
					id++;
				}
				config.SaveToFile(configFile, Encoding.UTF8);
			}
			configState = ConfigState.Clean;
			saveTimer.Interval = 2000;
			saveTimer.Tick += ( object sender, EventArgs e ) => {
				if (configState == ConfigState.Tainted) {
					configState = ConfigState.Saving;
					ThreadPool.QueueUserWorkItem(new WaitCallback(state => {
						foreach (DecimalTextBox d in tabPageSettings.Controls.OfType<DecimalTextBox>()) {
							try {
								var index = Convert.ToInt32(d.Tag);
								config[SECTION_CURRENCY + index.ToString()]["Value"].SetValue(d.Value.ToString(CultureInfo.InvariantCulture));
								currencies[index].Value = d.Value;
							}
							catch (Exception) { continue; }
						}
						config.SaveToFile(configFile, Encoding.UTF8);
						configState = ConfigState.Clean;
					}));
				}
			};
		}

		private void InitializeCurrency() {
			ResourceSet iconResourceSet = CurrencyIcons.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
			var iconItems = iconResourceSet.GetEnumerator();
			while (iconItems.MoveNext()) {
				if (!iconItems.Key.ToString().Contains("Chaos")) {
					this.currencies.Add(new Currency(iconItems.Key.ToString(), (Bitmap) iconItems.Value));
				} else {
					this.currencies.Add(new Currency(iconItems.Key.ToString(), (Bitmap) iconItems.Value, 1));
				}
			}
			iconResourceSet.Close();
		}

		private void PopulateCurrencyValues() {
			ThreadPool.QueueUserWorkItem(new WaitCallback(state => {
				foreach (DecimalTextBox dtb in tabPageSettings.Controls.OfType<DecimalTextBox>()) {
					while (!dtb.IsHandleCreated) { Thread.Sleep(100); }
					int index = -1;
					int.TryParse(dtb.Name.Substring(dtb.Name.Length - 2), out index);
					if (index >= 0) {
						try {
							dtb.Invoke(new Action(() => {
								dtb.Value = dtb.Value = this.currencies[index - 1].Value;
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