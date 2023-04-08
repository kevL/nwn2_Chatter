using System;
using System.Drawing;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	/// <summary>
	/// Used by ToolStrips/StatusStrips to get rid of white borders and draw a
	/// 3d-ish border.
	/// </summary>
	sealed class StripRenderer
		: ToolStripProfessionalRenderer
	{
		/// <summary>
		/// Overrides the standard toolstrip background renderer.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			e.Graphics.FillRectangle(SystemBrushes.Control, e.AffectedBounds);
		}

		/// <summary>
		/// Overrides the standard toolstrip border renderer.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			e.Graphics.DrawLine(Pens.Black, 0,0, e.ToolStrip.Width, 0);
			e.Graphics.DrawLine(Pens.Black, 0,0, 0, e.ToolStrip.Height);
		}
	}
}
