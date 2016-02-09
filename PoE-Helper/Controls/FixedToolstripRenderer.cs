using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoE_Helper {

	/// <summary>
	/// Workaround for Windows Forms rendering bug
	/// http://stackoverflow.com/questions/1918247/how-to-disable-the-line-under-tool-strip-in-winform-c
	/// </summary>
	class FixedToolStripRenderer : ToolStripSystemRenderer {

		public FixedToolStripRenderer() { }

		protected override void OnRenderToolStripBorder( ToolStripRenderEventArgs e ) {
			if (e.ToolStrip.GetType() == typeof(ToolStrip)) {

			} else {
				base.OnRenderToolStripBorder(e);
			}
		}
	}
}
