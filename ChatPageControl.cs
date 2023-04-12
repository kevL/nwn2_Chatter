using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	/// <summary>
	/// This <c>Control</c> appears as a <c>TabPage</c> in
	/// <c><see cref="Chatter">Chatter's</see></c> <c>TabControl</c>. It
	/// contains all the data about a loaded SoundSetFile and is responsible for
	/// displaying and editing that data.
	/// </summary>
	sealed class ChatPageControl
		: Control
	{
		#region Fields (static)
		/// <summary>
		/// The fixed count of cols in this <c>ChatPageControl</c>.
		/// </summary>
		const int COLCOUNT = 4;

		/// <summary>
		/// The fixed width of the first col (voices).
		/// </summary>
		const int COLWIDTH0 = 150;

		/// <summary>
		/// The fixed width of the second col (resrefs).
		/// </summary>
		const int COLWIDTH1 = 230;

		/// <summary>
		/// The fixed width of the third col (strrefs). The fourth col extends
		/// to the right edge of the table (Dialog.Tlk text).
		/// </summary>
		const int COLWIDTH2 = 85;

		/// <summary>
		/// text indent in each cell
		/// </summary>
		const int INDENT = 4;

		/// <summary>
		/// The fixed height of rows.
		/// </summary>
		const int ROWHEIGHT = 20;
		#endregion Fields (static)


		#region Fields
		readonly Chatter _f;

		/// <summary>
		/// The path-file-extension of the file that's been loaded into this
		/// <c>ChatPageControl</c>.
		/// </summary>
		internal string _pfe;

		/// <summary>
		/// The SoundSetFile specification of the data loaded in this
		/// <c>ChatPageControl</c>.
		/// </summary>
		/// <seealso cref="Chatter.Output"><c>Chatter.Output</c></seealso>
		internal SsfFormat _ver;

		/// <summary>
		/// <c>true</c> if the data loaded in this <c>ChatPageControl</c> has
		/// an extended voice-set.
		/// </summary>
		/// <seealso cref="Chatter.Extended"><c>Chatter.Extended</c></seealso>
		internal bool _extended;

		/// <summary>
		/// The array of resrefs that have been loaded into this
		/// <c>ChatPageControl</c>.
		/// </summary>
		internal string[] _resrefs;

		/// <summary>
		/// The array of strrefs that have been loaded into this
		/// <c>ChatPageControl</c>.
		/// </summary>
		internal uint[] _strrefs;

		/// <summary>
		/// The count of rows/voice-entries in this <c>ChatPageControl</c>.
		/// </summary>
		int _rcount;

		/// <summary>
		/// The vertical fckin scrollbar. Get a grip ...
		/// </summary>
		readonly VScrollBar _scroller = new VScrollBar();

		/// <summary>
		/// Since a .NET scrollbar's <c>Maximum</c> value is not its maximum
		/// value calculate and store what it really is.
		/// </summary>
		int _vertical;

		/// <summary>
		/// This will be set <c>true</c> if a SoundSetFile fails to load.
		/// </summary>
		internal bool _fail;

		/// <summary>
		/// <c>true</c> if this <c>ChatPageControl</c> was created from a zipped
		/// file in the nwn2/data folder.
		/// </summary>
		internal bool _datazipfile;
		#endregion Fields


		#region Properties
		bool _changed;
		internal bool Changed
		{
			get { return _changed; }
			set
			{
				_changed = value;
				_f.SetTitle();
			}
		}
		#endregion Properties


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="f"></param>
		/// <param name="pfe">path-file-extension to load a SoundSetFile or
		/// "ssf10" to create a new ver 1.0 SoundSetFile or "ssf11" to create a
		/// new ver 1.1 SoundSetFile</param>
		/// <param name="bytes"></param>
		internal ChatPageControl(Chatter f, string pfe, byte[] bytes = null)
		{
			_f = f;
			_pfe = pfe;

			DoubleBuffered = true;
			Margin = new Padding(0);

			Dock      = DockStyle.Fill;
			BackColor = SystemColors.ControlDark;

			_scroller.Dock = DockStyle.Right;
			_scroller.LargeChange = ROWHEIGHT;
			_scroller.ValueChanged += valuechanged_scroller;
			Controls.Add(_scroller);

			if (_pfe == "ssf10")
			{
				Changed = true;

				_ver = SsfFormat.ssf10;
				_extended = false;

				_rcount = 49;
				_resrefs = new string[_rcount];
				_strrefs = new uint  [_rcount];

				for (int i = 0; i != _rcount; ++i)
				{
					_resrefs[i] = String.Empty;
					_strrefs[i] = 0xFFFFFFFF;
				}
			}
			else if (_pfe == "ssf11")
			{
				Changed = true;

				_ver = SsfFormat.ssf11;
				_extended = true;

				_rcount = 51;
				_resrefs = new string[_rcount];
				_strrefs = new uint  [_rcount];

				for (int i = 0; i != _rcount; ++i)
				{
					_resrefs[i] = String.Empty;
					_strrefs[i] = 0xFFFFFFFF;
				}
			}
			else
			{
				string ver = null;
				if (bytes != null)
				{
					Changed = _datazipfile = true;
					_rcount = SoundsetFileService.ReadSoundsetFile(bytes, ref _resrefs, ref _strrefs, ref ver);
				}
				else
					_rcount = SoundsetFileService.ReadSoundsetFile(_pfe, ref _resrefs, ref _strrefs, ref ver);

				if (_rcount != -1)
				{
					if      (ver == SoundsetFileService.Ver1) _ver = SsfFormat.ssf10;
					else if (ver == SoundsetFileService.Ver2) _ver = SsfFormat.ssf11;

					_extended = (_rcount > 49);
				}
				else
					_fail = true;
			}
		}
		#endregion cTor


		#region Handlers (override)
		int _r;
		bool _isresref, _isstrref;

		/// <summary>
		/// Since if an <c>OpenFileDialog</c> pops up under the mousecursor in a
		/// <c>MouseDown</c> event the dialog will fire a <c>MouseUp</c> event
		/// and open its contextmenu ... the <c>MouseUp</c> event needs to be
		/// used to invoke the <c>OpenFileDialog</c>. That prevents a
		/// contextmenu from showing in the dialog's <c>MouseUp</c> - or
		/// <c>MouseClick</c> or whatever it is. But then there's the problem
		/// that user can click down and move the cursor before releasing it for
		/// the <c>MouseUp</c> event ... so the
		/// <c><see cref="OnMouseDown()">OnMouseDown()</see></c> will track what
		/// slot was clicked down on, and that slot shall be checked against
		/// when <c><see cref="OnMouseUp()">OnMouseUp()</see></c> fires.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			_r = -1; _isresref = _isstrref = false;

			if ((ModifierKeys & (Keys.Alt | Keys.Shift)) == Keys.None)
			{
				switch (e.Button)
				{
					case MouseButtons.Left:
						if (ModifierKeys == Keys.Control)
						{
							if (e.X > COLWIDTH0 && e.X < COLWIDTH0 + COLWIDTH1)
							{
								int r = (e.Y + _scroller.Value) / ROWHEIGHT;
								if (r < _resrefs.Length)
								{
									_r = r;
									_isresref = true;;
								}
							}
						}
						break;

					case MouseButtons.Right:
						if (ModifierKeys == Keys.Control)
						{
							if (e.X > COLWIDTH0 && e.X < COLWIDTH0 + COLWIDTH1)
							{
								int r = (e.Y + _scroller.Value) / ROWHEIGHT;
								if (r < _resrefs.Length)
								{
									_r = r;
									_isresref = true;;
								}
							}
						}
						else
						{
							int r = (e.Y + _scroller.Value) / ROWHEIGHT;

							if (e.X > COLWIDTH0 && e.X < COLWIDTH0 + COLWIDTH1)
							{
								if (r < _resrefs.Length)
								{
									_r = r;
									_isresref = true;;
								}
							}
							else if (e.X > COLWIDTH0 + COLWIDTH1 && e.X < COLWIDTH0 + COLWIDTH1 + COLWIDTH2)
							{
								if (r < _strrefs.Length)
								{
									_r = r;
									_isstrref = true;;
								}
							}
						}
						break;
				}
			}
		}

		static string _lastplaydirectory;
		/// <summary>
		/// Overrides the <c>MouseUp</c> handler. Rightclick selects the table
		/// and instantiates an <c><see cref="Inputbox"/></c> iff a resref or
		/// strref is clicked. <c>[Ctrl]</c> + Leftclick invokes an
		/// <c>OpenFileDialog</c> for the user to play a soundfile.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			Select();

			if ((ModifierKeys & (Keys.Alt | Keys.Shift)) == Keys.None)
			{
				switch (e.Button)
				{
					case MouseButtons.Left:
						if (ModifierKeys == Keys.Control)
						{
							if (_isresref && e.X > COLWIDTH0 && e.X < COLWIDTH0 + COLWIDTH1)
							{
								int r = (e.Y + _scroller.Value) / ROWHEIGHT;
								if (r == _r && r < _resrefs.Length)
								{
									using (var ofd = new OpenFileDialog())
									{
										ofd.AutoUpgradeEnabled = false;

										ofd.Title  = "Play WAV file";
										ofd.Filter = "WAV files (*.WAV)|*.WAV|All files (*.*)|*.*";

										ofd.RestoreDirectory = true; // do not track this as last location

										string dir;
										if (Directory.Exists(_lastplaydirectory))
											dir = _lastplaydirectory;
										else
											dir = Chatter.GetCurrentDirectory();

										ofd.FileName = Path.Combine(dir, _resrefs[r] + ".WAV");

										if (ofd.ShowDialog() == DialogResult.OK)
										{
											_lastplaydirectory = Path.GetDirectoryName(ofd.FileName);

											string audiofile = AudioConverter.deterwave(ofd.FileName); // this/these file/s will be deleted when Chatter closes
											if (audiofile.Length != 0)
											{
												using (var fs = new FileStream(audiofile, FileMode.Open, FileAccess.Read, FileShare.Read))
												using (var player = new System.Media.SoundPlayer(fs))
												{
													player.SoundLocation = audiofile;
													player.Play();
												}
											}
										}
									}
								}
							}
						}
						break;

					case MouseButtons.Right:
						if (ModifierKeys == Keys.Control)
						{
							if (_isresref && e.X > COLWIDTH0 && e.X < COLWIDTH0 + COLWIDTH1)
							{
								int r = (e.Y + _scroller.Value) / ROWHEIGHT;
								if (r == _r && r < _resrefs.Length)
								{
									using (var ofd = new OpenFileDialog())
									{
										ofd.AutoUpgradeEnabled = false;

										ofd.Title  = "Open NwN2 data/zip file";
										ofd.Filter = "ZIP files (*.ZIP)|*.ZIP|All files (*.*)|*.*";

										ofd.RestoreDirectory = true; // do not track this as last location

										string dir;
										if (Directory.Exists(Chatter._lastdatadirectory))
											dir = Chatter._lastdatadirectory;
										else
										{
											string dirT = Chatter.GetDatazipDirectory();
											if (dirT != null)
												dir = dirT;
											else
												dir = Chatter.GetCurrentDirectory();
										}

										ofd.FileName = Path.Combine(dir, "*.ZIP");
						
										if (ofd.ShowDialog() == DialogResult.OK)
										{
											Chatter._lastdatadirectory = Path.GetDirectoryName(ofd.FileName);

											string label = String.Empty;
											using (var dzld = new DatazipListDialog(ofd.FileName, r))
											{
												if (dzld.ShowDialog(this) == DialogResult.OK)
												{
													label = dzld.GetSelectedFile();
													if (label != _resrefs[r])
													{
														_resrefs[r] = Path.GetFileNameWithoutExtension(label);
														Changed = true;
														Invalidate();
													}
												}
											}
										}
									}
								}
							}
						}
						else
						{
							int r = (e.Y + _scroller.Value) / ROWHEIGHT;
							if (r == _r)
							{
								if (_isresref && e.X > COLWIDTH0 && e.X < COLWIDTH0 + COLWIDTH1)
								{
									if (r < _resrefs.Length)
									{
										using (var ip = new Inputbox(_resrefs[r], true))
										{
											if (ip.ShowDialog(this) == DialogResult.OK
												&& ip._result != _resrefs[r])
											{
												_resrefs[r] = ip._result;
												Changed = true;
												Invalidate();
											}
										}
									}
								}
								else if (_isstrref && e.X > COLWIDTH0 + COLWIDTH1 && e.X < COLWIDTH0 + COLWIDTH1 + COLWIDTH2)
								{
									if (r < _strrefs.Length)
									{
										string strref;
										if (_strrefs[r] == 0xFFFFFFFF) strref = String.Empty;
										else                           strref = _strrefs[r].ToString();

										using (var ip = new Inputbox(strref, false))
										{
											if (ip.ShowDialog(this) == DialogResult.OK
												&& ip._result != _strrefs[r].ToString())
											{
												_strrefs[r] = UInt32.Parse(ip._result);
												Changed = true;
												Invalidate();
											}
										}
									}
								}
							}
						}
						break;
				}
			}
		}

		/// <summary>
		/// Overrides the <c>MouseWheel</c> handler. Scrolls the table.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				if (_scroller.Value - ROWHEIGHT < 0)
					_scroller.Value = 0;
				else
					_scroller.Value -= ROWHEIGHT;
			}
			else if (e.Delta < 0)
			{
				if (_scroller.Value + ROWHEIGHT > _vertical)
					_scroller.Value = _vertical;
				else
					_scroller.Value += ROWHEIGHT;
			}
		}


		/// <summary>
		/// Overrides the <c>Paint</c> handler. Draws the table.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			int r,c, r_start = _scroller.Value / ROWHEIGHT;


			// fill backgrounds ->
			var rect = new Rectangle(0,0, Width, ROWHEIGHT);

			Brush brush;
			for (r = r_start; r != _rcount; ++r)
			{
				if ((rect.Y = ROWHEIGHT * r - _scroller.Value) > Bottom)
					break;

				brush = (r % 2 == 0) ? Brushes.AliceBlue : Brushes.BlanchedAlmond;
				e.Graphics.FillRectangle(brush, rect);
			}


			// draw text ->
			for (r = r_start; r != _rcount; ++r)
			{
				if ((rect.Y = ROWHEIGHT * r - _scroller.Value) > Bottom)
					break;

				string text = null;
				for (c = 0; c != COLCOUNT; ++c)
				{
					switch (c)
					{
						case 0:
							text = Chatter.Voices[r]; // WARNING: 'Voices' has only 51 entries. But in theory '_rcount' can be greater than 50.
							rect.X = INDENT;
							rect.Width = COLWIDTH0;
							break;

						case 1:
							text = _resrefs[r];
							rect.X = COLWIDTH0 + INDENT;
							rect.Width = COLWIDTH1 - INDENT;
							break;

						case 2:
						{
							uint strref = _strrefs[r];

							if (strref == 0xFFFFFFFF) text = String.Empty;
							else                      text = strref.ToString();

							rect.X = COLWIDTH0 + COLWIDTH1 + INDENT;
							rect.Width = COLWIDTH2 - INDENT;
							break;
						}

						case 3:
						{
							uint strref = _strrefs[r];

							int id;
							if (strref == 0xFFFFFFFF) id = -1;
							else                      id = (int)strref;

							if (id != -1 && TalkReader.DictDialo.ContainsKey(id)) //TalkReader.DictDialo.Count != 0
								text = TalkReader.DictDialo[id];
							else
								text = String.Empty;

							rect.X = COLWIDTH0 + COLWIDTH1 + COLWIDTH2 + INDENT;
							rect.Width = Width - COLWIDTH0 - COLWIDTH1 - COLWIDTH2 - INDENT;
							break;
						}
					}

					if (text.Length != 0)
					{
						TextRenderer.DrawText(e.Graphics,
											  text,
											  Font,
											  rect,
											  SystemColors.ControlText,
											  flags);
					}
				}
			}


			// draw horizontal lines ->
			int y;
			for (r = r_start + 1; r != _rcount + 1; ++r)
			{
				if ((y = ROWHEIGHT * r - _scroller.Value) > Bottom)
					break;

				e.Graphics.DrawLine(SystemPens.ControlDark,
									0,     y,
									Width, y);
			}

			// draw vertical lines ->
			e.Graphics.DrawLine(SystemPens.ControlDark, COLWIDTH0,                         0, COLWIDTH0,                         Height);
			e.Graphics.DrawLine(SystemPens.ControlDark, COLWIDTH0 + COLWIDTH1,             0, COLWIDTH0 + COLWIDTH1,             Height);
			e.Graphics.DrawLine(SystemPens.ControlDark, COLWIDTH0 + COLWIDTH1 + COLWIDTH2, 0, COLWIDTH0 + COLWIDTH1 + COLWIDTH2, Height);
		}

		/// <summary>
		/// Flags used when drawing texts.
		/// </summary>
		const TextFormatFlags flags = TextFormatFlags.NoPrefix
									| TextFormatFlags.NoPadding
									| TextFormatFlags.Left
									| TextFormatFlags.VerticalCenter
									| TextFormatFlags.SingleLine
									| TextFormatFlags.NoClipping;


		protected override void OnResize(EventArgs e)
		{
			InitScroll();
			base.OnResize(e);
		}
		#endregion Handlers (override)


		#region Handlers
		/// <summary>
		/// Scrolls the table vertically.
		/// </summary>
		/// <param name="sender"><c><see cref="_scroller"></see></c></param>
		/// <param name="e"></param>
		void valuechanged_scroller(object sender, EventArgs e)
		{
			Select(); // <- focus the table when the bar is moved by mousedrag (bar has to move > 0px)

			Invalidate();
		}
		#endregion Handlers


		#region Methods
		/// <summary>
		/// Initializes <c><see cref="_vertical"/></c> and
		/// <c><see cref="_scroller"/>.Maximum</c> value based on the height of
		/// the table.
		/// </summary>
		/// <returns>the value of <c><see cref="_vertical"/></c></returns>
		internal void InitScroll()
		{
			int height = ROWHEIGHT * _rcount;

			// NOTE: Do not set Maximum until after deciding whether or not
			// real Maximum < 0. 'Cause it borks everything up. bingo.

			int vertical = height - Height + (ROWHEIGHT - 1);
			if (vertical < ROWHEIGHT)
			{
				_vertical = (_scroller.Maximum = 0);
			}
			else
			{
				_vertical = ((_scroller.Maximum = vertical) - (ROWHEIGHT - 1));

				// handle .NET OnResize anomaly ->
				// keep the bottom of the table snuggled against the bottom
				// of the visible area when resize enlarges the area

				if (height < Height + _scroller.Value)
					_scroller.Value = _vertical;
			}
		}

		/// <summary>
		/// Updates this <c>ChatPageControl</c> if the
		/// <c><see cref="_extended"/></c> flag changes.
		/// </summary>
		internal void ChangedExtended()
		{
			if (_extended) _rcount = 51;
			else           _rcount = 49;

			var resrefs = new string[_rcount];
			for (int i = 0; i != _rcount; ++i)
			{
				if (i < _resrefs.Length) resrefs[i] = _resrefs[i];
				else                     resrefs[i] = String.Empty;
			}
			_resrefs = resrefs;

			var strrefs = new uint[_rcount];
			for (int i = 0; i != _rcount; ++i)
			{
				if (i < _strrefs.Length) strrefs[i] = _strrefs[i];
				else                     strrefs[i] = 0xFFFFFFFF;
			}
			_strrefs = strrefs;

			InitScroll();
			Refresh();
		}
		#endregion Methods
	}
}
