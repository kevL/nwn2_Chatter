using System;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	sealed class TabPageControl
		: TabControl
	{
		// DoDragDrop/OnDragEnter/OnDragOver/OnDragDrop/OnDragLeave
		// NOTE: The MouseUp event does NOT fire when a drag-drop is released.

//		#region Fields
//		TabPage _tabDrag;
//		#endregion Fields


		#region Properties (override)
		/// <summary>
		/// Prevents flicker.
		/// </summary>
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000; // enable 'WS_EX_COMPOSITED'
				return cp;
			}
		}
		#endregion Properties (override)
	}
}
