using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoE_Helper {
	public partial class LevelComboBox : ComboBox {
		public LevelComboBox() : base() { }

		protected override void OnTextUpdate( EventArgs e ) {
			base.OnTextUpdate(e);
		}

		protected override void OnDataSourceChanged( EventArgs e ) {
			base.OnDataSourceChanged(e);
		}

		[Category("Data")]
		[DefaultValue(1)]
		[DisplayName("MinimumValue")]
		[Description("Der Minimalwert, den das Steuerelement anzeigt.")]
		public int MinValue { get; set; }

		[Category("Data")]
		[DefaultValue(100)]
		[DisplayName("MaximumValue")]
		[Description("Der Maximalwert, den das Steuerelement anzeigt.")]
		public int MaxValue { get; set; }
	}
}
