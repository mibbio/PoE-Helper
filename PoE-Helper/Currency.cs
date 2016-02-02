using System.Drawing;
using System.Text.RegularExpressions;

namespace PoE_Helper {
	public sealed class Currency {
		private static readonly Regex _regexNum = new Regex(
			@"^_\d+_", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

		private string _label;
		private decimal _value;

		public Currency() : this("unknown", null) { }

		public Currency( string name, Bitmap visual, decimal value = decimal.Zero ) :
			this(name, name, visual, value) { }

		public Currency( string name, string label, Bitmap visual, decimal value = decimal.Zero ) {
			this.Name = name;
			this.Label = label;
			this.Value = value;
			this.Visual = visual;
		}

		public string Name { get; private set; }

		public string Label {
			get {
				return @_regexNum.Replace(_label, "").Replace("__", "\'").Replace('_', ' ');
			}
			private set { _label = value; }
		}

		public decimal Value {
			get {
				if (Label.Contains("Chaos")) { return (decimal) 1.0; }
				return _value;
			}
			set {
				_value = value;
			}
		}

		[SharpConfig.Ignore]
		public Bitmap Visual { get; private set; }
	}
}
