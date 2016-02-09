using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace PoE_Helper {
	[Localizable(true)]
	public class DecimalTextBox : TextBox {
		private static readonly NumberFormatInfo DecimalFormat = CultureInfo.CurrentUICulture.NumberFormat;
		private static readonly string DecimalSeperator = DecimalFormat.CurrencyDecimalSeparator;
		private static readonly int DecimalDigits = DecimalFormat.CurrencyDecimalDigits;

		public DecimalTextBox() : base() { }

		#region events
		protected override void OnTextChanged( EventArgs e ) {
			if (IsDecimal()) {
				base.OnTextChanged(e);
			}
		}

		protected override void OnKeyPress( KeyPressEventArgs e ) {
			if (!char.IsNumber(e.KeyChar)
				&& ((Keys) e.KeyChar != Keys.Back)
				&& ((Keys) e.KeyChar != Keys.Return)
				&& (e.KeyChar.ToString() != DecimalSeperator)) {
				e.Handled = true;
			}
			if (e.KeyChar.ToString() == DecimalSeperator && Text.IndexOf(DecimalSeperator) > 0) {
				e.Handled = true;
			}

			base.OnKeyPress(e);
		}

		protected override void OnGotFocus( EventArgs e ) {
			if (ReadOnly) { return; }
			ResetValueOnFocus();
			base.OnGotFocus(e);
		}

		protected override void OnValidating( CancelEventArgs e ) {
			decimal value;
			decimal.TryParse(Text, out value);
			Text = value.ToString("N" + DecimalDigits);
		}

		protected override void OnCreateControl() {
			if (Value != 0) {
				decimal value;
				decimal.TryParse("0", out value);
				Text = value.ToString("N" + DecimalDigits);
			}
			base.OnCreateControl();
		}
		#endregion

		private void ResetValueOnFocus() {
			if (IsDecimal()) {
				if (!IsDecimalZero()) {
					return;
				}
			}
			Text = "";
		}

		private bool IsDecimal() {
			decimal result;
			return decimal.TryParse(Text, out result);
		}

		private bool IsDecimalZero() {
			return (decimal.Parse(Text, DecimalFormat) == 0);
		}

		#region properties
		[Browsable(false)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Category("Appearance")]
		[Description("Der anzuzeigende Wert.")]
		[DefaultValue(typeof(decimal), "0")]
		public decimal Value {
			get {
				decimal d = new decimal(0.0);
				try { d = decimal.Parse(Text); }
				catch (FormatException) { }
				return d;
			}
			set { Text = value.ToString("N" + DecimalDigits); }
		}
		#endregion
	}
}
