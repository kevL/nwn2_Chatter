﻿using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Microsoft.Win32;


namespace nwn2_Chatter
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Chatter
		: Form
	{
		#region Fields (static)
		/// <summary>
		/// This tracks the SoundSetFile specification in this <c>Chatter</c>.
		/// It is respected when saving a file instead of
		/// <c><see cref="ChatPageControl._ver">ChatPageControl._ver</see></c>.
		/// The latter is used to set <c>Output</c> when an SSF file is loaded
		/// and reset each time that that <c>ChatPageControl</c> becomes
		/// focused.
		/// </summary>
		internal static SsfFormat Output;

		/// <summary>
		/// This tracks the count of entries in the SoundSetFile (49 or 51) in
		/// this <c>Chatter</c>. It is respected when saving a file instead of
		/// <c><see cref="ChatPageControl._extended">ChatPageControl._extended</see></c>.
		/// The latter is used to set <c>Extended</c> when an SSF file is
		/// loaded and reset each time that that <c>ChatPageControl</c> becomes
		/// focused.
		/// </summary>
		internal static bool Extended;

		/// <summary>
		/// The array of standard voices (attack, battlecry, etc) in a
		/// SoundSetFile. <c><see cref="Extended"/></c> files have 51 entries
		/// while standard files have only 49. Generally speaking, SSF V1.0
		/// files for NwN have 49 and SSF V1.1 files for NwN2 might have 51 if
		/// the voice-set is for a player-companion. The 2 extra entries are
		/// Attack1 and Attack2 at ids #49 and #50.
		/// <br/><br/>
		/// Note however that both the 1.0 and 1.1 specifications technically
		/// support an arbitrary count of voices but in practice I've seen only
		/// 49 or 51 voices in SoundSetFiles and so that's only what Chatter
		/// supports. The game-engines will have their own way of doing things
		/// of course ...
		/// </summary>
		internal static string[] Voices;
		#endregion Fields (static)


		#region Fields
		/// <summary>
		/// Tracks whether to cancel saving and/or closing a
		/// <c><see cref="ChatPageControl"/></c> throughout
		/// <list type="bullet">
		/// <item><c><see cref="file_click_save()">file_click_save()</see></c></item>
		/// <item><c><see cref="file_click_close()">file_click_close()</see></c></item>
		/// </list>
		/// <br/><br/>
		/// because cancelling the save-routine might need to cancel closing.
		/// </summary>
		bool _cancel;
		#endregion Fields


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal Chatter()
		{
			InitializeComponent();
			InitializeObstructor();

			ss_bot.Renderer = new StripRenderer();

			tssl_file    .Text =
			tssl_ver     .Text =
			tssl_extended.Text =
			tssl_pfe     .Text = String.Empty;

			AssemblyName an = Assembly.GetExecutingAssembly().GetName();
			string ver = an.Version.Major + "."
					   + an.Version.Minor + "."
					   + an.Version.Build + "."
					   + an.Version.Revision;
#if DEBUG
			ver += ".d";
#endif
			la_about.Text = ver;

			CreateVoicesArray();
		}
		#endregion cTor


		#region Handlers (override)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (tc_pages.SelectedTab != null)
			{
				for (int i = 0; i != tc_pages.TabPages.Count; ++i)
				{
					if (ischanged(tc_pages.TabPages[i].Tag as ChatPageControl, i == tc_pages.SelectedIndex))
					{
						using (var ib = new Infobox(Infobox.Title_alert,
													"Changes detected. Are you sure you want to quit ...",
													null,
													InfoboxType.Warn,
													InfoboxButtons.CancelYes))
						{
							if (ib.ShowDialog(this) == DialogResult.Cancel)
								e.Cancel = true;

							break;
						}
					}
				}
			}
			base.OnFormClosing(e);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>Requires <c>KeyPreview</c> <c>true</c>.</remarks>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Control | Keys.S:
					if (tc_pages.SelectedTab != null)
					{
						var chatter = tc_pages.SelectedTab.Tag as ChatPageControl;
						if (!iscreated(chatter) && !chatter._datazipfile)
						{
							file_click_save(null, EventArgs.Empty);
						}
					}
					break;
			}

			base.OnKeyDown(e);
		}
		#endregion Handlers (override)


		#region Handlers
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="tc_pages"/></c></param>
		/// <param name="e"></param>
		void tc_selectedindexchanged(object sender, EventArgs e)
		{
			if (tc_pages.SelectedTab != null)
			{
				var chatter = tc_pages.SelectedTab.Tag as ChatPageControl;
				SetOutputFormat(chatter);
				SetStatusbarInfo(chatter);
			}
			else
			{
				pa_obstructor.Visible = true;

				it_outputformat_10      .Checked =
				it_outputformat_11      .Checked =
				it_outputformat_extended.Checked = false;

				tssl_file    .Text =
				tssl_ver     .Text =
				tssl_extended.Text =
				tssl_pfe     .Text = String.Empty;

				SetTitle();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void file_dropdownopening(object sender, EventArgs e)
		{
			if (tc_pages.SelectedTab != null)
			{
				var chatter = tc_pages.SelectedTab.Tag as ChatPageControl;

				it_file_save  .Enabled = !iscreated(chatter) && !chatter._datazipfile;
				it_file_saveas.Enabled =
				it_file_close .Enabled = true;
			}
			else
			{
				it_file_save  .Enabled =
				it_file_saveas.Enabled =
				it_file_close .Enabled = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">
		/// <list type="bullet">
		/// <item><c><see cref="it_file_create10"/></c></item>
		/// <item><c><see cref="it_file_create11"/></c></item>
		/// </list></param>
		/// <param name="e"></param>
		void file_click_create(object sender, EventArgs e)
		{
			string ver;
			if (sender == it_file_create10) ver = "ssf10";
			else                            ver = "ssf11"; // sender == it_file_create11

			var chatter = new ChatPageControl(this, ver);

			var page = new TabPage();
			page.Tag = chatter;
			tc_pages.TabPages.Add(page);

			page.Text = "created";

			page.Controls.Add(chatter);
			tc_pages.SelectedTab = page;

			SetOutputFormat(chatter);
			SetStatusbarInfo(chatter);

			chatter.InitScroll();
			pa_obstructor.Visible = false;

			chatter.Select();
		}

		string _lastopendirectory;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="it_file_open"/></c></param>
		/// <param name="e"></param>
		void file_click_open(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.AutoUpgradeEnabled = false;

				ofd.Title  = "Open SSF file";
				ofd.Filter = "SSF files (*.SSF)|*.SSF|All files (*.*)|*.*";

				string dir;
				if (Directory.Exists(_lastopendirectory))
				{
					dir = _lastopendirectory;
					ofd.RestoreDirectory = true;
				}
				else
					dir = GetCurrentDirectory();

				ofd.FileName = Path.Combine(dir, "*.SSF");

				if (ofd.ShowDialog() == DialogResult.OK
					&& !isloaded(ofd.FileName))
				{
					_lastopendirectory = Path.GetDirectoryName(ofd.FileName);

					CreateChatTab(new ChatPageControl(this, ofd.FileName));
				}
			}
		}

		internal static string _lastdatadirectory;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="it_file_opendatazip"/></c></param>
		/// <param name="e"></param>
		/// <remarks>This is waranteed only for the Zipfiles in the NwN2 /Data
		/// folder.</remarks>
		void file_click_opendatazip(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.AutoUpgradeEnabled = false;

				ofd.Title  = "Open NwN2 data/zip file";
				ofd.Filter = "ZIP files (*.ZIP)|*.ZIP|All files (*.*)|*.*";

				string dir = null;
				if (Directory.Exists(_lastdatadirectory))
				{
					dir = _lastdatadirectory;
					ofd.RestoreDirectory = true;
				}
				else
				{
					using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Obsidian\\NWN 2\\Neverwinter", false))
					{
						if (key != null)
						{
							object val = key.GetValue("Path");
							if (val != null)
							{
								dir = val as string; // -> "C:\Neverwinter Nights 2"
								if (Directory.Exists(dir))
								{
									dir = Path.Combine(dir, "Data");
									ofd.RestoreDirectory = true;
								}
							}
						}
					}

					if (!Directory.Exists(dir))
						dir = GetCurrentDirectory();
				}

				ofd.FileName = Path.Combine(dir, "*.ZIP");

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					_lastdatadirectory = Path.GetDirectoryName(ofd.FileName);

					string label = String.Empty;
					using (var dzld = new DataZipListDialog(ofd.FileName, false))
					{
						if (dzld.ShowDialog(this) == DialogResult.OK)
							label = dzld.GetSelectedFile();
					}

					if (label.Length != 0) // TODO: don't decompress the zipfile twice (done above AND done below)
					{
						using (var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
						using (var zipfile = new ZipFile(fs))
						{
							ZipEntry zipentry = zipfile.GetEntry(label);
							if (zipentry != null)
							{
								label = Path.Combine(ofd.FileName, label); // note that's the pfe of the zipfile + file.SFF
								if (!isloaded(label))
								{
									CreateChatTab(new ChatPageControl(this, label, GetDecBytes(zipfile, zipentry)));
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">
		/// <list type="bullet">
		/// <item><c><see cref="it_file_save"/></c></item>
		/// <item><c><see cref="it_file_saveas"/></c></item>
		/// <item><c><see cref="it_file_close"/></c></item>
		/// <item><c>null</c> - <c><see cref="OnKeyDown()">OnKeyDown()</see> [Ctrl+s]</c></item>
		/// </list></param>
		/// <param name="e"></param>
		/// <remarks>The it is disabled if there is no
		/// <c><see cref="ChatPageControl"/></c>.</remarks>
		void file_click_save(object sender, EventArgs e)
		{
			//if (sender != null) logfile.Log("Chatter.file_click_save() sender= " + ((sender as ToolStripMenuItem).Name));
			//else                logfile.Log("Chatter.file_click_save() sender NULL");

			_cancel = false;

			var chatter = tc_pages.SelectedTab.Tag as ChatPageControl;
			if (chatter._ver != Output)
			{
				using (var ib = new Infobox(Infobox.Title_alert,
											"The spec of the loaded file is not the same as the Output Format."
												+ " Are you sure you want to proceed ...",
											null,
											InfoboxType.Warn,
											InfoboxButtons.CancelYes))
				{
					if (ib.ShowDialog(this) == DialogResult.Cancel)
						_cancel = true;
				}
			}

			if (!_cancel)
			{
				if (!CheckResrefLengths(chatter._resrefs))
				{
					using (var ib = new Infobox(Infobox.Title_alert,
												"Detected resrefs that are longer than the spec allows."
													+ " Are you sure you want to proceed ...",
												null,
												InfoboxType.Warn,
												InfoboxButtons.CancelYes))
					{
						if (ib.ShowDialog(this) == DialogResult.Cancel)
							_cancel = true;
					}
				}

				if (!_cancel)
				{
					bool update = false;

					if (chatter._extended != Extended)
					{
						string alert;
						if (chatter._extended)
							alert = "The Attack2 and Attack3 voices will be clipped from the end of the file.";
						else
							alert = "Attack2 and Attack3 voices will be added to the end of the file.";

						using (var ib = new Infobox(Infobox.Title_alert,
													alert + " Are you sure you want to proceed ...",
													null,
													InfoboxType.Warn,
													InfoboxButtons.CancelYes))
						{
							if (ib.ShowDialog(this) == DialogResult.Cancel)
								_cancel = true;
							else if (sender == it_file_save)
								update = true;
						}
					}

					if (!_cancel)
					{
						//logfile.Log("WriteSoundsetFile()");
						//logfile.Log(". pfe= " + chatter._pfe);
						SoundsetFileService.WriteSoundsetFile(chatter);

						chatter.Changed = false;

						chatter._ver = Output;
						chatter._extended = Extended;

						SetTitle();
						SetStatusbarInfo(chatter);

						if (update) chatter.extend();
					}
				}
			}
		}

		string _lastsavedirectory;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender">
		/// <list type="button">
		/// <item><c><see cref="it_file_saveas"/></c></item>
		/// <item><c><see cref="it_file_close"/></c></item>
		/// </list></param>
		/// <param name="e"></param>
		/// <remarks>The it is disabled if there is no
		/// <c><see cref="ChatPageControl"/></c>.</remarks>
		void file_click_saveas(object sender, EventArgs e)
		{
			using (var sfd = new SaveFileDialog())
			{
				sfd.AutoUpgradeEnabled = false;

				sfd.Title  = "Save as ...";
				sfd.Filter = "SSF files (*.SSF)|*.SSF|All files (*.*)|*.*";

				var chatter = (tc_pages.SelectedTab.Tag as ChatPageControl);

				string dir, fe;
				if (iscreated(chatter)) fe = "*.SSF";
				else                    fe = Path.GetFileName(chatter._pfe);


				if (Directory.Exists(_lastsavedirectory))
				{
					dir = _lastsavedirectory;
					sfd.RestoreDirectory = true;
				}
				else if (!iscreated(chatter) && !chatter._datazipfile
					&& Directory.Exists(Path.GetDirectoryName(chatter._pfe)))
				{
					dir = Path.GetDirectoryName(chatter._pfe);
					sfd.RestoreDirectory = true;
				}
				else
					dir = GetCurrentDirectory();

				sfd.FileName = Path.Combine(dir, fe);

				if (sfd.ShowDialog() == DialogResult.OK)
				{
					_lastsavedirectory = Path.GetDirectoryName(sfd.FileName);

					tc_pages.SelectedTab.Text = Path.GetFileName(chatter._pfe = sfd.FileName);
					chatter._datazipfile = false;

					file_click_save(sender, e);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="it_file_close"/></c></param>
		/// <param name="e"></param>
		/// <remarks>The it is disabled if there is no
		/// <c><see cref="ChatPageControl"/></c>.</remarks>
		void file_click_close(object sender, EventArgs e)
		{
			_cancel = false;

			var chatter = tc_pages.SelectedTab.Tag as ChatPageControl;
			if (ischanged(chatter))
			{
				using (var ib = new Infobox(Infobox.Title_alert,
											"The data has changed. Do you wish to save the file ...",
											null,
											InfoboxType.Warn,
											InfoboxButtons.CancelYesNo))
				{
					switch (ib.ShowDialog(this))
					{
						case DialogResult.Cancel:	// cancel
							_cancel = true;
							break;

						case DialogResult.OK:		// yes
							if (iscreated(chatter) || chatter._datazipfile)
							{
								file_click_saveas(sender, e);
							}
							else
								file_click_save(sender, e); // -> set '_cancel'
							break;
					}
				}
			}

			if (!_cancel)
			{
				tc_pages.SelectedTab.Controls.Remove(chatter);
				tc_pages.TabPages.Remove(tc_pages.SelectedTab);

				chatter.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="it_file_quit"/></c></param>
		/// <param name="e"></param>
		void file_click_quit(object sender, EventArgs e)
		{
			Close();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void outputformat_dropdownopening(object sender, EventArgs e)
		{
			it_outputformat_10      .Enabled =
			it_outputformat_11      .Enabled =
			it_outputformat_standard.Enabled =
			it_outputformat_extended.Enabled = (tc_pages.SelectedTab != null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="it_outputformat_10"/></c></param>
		/// <param name="e"></param>
		/// <remarks>The it is disabled if there is no
		/// <c><see cref="ChatPageControl"/></c>.</remarks>
		void outputformat_click_10(object sender, EventArgs e)
		{
			Output = SsfFormat.ssf10;
			it_outputformat_10.Checked = true;
			it_outputformat_11.Checked = false;

			SetTitle();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="it_outputformat_11"/></c></param>
		/// <param name="e"></param>
		/// <remarks>The it is disabled if there is no
		/// <c><see cref="ChatPageControl"/></c>.</remarks>
		void outputformat_click_11(object sender, EventArgs e)
		{
			Output = SsfFormat.ssf11;
			it_outputformat_10.Checked = false;
			it_outputformat_11.Checked = true;

			SetTitle();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="it_outputformat_standard"/></c></param>
		/// <param name="e"></param>
		/// <remarks>The it is disabled if there is no
		/// <c><see cref="ChatPageControl"/></c>.</remarks>
		void outputformat_click_standard(object sender, EventArgs e)
		{
			Extended = false;
			it_outputformat_standard.Checked = true;
			it_outputformat_extended.Checked = false;

			SetTitle();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"><c><see cref="it_outputformat_extended"/></c></param>
		/// <param name="e"></param>
		/// <remarks>The it is disabled if there is no
		/// <c><see cref="ChatPageControl"/></c>.</remarks>
		void outputformat_click_extended(object sender, EventArgs e)
		{
			Extended = true;
			it_outputformat_standard.Checked = false;
			it_outputformat_extended.Checked = true;

			SetTitle();
		}


		string _lasttlkdirectory;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void talktable_click_load(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.AutoUpgradeEnabled = false;

				ofd.Title  = "Open TLK file";
				ofd.Filter = "TLK files (*.TLK)|*.TLK|All files (*.*)|*.*";

				string dir;
				if (Directory.Exists(_lasttlkdirectory))
				{
					dir = _lasttlkdirectory;
					ofd.RestoreDirectory = true;
				}
				else
					dir = GetCurrentDirectory();

				ofd.FileName = Path.Combine(dir, "*.TLK");

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					_lasttlkdirectory = Path.GetDirectoryName(ofd.FileName);

					if (TalkReader.Load(ofd.FileName))
						it_talktable_load.Checked = true;
					else
						it_talktable_load.Checked = false;

					if (tc_pages.SelectedTab != null)
						(tc_pages.SelectedTab.Tag as ChatPageControl).Invalidate();
				}
			}
		}
		#endregion Handlers


		#region Methods (static)
		/// <summary>
		/// Creates the array of standard voice-strings in all SoundSetFiles.
		/// </summary>
		static void CreateVoicesArray()
		{
			Voices = new string[51];

			Voices[ 0] = "Attack";
			Voices[ 1] = "Battlecry 1";
			Voices[ 2] = "Battlecry 2";
			Voices[ 3] = "Battlecry 3";
			Voices[ 4] = "Heal me";
			Voices[ 5] = "Help";
			Voices[ 6] = "Enemies sighted";
			Voices[ 7] = "Flee";
			Voices[ 8] = "Taunt";
			Voices[ 9] = "Guard me";
			Voices[10] = "Hold";
			Voices[11] = "Attack Grunt 1";
			Voices[12] = "Attack Grunt 2";
			Voices[13] = "Attack Grunt 3";
			Voices[14] = "Pain Grunt 1";
			Voices[15] = "Pain Grunt 2";
			Voices[16] = "Pain Grunt 3";
			Voices[17] = "Near death";
			Voices[18] = "Death";
			Voices[19] = "Poisoned";
			Voices[20] = "Spell failed";
			Voices[21] = "Weapon ineffective";
			Voices[22] = "Follow me";
			Voices[23] = "Look here";
			Voices[24] = "Group party";
			Voices[25] = "Move over";
			Voices[26] = "Pick lock";
			Voices[27] = "Search";
			Voices[28] = "Go stealthy";
			Voices[29] = "Can do";
			Voices[30] = "Cannot do";
			Voices[31] = "Task complete";
			Voices[32] = "Encumbered";
			Voices[33] = "Selected";
			Voices[34] = "Hello";
			Voices[35] = "Yes";
			Voices[36] = "No";
			Voices[37] = "Stop";
			Voices[38] = "Rest";
			Voices[39] = "Bored";
			Voices[40] = "Goodbye";
			Voices[41] = "Thank you";
			Voices[42] = "Laugh";
			Voices[43] = "Cuss";
			Voices[44] = "Cheer";
			Voices[45] = "Something to say";
			Voices[46] = "Good idea";
			Voices[47] = "Bad idea";
			Voices[48] = "Threaten";

			Voices[49] = "Attack 2"; // found only in nwn2 ->
			Voices[50] = "Attack 3";
		}

		/// <summary>
		/// Helper for
		/// <c><see cref="file_click_save()">file_click_save()</see></c>. Checks
		/// if any resref-lengths have too many characters for the currently
		/// specified <c><see cref="Output"/></c> format.
		/// </summary>
		/// <param name="resrefs"></param>
		/// <returns><c>true</c> if all resref-lengths are okay</returns>
		static bool CheckResrefLengths(string[] resrefs)
		{
			int len;
			if (Output == SsfFormat.ssf10) len = 16;
			else                           len = 32; // Output == SsfFormat.ssf11

			for (int i = 0; i != resrefs.Length; ++i)
			{
				if (resrefs[i].Length > len)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Gets initial directory for <c>OpenFileDialog</c>.
		/// </summary>
		/// <returns>few really know ...</returns>
		/// <remarks>When the app starts the CurrentDirectory is set to
		/// <c>Application.StartupPath</c> by .NET. But invoking an
		/// <c>OpenFileDialog</c> with that as its <c>InitialDirectory</c> is
		/// bogus - so if the directory is left blank the dialog finds one of
		/// the ComDlg32 MRUs in the registry and uses that instead.
		/// <br/><br/>
		/// Note that <c>OpenFileDialog.InitialDirectory</c> does not always
		/// work - so combine the desired path with the desired filename and
		/// assign it to <c>OpenFileDialog.FileName</c> before calling
		/// <c>ShowDialog()</c>.</remarks>
		internal static string GetCurrentDirectory()
		{
			string dir = Directory.GetCurrentDirectory();
			if (dir != Application.StartupPath)
				return dir;

			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//			return String.Empty;
		}

		/// <summary>
		/// Gets the decompressed data of a zipped-file entry as an array of
		/// <c>bytes</c> given a <c><see cref="ZipFile"/></c> and 
		/// <c><see cref="ZipEntry"/></c>.
		/// </summary>
		/// <param name="zf"><c>ZipFile</c></param>
		/// <param name="ze"><c>ZipEntry</c></param>
		internal static byte[] GetDecBytes(ZipFile zf, ZipEntry ze)
		{
			byte[] bytes = null;

			using (Stream s0 = zf.GetInputStream(ze))
			using (var s1 = new MemoryStream())
			{
				CopyStream(s0,s1);
				bytes = s1.ToArray();
			}
			return bytes;
		}

		/// <summary>
		/// Copies a <c>Stream</c> from <paramref name="input"/> to
		/// <paramref name="output"/>.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		static void CopyStream(Stream input, Stream output)
		{
			var buffer = new byte[4096]; //32768 or 81920
			int read;
			while ((read = input.Read(buffer, 0, buffer.Length)) != 0)
				output.Write(buffer, 0, read);
		}
		#endregion Methods (static)


		#region Methods
		/// <summary>
		/// Creates a <c>TabPage</c> for a specified
		/// <c><see cref="ChatPageControl"/></c>. The <c>TabPage</c> will then
		/// be selected and stuff gets set up for this <c>Chatter</c> to reflect
		/// current state.
		/// </summary>
		/// <param name="chatter"></param>
		void CreateChatTab(ChatPageControl chatter)
		{
			if (!chatter._fail && chatter._ver != SsfFormat.non)
			{
				var page = new TabPage();
				page.Tag = chatter;
				tc_pages.TabPages.Add(page);

				page.Text = Path.GetFileNameWithoutExtension(chatter._pfe);

				page.Controls.Add(chatter);
				tc_pages.SelectedTab = page;

				SetOutputFormat(chatter);
				SetStatusbarInfo(chatter);

				chatter.InitScroll();
				pa_obstructor.Visible = false;

				chatter.Select();
			}
			else
			{
				chatter.Dispose();

				using (var ib = new Infobox(Infobox.Title_error,
											"That is not a valid SSF file.",
											null,
											InfoboxType.Error))
				{
					ib.ShowDialog(this);
				}
			}
		}

		/// <summary>
		/// Checks if a specified <c><see cref="ChatPageControl"/></c> has a
		/// valid
		/// <c><see cref="ChatPageControl._pfe">ChatPageControl._pfe</see></c>
		/// to which it can be saved.
		/// </summary>
		/// <param name="chatter"></param>
		/// <returns><c>true</c> if
		/// <c><see cref="ChatPageControl._pfe">ChatPageControl._pfe</see></c>
		/// has not been written to file</returns>
		bool iscreated(ChatPageControl chatter)
		{
			return chatter._pfe == "ssf10"
				|| chatter._pfe == "ssf11";
		}

		/// <summary>
		/// Prints info about the SoundSetFile to the statusbar.
		/// </summary>
		/// <paramref name="chatter"></paramref>
		/// <remarks>This is the info in the currently active
		/// <c><see cref="ChatPageControl"/></c> - not the OutputFormat on the
		/// MainMenu.</remarks>
		void SetStatusbarInfo(ChatPageControl chatter)
		{
			tssl_file.Text = "file";

			if (chatter._ver == SsfFormat.ssf10)
				tssl_ver.Text = "SSF 1.0";
			else if (chatter._ver == SsfFormat.ssf11)
				tssl_ver.Text = "SSF 1.1";

			if (chatter._extended)
				tssl_extended.Text = "extended";
			else
				tssl_extended.Text = "standard";

			if (iscreated(chatter))
				tssl_pfe.Text = String.Empty;
			else
				tssl_pfe.Text = chatter._pfe;
		}

		/// <summary>
		/// Sets <c><see cref="Output"/></c> and <c><see cref="Extended"/></c>
		/// as well as this <c>Chatter's</c> titletext when a SoundSetFile loads
		/// or a <c><see cref="ChatPageControl">ChatPageControl's</see></c>
		/// tabpage becomes activated.
		/// </summary>
		/// <param name="chatter"></param>
		void SetOutputFormat(ChatPageControl chatter)
		{
			switch (chatter._ver)
			{
				case SsfFormat.ssf10:
					Output = SsfFormat.ssf10;
					it_outputformat_10.Checked = true;
					it_outputformat_11.Checked = false;
					break;

				case SsfFormat.ssf11:
					Output = SsfFormat.ssf11;
					it_outputformat_10.Checked = false;
					it_outputformat_11.Checked = true;
					break;
			}

			Extended = chatter._extended;
			it_outputformat_standard.Checked = !Extended;
			it_outputformat_extended.Checked =  Extended;

			SetTitle();
		}

		/// <summary>
		/// Sets the titlebar caption.
		/// </summary>
		internal void SetTitle()
		{
			string text = "Chatter";
			if (tc_pages.SelectedTab != null)
			{
				text += " - output SSF " + (Output == SsfFormat.ssf10 ? "1.0" : "1.1") + " spec "
					  + (Extended ? "extended" : "standard");

				if (ischanged(tc_pages.SelectedTab.Tag as ChatPageControl))
					text += " *";
			}
			Text = text;
		}

		/// <summary>
		/// Checks for
		/// <c><see cref="ChatPageControl.Changed">ChatPageControl.Changed</see></c>
		/// etc.
		/// </summary>
		/// <param name="chatter"></param>
		/// <param name="selected"><c>true</c> to match
		/// <c><see cref="Output"/></c> and <c><see cref="Extended"/></c> only
		/// to the currently selected <c>ChatPageControl</c> - default
		/// <c>true</c> but can be set <c>false</c> by
		/// <c><see cref="OnFormClosing()">OnFormClosing()</see></c></param>
		/// <returns><c>true</c> if <paramref name="chatter"/> is not considered
		/// to be changed</returns>
		bool ischanged(ChatPageControl chatter, bool selected = true)
		{
			return chatter.Changed
				|| chatter._datazipfile
				|| (selected
					&& (   chatter._ver      != Output
						|| chatter._extended != Extended));
		}

		/// <summary>
		/// Checks if a specified path-file-extension is already loaded. Selects
		/// the tabpage if so.
		/// </summary>
		/// <param name="pfe"></param>
		/// <returns><c>true</c> if the file is already loaded</returns>
		bool isloaded(string pfe)
		{
			ChatPageControl chatter;
			for (int i = 0; i != tc_pages.TabPages.Count; ++i)
			{
				chatter = tc_pages.TabPages[i].Tag as ChatPageControl;
				if (chatter._pfe == pfe)
				{
					tc_pages.SelectedTab = tc_pages.TabPages[i];
					return true;
				}
			}
			return false;
		}
		#endregion Methods
	}


	/// <summary>
	/// A global <c>enum</c> for the SoundSetFile specification.
	/// </summary>
	enum SsfFormat
	{
		/// <summary>
		/// no format specified
		/// </summary>
		non,

		/// <summary>
		/// SSF V1.0
		/// </summary>
		ssf10,

		/// <summary>
		/// SSF V1.1
		/// </summary>
		ssf11
	}
}
