using System;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	sealed partial class DatazipListDialog
	{
		#region Designer
		Label la_Filter;
		TextBox tb_Filter;
		ListBox lb_List;
		Button bu_Load;
		Button bu_Accept;
		Button bu_Cancel;
		Button bu_Play;

		void InitializeComponent()
		{
			this.la_Filter = new System.Windows.Forms.Label();
			this.tb_Filter = new System.Windows.Forms.TextBox();
			this.lb_List = new System.Windows.Forms.ListBox();
			this.bu_Load = new System.Windows.Forms.Button();
			this.bu_Accept = new System.Windows.Forms.Button();
			this.bu_Cancel = new System.Windows.Forms.Button();
			this.bu_Play = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// la_Filter
			// 
			this.la_Filter.Location = new System.Drawing.Point(0, 0);
			this.la_Filter.Margin = new System.Windows.Forms.Padding(0);
			this.la_Filter.Name = "la_Filter";
			this.la_Filter.Size = new System.Drawing.Size(45, 21);
			this.la_Filter.TabIndex = 0;
			this.la_Filter.Text = "filtr";
			this.la_Filter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tb_Filter
			// 
			this.tb_Filter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_Filter.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tb_Filter.Location = new System.Drawing.Point(45, 0);
			this.tb_Filter.Margin = new System.Windows.Forms.Padding(0);
			this.tb_Filter.Name = "tb_Filter";
			this.tb_Filter.Size = new System.Drawing.Size(247, 22);
			this.tb_Filter.TabIndex = 1;
			this.tb_Filter.WordWrap = false;
			this.tb_Filter.TextChanged += new System.EventHandler(this.textchanged_filter);
			// 
			// lb_List
			// 
			this.lb_List.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lb_List.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lb_List.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lb_List.FormattingEnabled = true;
			this.lb_List.IntegralHeight = false;
			this.lb_List.ItemHeight = 14;
			this.lb_List.Location = new System.Drawing.Point(0, 22);
			this.lb_List.Margin = new System.Windows.Forms.Padding(0);
			this.lb_List.Name = "lb_List";
			this.lb_List.Size = new System.Drawing.Size(292, 380);
			this.lb_List.TabIndex = 2;
			this.lb_List.SelectedIndexChanged += new System.EventHandler(this.selectedindexchanged_list);
			this.lb_List.DoubleClick += new System.EventHandler(this.doubleclick_list);
			// 
			// bu_Load
			// 
			this.bu_Load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.bu_Load.Location = new System.Drawing.Point(1, 405);
			this.bu_Load.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Load.Name = "bu_Load";
			this.bu_Load.Size = new System.Drawing.Size(75, 22);
			this.bu_Load.TabIndex = 3;
			this.bu_Load.Text = "load ...";
			this.bu_Load.UseVisualStyleBackColor = true;
			this.bu_Load.Click += new System.EventHandler(this.click_Load);
			// 
			// bu_Accept
			// 
			this.bu_Accept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Accept.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bu_Accept.Enabled = false;
			this.bu_Accept.Location = new System.Drawing.Point(139, 402);
			this.bu_Accept.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Accept.Name = "bu_Accept";
			this.bu_Accept.Size = new System.Drawing.Size(75, 25);
			this.bu_Accept.TabIndex = 4;
			this.bu_Accept.Text = "Accept";
			this.bu_Accept.UseVisualStyleBackColor = true;
			// 
			// bu_Cancel
			// 
			this.bu_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bu_Cancel.Location = new System.Drawing.Point(215, 402);
			this.bu_Cancel.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Cancel.Name = "bu_Cancel";
			this.bu_Cancel.Size = new System.Drawing.Size(75, 25);
			this.bu_Cancel.TabIndex = 5;
			this.bu_Cancel.Text = "cancel";
			this.bu_Cancel.UseVisualStyleBackColor = true;
			// 
			// bu_Play
			// 
			this.bu_Play.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.bu_Play.Enabled = false;
			this.bu_Play.Location = new System.Drawing.Point(1, 427);
			this.bu_Play.Margin = new System.Windows.Forms.Padding(0);
			this.bu_Play.Name = "bu_Play";
			this.bu_Play.Size = new System.Drawing.Size(289, 23);
			this.bu_Play.TabIndex = 6;
			this.bu_Play.Text = "play";
			this.bu_Play.UseVisualStyleBackColor = true;
			this.bu_Play.Click += new System.EventHandler(this.click_Play);
			// 
			// DatazipListDialog
			// 
			this.AcceptButton = this.bu_Accept;
			this.CancelButton = this.bu_Cancel;
			this.ClientSize = new System.Drawing.Size(292, 450);
			this.Controls.Add(this.bu_Play);
			this.Controls.Add(this.la_Filter);
			this.Controls.Add(this.tb_Filter);
			this.Controls.Add(this.lb_List);
			this.Controls.Add(this.bu_Load);
			this.Controls.Add(this.bu_Accept);
			this.Controls.Add(this.bu_Cancel);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = global::nwn2_Chatter.Properties.Resource1.chatter_icon;
			this.MaximizeBox = false;
			this.Name = "DatazipListDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion Designer
	}
}
