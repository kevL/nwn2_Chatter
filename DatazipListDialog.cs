using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	/// <summary>
	/// A dialog that lists file-labels in the stock NwN2 /Data Zipfiles and
	/// allows the user to select one.
	/// </summary>
	sealed partial class DatazipListDialog
		: Form
	{
		#region Fields (static)
		static string _filter = String.Empty;

		const int marginhori_button_outer =  6;
		const int marginhori_button_inner = 10;
		const int marginvert_list = 4;
		const int marginvert_butt = 6;

		static int _x = Int32.MinValue, _y,_w,_h;
		#endregion Fields (static)


		#region Fields
		/// <summary>
		/// File+extension of the currently opened Zipfile.
		/// </summary>
		string _zipfe;

		/// <summary>
		/// The cache of all file-labels in the currently opened Zipfile.
		/// </summary>
		readonly List<string> _filelist = new List<string>();

		/// <summary>
		/// The <c><see cref="ZipFile"/></c> that is currently loaded and being
		/// accessed.
		/// </summary>
		ZipFile _zipfile;

		/// <summary>
		/// The <c>FileStream</c> for <c><see cref="_zipfile"/></c> needs to
		/// remain open for the duration of this <c>DatazipListDialog</c>. So
		/// <c>Close()</c> it in
		/// <c><see cref="OnFormClosing()">OnFormClosing()</see></c>.
		/// </summary>
		FileStream _fs;

		/// <summary>
		/// <c>false</c> if this <c>DatazipListDialog</c> is invoked to open a
		/// SoundSetFile or <c>true</c> if to play/insert an audiofile.
		/// </summary>
		bool _isaudio;

		/// <summary>
		/// Tracks the top-id in the file-list.
		/// </summary>
		int _tid = -1;

		readonly Timer _t1 = new Timer();
		#endregion Fields


		#region cTor
		/// <summary>
		/// Instantiates this <c>DatazipListDialog</c>.
		/// </summary>
		/// <param name="pfe"></param>
		/// <param name="voiceid"></param>
		internal DatazipListDialog(string pfe, int voiceid = -1)
		{
			InitializeComponent();

			if (_isaudio = (voiceid != -1))
				bu_Play.Text = Chatter.Voices[voiceid];

			int wMin = bu_Load.Width + bu_Accept.Width + bu_Cancel.Width
					 + marginhori_button_outer + marginhori_button_inner
					 + Width - ClientSize.Width;
			int hMin = la_Filter.Height + lb_List.ItemHeight * 3 + bu_Load.Height
					 + marginvert_list + marginvert_butt
					 + Height - ClientSize.Height;
			MinimumSize = new Size(wMin, hMin);

			if (_x != Int32.MinValue)
			{
				StartPosition = FormStartPosition.Manual;
				Location   = new Point(_x,_y);
				ClientSize = new Size (_w,_h);
			}

			tb_Filter.BackColor = Color.BlanchedAlmond;
			lb_List  .BackColor = Color.AliceBlue;


			_fs = new FileStream(pfe, FileMode.Open, FileAccess.Read); // TODO: Exception handling <-
			_zipfile = new ZipFile(_fs);

			ZipEntry[] zipentries = _zipfile.GetEntries();
			foreach (var zipentry in zipentries)
			{
				string label = Path.GetFileName(zipentry.Label);
				if (label.Length != 0 && !_filelist.Contains(label))
				{
					_filelist.Add(label);
					lb_List.Items.Add(label);
				}
			}

			Text = _zipfe = Path.GetFileName(pfe);

			tb_Filter.Text = _filter;
			bu_Cancel.Select();

			_t1.Tick += t1_tick;
			_t1.Interval = 100;
			_t1.Start();
		}
		#endregion cTor


		#region Handlers (override)
		/// <summary>
		/// Overrides the <c>FormClosing</c> handler.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_x = Math.Max(0, Location.X);
			_y = Math.Max(0, Location.Y);
			_w = ClientSize.Width;
			_h = ClientSize.Height;

			_t1.Dispose();
			_fs.Close();
			_zipfile.Dispose();

			base.OnFormClosing(e);
		}

		/// <summary>
		/// Overrides the <c>Resize</c> handler. Restores <c>List.TopIndex</c>.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnResize(EventArgs e)
		{
			if (WindowState != FormWindowState.Minimized)
			{
				lb_List.BeginUpdate();
				base.OnResize(e);

				if (_tid != -1) lb_List.TopIndex = _tid;
				lb_List.EndUpdate();
			}
			else
				base.OnResize(e);
		}
		#endregion Handlers (override)


		#region Handlers
		/// <summary>
		/// Determines if the Accept and/or Play buttons should be enabled.
		/// </summary>
		/// <param name="sender"><c><see cref="lb_List"/></c></param>
		/// <param name="e"></param>
		void selectedindexchanged_list(object sender, EventArgs e)
		{
			if (lb_List.SelectedIndex != -1)
			{
				string f = GetSelectedFile();
				f = f.Substring(f.Length - 4).ToUpperInvariant();

				bu_Accept.Enabled = (!_isaudio && f == ".SSF") || (_isaudio && f == ".WAV");
				bu_Play  .Enabled = f == ".WAV";
			}
			else
			{
				bu_Accept.Enabled =
				bu_Play  .Enabled = false;
			}
		}

		/// <summary>
		/// Accepts <c>SelectedItem</c> in <c><see cref="lb_List"/></c> if
		/// valid.
		/// </summary>
		/// <param name="sender"><c><see cref="lb_List"/></c></param>
		/// <param name="e"></param>
		void doubleclick_list(object sender, EventArgs e)
		{
			if (bu_Accept.Enabled)
				DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Updates the displayed file-list wrt <c><see cref="_filter"/></c>.
		/// </summary>
		/// <param name="sender"><c><see cref="tb_Filter"/></c></param>
		/// <param name="e"></param>
		void textchanged_filter(object sender, EventArgs e)
		{
			lb_List.BeginUpdate();

			_filter = tb_Filter.Text;

			lb_List.Items.Clear();
			foreach (var file in _filelist)
			{
				if (file.IndexOf(_filter, StringComparison.InvariantCultureIgnoreCase) != -1)
					lb_List.Items.Add(file);
			}

			_tid = -1;
			bu_Accept.Enabled =
			bu_Play  .Enabled = false;

			lb_List.EndUpdate();
		}

		/// <summary>
		/// Tracks the top-id.
		/// </summary>
		/// <param name="sender"><c><see cref="_t1"/></c></param>
		/// <param name="e"></param>
		void t1_tick(object sender, EventArgs e)
		{
			if (lb_List.Items.Count != 0)
				_tid = lb_List.TopIndex;
			else
				_tid = -1;
		}

		/// <summary>
		/// Invokes an <c>OpenFileDialog</c> for the user to open a different
		/// zip-archive.
		/// </summary>
		/// <param name="sender"><c><see cref="bu_Load"/></c></param>
		/// <param name="e"></param>
		void click_Load(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.AutoUpgradeEnabled = false;

				ofd.Title  = "Open nwn2 /data file";
				ofd.Filter = Chatter.FileFilter_ZIP;

//				ofd.RestoreDirectory = true; // allow tracking as last location

				string dir;
				if (Directory.Exists(Chatter._lastdatadirectory))
					dir = Chatter._lastdatadirectory;
				else
				{
					string dirT = Chatter.GetDatazipDirectory();
					if (dirT != null)
					{
						dir = dirT;
						ofd.RestoreDirectory = true; // no need to track this as last location; it's redetermined by GetDatazipDirectory()
					}
					else
						dir = Chatter.GetCurrentDirectory();
				}

				ofd.FileName = Path.Combine(dir, _zipfe);

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					Chatter._lastdatadirectory = Path.GetDirectoryName(ofd.FileName);

					lb_List.BeginUpdate();

					_filelist.Clear();
					lb_List.Items.Clear();

					_fs.Close();
					_fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);

					_zipfile.Dispose();
					_zipfile = new ZipFile(_fs);

					ZipEntry[] zipentries = _zipfile.GetEntries();
					foreach (var zipentry in zipentries)
					{
						string label = Path.GetFileName(zipentry.Label);
						if (label.Length != 0 && !_filelist.Contains(label))
						{
							_filelist.Add(label);

							if (label.IndexOf(_filter, StringComparison.InvariantCultureIgnoreCase) != -1)
								lb_List.Items.Add(label);
						}
					}

					Text = _zipfe = Path.GetFileName(ofd.FileName);

					_tid = -1;
					bu_Accept.Enabled =
					bu_Play  .Enabled = false;

					lb_List.EndUpdate();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="bu_Play"/></c></param>
		/// <param name="e"></param>
		void click_Play(object sender, EventArgs e)
		{
			ZipEntry zipentry = _zipfile.GetEntry(GetSelectedFile());

			byte[] bytes = Chatter.GetDecBytes(_zipfile, zipentry);

			string pfe = Path.Combine(Path.GetTempPath(), Path.GetFileName(zipentry.Label));
			File.WriteAllBytes(pfe, bytes);

			string audiofile = AudioConverter.deterwave(pfe); // this/these file/s will be deleted when Chatter closes
			if (audiofile != null)
			{
				using (var fs = new FileStream(audiofile, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var player = new System.Media.SoundPlayer(fs))
				{
					player.SoundLocation = audiofile;
					player.Play();
				}
			}

			if (File.Exists(pfe))
				File.Delete(pfe);
		}
		#endregion Handlers


		#region Methods
		/// <summary>
		/// Gets the <c>SelectedItem</c> in <c><see cref="lb_List"/></c> as a
		/// <c>string</c>.
		/// </summary>
		/// <returns></returns>
		internal string GetSelectedFile()
		{
			return lb_List.SelectedItem as string;
		}
		#endregion Methods
	}
}
