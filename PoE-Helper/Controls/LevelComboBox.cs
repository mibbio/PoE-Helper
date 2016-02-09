using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PoE_Helper {
	public class LevelComboBox : ComboBox {
		public LevelComboBox() : base() {
			this.DrawItem += LevelComboBox_DrawItem;
		}

		private void LevelComboBox_DrawItem( object sender, DrawItemEventArgs e ) {
			ComboBox cbx = sender as ComboBox;
			if (cbx != null) {
				e.DrawBackground();

				if (e.Index >= 0) {
					StringFormat sf = new StringFormat();
					sf.LineAlignment = StringAlignment.Center;
					sf.Alignment = StringAlignment.Center;

					Brush brush = new SolidBrush(cbx.ForeColor);

					if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
						brush = SystemBrushes.HighlightText;
					}

					e.Graphics.DrawString(cbx.Items[e.Index].ToString(), cbx.Font, brush, e.Bounds, sf);
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(DrawMode.OwnerDrawFixed)]
		public new DrawMode DrawMode {
			get { return DrawMode.OwnerDrawFixed; }
			set { base.DrawMode = DrawMode.OwnerDrawFixed; }
		}

		protected override void OnTextUpdate( EventArgs e ) {
			base.OnTextUpdate(e);
		}

		protected override void OnDataSourceChanged( EventArgs e ) {
			IList<int> src = null;
			if (DataSource != null && Created) {
				src = DataSource;
			}

			int index = (src != null) ? src.IndexOf(SelectedItem) : 0;
			int oldValue = SelectedItem;
			base.OnDataSourceChanged(e);
			// checking
			BeginUpdate();
			if (index < 0) {
				if (oldValue < DataSource[0]) {
					SelectedIndex = 0;
				} else {
					SelectedIndex = DataSource.Count - 1;
				}
			} else {
				SelectedIndex = index;
			}
			EndUpdate();
		}

		[Category("Data")]
		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[AttributeProvider(typeof(IList<int>))]
		public new IList<int> DataSource {
			get {
				return (IList<int>) base.DataSource;
			}
			set {
				base.DataSource = value;
			}
		}

		public new int SelectedItem {
			get {
				try {
					return Convert.ToInt32(base.SelectedItem);
				}
				catch (Exception) {
					return int.MinValue;
				}
			}
			set {
				base.SelectedItem = value;
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		public new string DisplayMember {
			get {
				return base.DisplayMember;
			}
			set {
				base.DisplayMember = value;
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		public new string ValueMember {
			get {
				return base.ValueMember;
			}
			set {
				base.ValueMember = value;
			}
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
