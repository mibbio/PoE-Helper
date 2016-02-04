using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using PoE_Helper.Enum;
using SharpConfig;

namespace PoE_Helper {
	public class AppConfig {
		private const string SECTION_CURRENCY = "Currency";

		private readonly string saveFile;
		private readonly List<Currency> currencies;

		private Configuration config;
		private ConfigState configState = ConfigState.Loading;

		public AppConfig( string configFile ) {
			this.saveFile = configFile;
			this.configState = ConfigState.Loading;
			this.currencies = new List<Currency>(24);
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
		}

		public void Save( List<DecimalTextBox> valueContainer ) {
			if (State == ConfigState.Tainted) {
				State = ConfigState.Saving;
				lock (this) {
					ThreadPool.QueueUserWorkItem(new WaitCallback(state => {
						foreach (DecimalTextBox d in valueContainer) {
							try {
								var index = Convert.ToInt32(d.Tag);
								config[SECTION_CURRENCY + index.ToString()]["Value"].SetValue(d.Value.ToString(CultureInfo.InvariantCulture));
								currencies[index].Value = d.Value;
							}
							catch (Exception) { continue; }
						}
						config.SaveToFile(saveFile, Encoding.UTF8);
						State = ConfigState.Clean;
					}));
				}
			}
		}

		public void InitializeCurrency() {
			lock (this) {
				ThreadPool.QueueUserWorkItem(new WaitCallback(( state ) => {
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
				}));
			}
		}

		[Conditional("DEBUG")]
		public void RandomizeCurrency() {
			Random rnd = new Random();
			foreach (Currency c in this.currencies) { c.Value = rnd.Next(1, 100); }
			State = ConfigState.Tainted;
		}

		public Currency GetCurrency( string name ) {
			return this.currencies.DefaultIfEmpty(null).FirstOrDefault(c => c.Name == name);
		}

		public Currency GetCurrency( int index ) {
			try { return currencies[index]; }
			catch (IndexOutOfRangeException) { return null; }
		}

		public ConfigState State {
			get { return configState; }
			set { configState = value; }
		}
	}
}
