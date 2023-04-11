using System;
using System.Drawing;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	partial class Chatter
	{
		Panel pa_obstructor;

		/// <summary>
		/// Initializes the obstructor panel.
		/// </summary>
		void InitializeObstructor()
		{
			pa_obstructor = new Panel();
			pa_obstructor.BackColor = SystemColors.ControlDark;
			pa_obstructor.Dock = DockStyle.Fill;
			pa_obstructor.Location = new Point(0, 24);
			pa_obstructor.Margin = new Padding(0);
			pa_obstructor.Name = "pa_obstructor";
			pa_obstructor.Size = new Size(742, 450);
			pa_obstructor.TabIndex = 3;
			Controls.Add(pa_obstructor);
			pa_obstructor.BringToFront();
		}


		MenuStrip menubar;
		ToolStripMenuItem it_file;
		ToolStripMenuItem it_file_create10;
		ToolStripMenuItem it_file_create11;
		ToolStripMenuItem it_file_recent;
		ToolStripMenuItem it_file_open;
		ToolStripMenuItem it_file_opendatazip;
		ToolStripMenuItem it_file_save;
		ToolStripMenuItem it_file_saveas;
		ToolStripMenuItem it_file_close;
		ToolStripMenuItem it_file_quit;
		ToolStripMenuItem it_outputformat;
		ToolStripMenuItem it_outputformat_10;
		ToolStripMenuItem it_outputformat_11;
		ToolStripMenuItem it_outputformat_standard;
		ToolStripMenuItem it_outputformat_extended;
		ToolStripMenuItem it_talktable;
		ToolStripMenuItem it_talktable_load;

		ToolStripLabel la_about;

		ToolStripSeparator tss_recent;
		ToolStripSeparator tss1;
		ToolStripSeparator tss2;
		ToolStripSeparator tss3;

		TabPageControl tc_pages;
		StatusStrip ss_bot;
		ToolStripStatusLabel tssl_file;
		ToolStripStatusLabel tssl_ver;
		ToolStripStatusLabel tssl_extended;
		ToolStripStatusLabel tssl_pfe;


		/// <summary>
		/// This method is required for Windows Forms designer support. Do not
		/// change the method contents inside the source code editor. The Forms
		/// designer might not be able to load this method if it was changed
		/// manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.menubar = new System.Windows.Forms.MenuStrip();
			this.it_file = new System.Windows.Forms.ToolStripMenuItem();
			this.it_file_create10 = new System.Windows.Forms.ToolStripMenuItem();
			this.it_file_create11 = new System.Windows.Forms.ToolStripMenuItem();
			this.tss_recent = new System.Windows.Forms.ToolStripSeparator();
			this.it_file_recent = new System.Windows.Forms.ToolStripMenuItem();
			this.tss1 = new System.Windows.Forms.ToolStripSeparator();
			this.it_file_open = new System.Windows.Forms.ToolStripMenuItem();
			this.it_file_opendatazip = new System.Windows.Forms.ToolStripMenuItem();
			this.it_file_save = new System.Windows.Forms.ToolStripMenuItem();
			this.it_file_saveas = new System.Windows.Forms.ToolStripMenuItem();
			this.it_file_close = new System.Windows.Forms.ToolStripMenuItem();
			this.tss2 = new System.Windows.Forms.ToolStripSeparator();
			this.it_file_quit = new System.Windows.Forms.ToolStripMenuItem();
			this.it_outputformat = new System.Windows.Forms.ToolStripMenuItem();
			this.it_outputformat_10 = new System.Windows.Forms.ToolStripMenuItem();
			this.it_outputformat_11 = new System.Windows.Forms.ToolStripMenuItem();
			this.tss3 = new System.Windows.Forms.ToolStripSeparator();
			this.it_outputformat_standard = new System.Windows.Forms.ToolStripMenuItem();
			this.it_outputformat_extended = new System.Windows.Forms.ToolStripMenuItem();
			this.it_talktable = new System.Windows.Forms.ToolStripMenuItem();
			this.it_talktable_load = new System.Windows.Forms.ToolStripMenuItem();
			this.la_about = new System.Windows.Forms.ToolStripLabel();
			this.tc_pages = new nwn2_Chatter.TabPageControl();
			this.ss_bot = new System.Windows.Forms.StatusStrip();
			this.tssl_file = new System.Windows.Forms.ToolStripStatusLabel();
			this.tssl_ver = new System.Windows.Forms.ToolStripStatusLabel();
			this.tssl_extended = new System.Windows.Forms.ToolStripStatusLabel();
			this.tssl_pfe = new System.Windows.Forms.ToolStripStatusLabel();
			this.menubar.SuspendLayout();
			this.ss_bot.SuspendLayout();
			this.SuspendLayout();
			// 
			// menubar
			// 
			this.menubar.Font = new System.Drawing.Font("Comic Sans MS", 8F);
			this.menubar.GripMargin = new System.Windows.Forms.Padding(0);
			this.menubar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.it_file,
			this.it_outputformat,
			this.it_talktable,
			this.la_about});
			this.menubar.Location = new System.Drawing.Point(0, 0);
			this.menubar.Name = "menubar";
			this.menubar.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.menubar.Size = new System.Drawing.Size(742, 24);
			this.menubar.TabIndex = 0;
			this.menubar.Text = "menubar";
			// 
			// it_file
			// 
			this.it_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.it_file_create10,
			this.it_file_create11,
			this.tss_recent,
			this.it_file_recent,
			this.tss1,
			this.it_file_open,
			this.it_file_opendatazip,
			this.it_file_save,
			this.it_file_saveas,
			this.it_file_close,
			this.tss2,
			this.it_file_quit});
			this.it_file.Name = "it_file";
			this.it_file.Padding = new System.Windows.Forms.Padding(0);
			this.it_file.Size = new System.Drawing.Size(30, 24);
			this.it_file.Text = "File";
			this.it_file.DropDownOpening += new System.EventHandler(this.file_dropdownopening);
			// 
			// it_file_create10
			// 
			this.it_file_create10.Name = "it_file_create10";
			this.it_file_create10.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_create10.Size = new System.Drawing.Size(168, 20);
			this.it_file_create10.Text = "create SSF 1.0";
			this.it_file_create10.Click += new System.EventHandler(this.file_click_create);
			// 
			// it_file_create11
			// 
			this.it_file_create11.Name = "it_file_create11";
			this.it_file_create11.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_create11.Size = new System.Drawing.Size(168, 20);
			this.it_file_create11.Text = "create SSF 1.1";
			this.it_file_create11.Click += new System.EventHandler(this.file_click_create);
			// 
			// tss_recent
			// 
			this.tss_recent.Name = "tss_recent";
			this.tss_recent.Size = new System.Drawing.Size(165, 6);
			this.tss_recent.Visible = false;
			// 
			// it_file_recent
			// 
			this.it_file_recent.Name = "it_file_recent";
			this.it_file_recent.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_recent.Size = new System.Drawing.Size(168, 20);
			this.it_file_recent.Text = "recent files";
			this.it_file_recent.Visible = false;
			// 
			// tss1
			// 
			this.tss1.Name = "tss1";
			this.tss1.Size = new System.Drawing.Size(165, 6);
			// 
			// it_file_open
			// 
			this.it_file_open.Name = "it_file_open";
			this.it_file_open.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_open.Size = new System.Drawing.Size(168, 20);
			this.it_file_open.Text = "Open ...";
			this.it_file_open.Click += new System.EventHandler(this.file_click_open);
			// 
			// it_file_opendatazip
			// 
			this.it_file_opendatazip.Name = "it_file_opendatazip";
			this.it_file_opendatazip.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_opendatazip.Size = new System.Drawing.Size(168, 20);
			this.it_file_opendatazip.Text = "Open nwn2 data ...";
			this.it_file_opendatazip.Click += new System.EventHandler(this.file_click_opendatazip);
			// 
			// it_file_save
			// 
			this.it_file_save.Name = "it_file_save";
			this.it_file_save.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_save.ShortcutKeyDisplayString = "[Ctrl+s]";
			this.it_file_save.Size = new System.Drawing.Size(168, 20);
			this.it_file_save.Text = "Save";
			this.it_file_save.Click += new System.EventHandler(this.file_click_save);
			// 
			// it_file_saveas
			// 
			this.it_file_saveas.Name = "it_file_saveas";
			this.it_file_saveas.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_saveas.Size = new System.Drawing.Size(168, 20);
			this.it_file_saveas.Text = "Save as ...";
			this.it_file_saveas.Click += new System.EventHandler(this.file_click_saveas);
			// 
			// it_file_close
			// 
			this.it_file_close.Name = "it_file_close";
			this.it_file_close.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_close.Size = new System.Drawing.Size(168, 20);
			this.it_file_close.Text = "Close";
			this.it_file_close.Click += new System.EventHandler(this.file_click_close);
			// 
			// tss2
			// 
			this.tss2.Name = "tss2";
			this.tss2.Size = new System.Drawing.Size(165, 6);
			// 
			// it_file_quit
			// 
			this.it_file_quit.Name = "it_file_quit";
			this.it_file_quit.Padding = new System.Windows.Forms.Padding(0);
			this.it_file_quit.Size = new System.Drawing.Size(168, 20);
			this.it_file_quit.Text = "Quit";
			this.it_file_quit.Click += new System.EventHandler(this.file_click_quit);
			// 
			// it_outputformat
			// 
			this.it_outputformat.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.it_outputformat_10,
			this.it_outputformat_11,
			this.tss3,
			this.it_outputformat_standard,
			this.it_outputformat_extended});
			this.it_outputformat.Name = "it_outputformat";
			this.it_outputformat.Padding = new System.Windows.Forms.Padding(0);
			this.it_outputformat.Size = new System.Drawing.Size(84, 24);
			this.it_outputformat.Text = "Output Format";
			this.it_outputformat.DropDownOpening += new System.EventHandler(this.outputformat_dropdownopening);
			// 
			// it_outputformat_10
			// 
			this.it_outputformat_10.Name = "it_outputformat_10";
			this.it_outputformat_10.Padding = new System.Windows.Forms.Padding(0);
			this.it_outputformat_10.Size = new System.Drawing.Size(207, 20);
			this.it_outputformat_10.Text = "SSF 1.0  (16 char resrefs)";
			this.it_outputformat_10.Click += new System.EventHandler(this.outputformat_click_10);
			// 
			// it_outputformat_11
			// 
			this.it_outputformat_11.Name = "it_outputformat_11";
			this.it_outputformat_11.Padding = new System.Windows.Forms.Padding(0);
			this.it_outputformat_11.Size = new System.Drawing.Size(207, 20);
			this.it_outputformat_11.Text = "SSF 1.1  (32 char resrefs)";
			this.it_outputformat_11.Click += new System.EventHandler(this.outputformat_click_11);
			// 
			// tss3
			// 
			this.tss3.Name = "tss3";
			this.tss3.Size = new System.Drawing.Size(204, 6);
			// 
			// it_outputformat_standard
			// 
			this.it_outputformat_standard.Name = "it_outputformat_standard";
			this.it_outputformat_standard.Padding = new System.Windows.Forms.Padding(0);
			this.it_outputformat_standard.Size = new System.Drawing.Size(207, 20);
			this.it_outputformat_standard.Text = "standard  (49 voices)";
			this.it_outputformat_standard.Click += new System.EventHandler(this.outputformat_click_standard);
			// 
			// it_outputformat_extended
			// 
			this.it_outputformat_extended.Name = "it_outputformat_extended";
			this.it_outputformat_extended.Padding = new System.Windows.Forms.Padding(0);
			this.it_outputformat_extended.Size = new System.Drawing.Size(207, 20);
			this.it_outputformat_extended.Text = "extended  (51 voices)";
			this.it_outputformat_extended.Click += new System.EventHandler(this.outputformat_click_extended);
			// 
			// it_talktable
			// 
			this.it_talktable.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.it_talktable_load});
			this.it_talktable.Name = "it_talktable";
			this.it_talktable.Padding = new System.Windows.Forms.Padding(0);
			this.it_talktable.Size = new System.Drawing.Size(61, 24);
			this.it_talktable.Text = "TalkTable";
			// 
			// it_talktable_load
			// 
			this.it_talktable_load.Name = "it_talktable_load";
			this.it_talktable_load.Padding = new System.Windows.Forms.Padding(0);
			this.it_talktable_load.Size = new System.Drawing.Size(154, 20);
			this.it_talktable_load.Text = "Load Talkfile ...";
			this.it_talktable_load.Click += new System.EventHandler(this.talktable_click_load);
			// 
			// la_about
			// 
			this.la_about.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.la_about.Margin = new System.Windows.Forms.Padding(0);
			this.la_about.Name = "la_about";
			this.la_about.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
			this.la_about.Size = new System.Drawing.Size(40, 24);
			this.la_about.Text = "about";
			// 
			// tc_pages
			// 
			this.tc_pages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tc_pages.Location = new System.Drawing.Point(0, 24);
			this.tc_pages.Margin = new System.Windows.Forms.Padding(0);
			this.tc_pages.Name = "tc_pages";
			this.tc_pages.Padding = new System.Drawing.Point(5, 2);
			this.tc_pages.SelectedIndex = 0;
			this.tc_pages.Size = new System.Drawing.Size(742, 428);
			this.tc_pages.TabIndex = 1;
			this.tc_pages.SelectedIndexChanged += new System.EventHandler(this.tc_selectedindexchanged);
			// 
			// ss_bot
			// 
			this.ss_bot.Font = new System.Drawing.Font("Consolas", 9F);
			this.ss_bot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tssl_file,
			this.tssl_ver,
			this.tssl_extended,
			this.tssl_pfe});
			this.ss_bot.Location = new System.Drawing.Point(0, 452);
			this.ss_bot.Name = "ss_bot";
			this.ss_bot.Size = new System.Drawing.Size(742, 22);
			this.ss_bot.TabIndex = 2;
			// 
			// tssl_file
			// 
			this.tssl_file.AutoSize = false;
			this.tssl_file.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.tssl_file.Name = "tssl_file";
			this.tssl_file.Size = new System.Drawing.Size(45, 22);
			this.tssl_file.Text = "file";
			this.tssl_file.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tssl_ver
			// 
			this.tssl_ver.AutoSize = false;
			this.tssl_ver.Margin = new System.Windows.Forms.Padding(0);
			this.tssl_ver.Name = "tssl_ver";
			this.tssl_ver.Size = new System.Drawing.Size(65, 22);
			this.tssl_ver.Text = "ver";
			this.tssl_ver.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tssl_extended
			// 
			this.tssl_extended.AutoSize = false;
			this.tssl_extended.Margin = new System.Windows.Forms.Padding(0);
			this.tssl_extended.Name = "tssl_extended";
			this.tssl_extended.Size = new System.Drawing.Size(75, 22);
			this.tssl_extended.Text = "extended";
			this.tssl_extended.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tssl_pfe
			// 
			this.tssl_pfe.Margin = new System.Windows.Forms.Padding(0);
			this.tssl_pfe.Name = "tssl_pfe";
			this.tssl_pfe.Size = new System.Drawing.Size(539, 22);
			this.tssl_pfe.Spring = true;
			this.tssl_pfe.Text = "pfe";
			this.tssl_pfe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Chatter
			// 
			this.ClientSize = new System.Drawing.Size(742, 474);
			this.Controls.Add(this.tc_pages);
			this.Controls.Add(this.menubar);
			this.Controls.Add(this.ss_bot);
			this.Font = new System.Drawing.Font("Comic Sans MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = global::nwn2_Chatter.Properties.Resource1.chatter_icon;
			this.KeyPreview = true;
			this.MainMenuStrip = this.menubar;
			this.Name = "Chatter";
			this.Text = "Chatter";
			this.menubar.ResumeLayout(false);
			this.menubar.PerformLayout();
			this.ss_bot.ResumeLayout(false);
			this.ss_bot.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
