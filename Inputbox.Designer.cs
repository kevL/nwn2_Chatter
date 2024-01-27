using System;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	sealed partial class Inputbox
	{
		#region Designer
		TextBox tb_input;

		void InitializeComponent()
		{
			this.tb_input = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// tb_input
			// 
			this.tb_input.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb_input.Location = new System.Drawing.Point(0, 0);
			this.tb_input.Margin = new System.Windows.Forms.Padding(0);
			this.tb_input.Name = "tb_input";
			this.tb_input.Size = new System.Drawing.Size(242, 24);
			this.tb_input.TabIndex = 0;
			this.tb_input.TextChanged += new System.EventHandler(this.textchanged_input);
			// 
			// Inputbox
			// 
			this.ClientSize = new System.Drawing.Size(242, 39);
			this.Controls.Add(this.tb_input);
			this.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = global::nwn2_Chatter.Properties.Resource1.chatter_icon;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.Name = "Inputbox";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion Designer
	}
}
