using System;
using System.Drawing;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	sealed partial class Inputbox
		: Form
	{
		enum TextChangedVerificationStep
		{
			non,	// dialog start
			first,	// verify text after _t1 tick
			user	// user is typing text
		}

		#region Fields (static)
		static int _x = Int32.MinValue;
		static int _y;
		#endregion Fields (static)


		#region Fields
		/// <summary>
		/// The <c><see cref="ChatPageControl"/></c> that invoked this
		/// <c>Inputbox</c> will try to apply this value when the user presses
		/// <c>[Enter]</c>.
		/// </summary>
		internal string _result = String.Empty;

		/// <summary>
		/// A cache of the last valid string that will be recalled if the user
		/// enters an invalid character.
		/// </summary>
		string _pre = String.Empty;

		/// <summary>
		/// The maximum length of a string based on the current
		/// <c><see cref="Chatter.Output"/> setting</c>.
		/// </summary>
		int _len;

		/// <summary>
		/// Tracks the position of the caret for repositioning if an illegal
		/// character is typed.
		/// </summary>
		int _pos;

		/// <summary>
		/// <c>true</c> if the input/output is a resref-string or <c>false</c>
		/// if the input/output is a strref-uint.
		/// </summary>
		bool _isresref;

		/// <summary>
		/// Tracks how text verification should be dealt with during the loading
		/// sequence.
		/// </summary>
		TextChangedVerificationStep _init = TextChangedVerificationStep.non;

		Timer _t1 = new Timer();
		#endregion Fields


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="val"></param>
		/// <param name="isresref"></param>
		internal Inputbox(string val, bool isresref)
		{
			InitializeComponent();

			if (_isresref = isresref)
				Text = "input resref";
			else
				Text = "input strref";

			ClientSize = new Size(242, 24);
			MinimumSize = new Size(Size.Width, Size.Height);
			MaximumSize = new Size(Int32.MaxValue, Size.Height);

			if (_x != Int32.MinValue)
			{
				StartPosition = FormStartPosition.Manual;
				Location = new Point(_x,_y);
			}

			tb_input.BackColor = Color.WhiteSmoke;

			if (Chatter.Output == SsfFormat.ssf10) _len = 16;
			else                                   _len = 32; // Chatter.Output == SsfFormat.ssf11

			tb_input.Text = val;
			tb_input.SelectionStart = tb_input.Text.Length;

			_t1.Tick += t1_tick;
			_t1.Interval = 80;
			_t1.Start();
		}
		#endregion cTor


		#region Handlers (override)
		/// <summary>
		/// Caches this <c>Inputbox's</c> <c>Location</c>.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_x = Math.Max(0, Location.X);
			_y = Math.Max(0, Location.Y);

			base.OnFormClosing(e);
		}

		/// <summary>
		/// <c>[Enter]</c> sets <c><see cref="_result"/></c> and closes this
		/// <c>Inputbox</c> and returns <c>DialogResult.OK</c> or
		/// <c>[Escape]</c> just closes this <c>Inputbox</c> and returns
		/// <c>DialogResult.Cancel</c>.
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>Requires <c>KeyPreview</c> <c>true</c>.</remarks>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Enter:
					e.SuppressKeyPress = true;

					if (_isresref || tb_input.Text.Length != 0)
						_result = tb_input.Text;
					else
						_result = UInt32.MaxValue.ToString(); // "4294967295" aka. 0xFFFFFFFF

					DialogResult = DialogResult.OK;
					break;

				case Keys.Escape:
					e.SuppressKeyPress = true;
					DialogResult = DialogResult.Cancel;
					break;
			}
			base.OnKeyDown(e);
		}
		#endregion Handlers (override)


		#region Handlers
		/// <summary>
		/// If an error occurs during initialization of
		/// <c><see cref="tb_input"/></c> the <c><see cref="Infobox"/></c> pops
		/// in the topleft of the screen instead of centered on this
		/// <c><see cref="Inputbox"/></c> because that happens before the
		/// <c>Inputbox</c> becomes visible. The workaround is to delay checking
		/// text-validity for a bit ... then call
		/// <c><see cref="textchanged_input()">textchanged_input()</see></c>.
		/// </summary>
		/// <param name="sender"><c><see cref="_t1"/></c></param>
		/// <param name="e"></param>
		void t1_tick(object sender, EventArgs e)
		{
			_t1.Stop();
			_t1.Dispose();

			_init = TextChangedVerificationStep.first;
			textchanged_input(null, EventArgs.Empty);
			_init = TextChangedVerificationStep.user;
		}

		/// <summary>
		/// Tries to ensure that the user doesn't do anything stupid.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void textchanged_input(object sender, EventArgs e)
		{
			if (_init != TextChangedVerificationStep.non)
			{
				if (_isresref)
				{
					if (!islegal(tb_input.Text))
					{
						using (var ib = new Infobox(Infobox.Title_error,
													"only alphanumeric and underscore characters are allowed",
													null,
													InfoboxType.Error))
						{
							ib.ShowDialog(this);
						}

						if (_init == TextChangedVerificationStep.first)
						{
							tb_input.Text = String.Empty; // Recurse <- sets '_pre' - TODO: remove non-alphanumeric+underscore chars
						}
						else
						{
							_pos = tb_input.SelectionStart; // store caret position
							tb_input.Text = _pre; // Recurse <- sets '_pre'
							tb_input.SelectionStart = _pos - 1; // reposition caret
						}
					}
					else if (tb_input.Text.Length > _len)
					{
						using (var ib = new Infobox(Infobox.Title_error,
													"string length exceeds output format",
													null,
													InfoboxType.Error))
						{
							ib.ShowDialog(this);
						}

						if (_init == TextChangedVerificationStep.first)
						{
							tb_input.Text = tb_input.Text.Substring(0, _len); // Recurse <- sets '_pre'
							tb_input.SelectionStart = tb_input.Text.Length;
						}
						else
						{
							_pos = tb_input.SelectionStart; // store caret position
							tb_input.Text = _pre; // Recurse <- sets '_pre'
							tb_input.SelectionStart = _pos - 1; // reposition caret
						}
					}
					else
						_pre = tb_input.Text;
				}
				else // is strref
				{
					uint result = 0;
					if (tb_input.Text.Length != 0 && !UInt32.TryParse(tb_input.Text, out result))
					{
						using (var ib = new Infobox(Infobox.Title_error,
													"could not parse to 32-bit unsigned value",
													null,
													InfoboxType.Error))
						{
							ib.ShowDialog(this);
						}

						if (_init == TextChangedVerificationStep.first)
						{
							tb_input.Text = String.Empty; // Recurse <- sets '_pre'
						}
						else
						{
							_pos = tb_input.SelectionStart; // store caret position
							tb_input.Text = _pre; // Recurse <- sets '_pre'
							tb_input.SelectionStart = _pos - 1; // reposition caret
						}
					}
					else if (result > 0x01FFFFFF)
					{
						using (var ib = new Infobox(Infobox.Title_error,
													"value must be less than 33,554,432",
													null,
													InfoboxType.Error))
						{
							ib.ShowDialog(this);
						}

						if (_init == TextChangedVerificationStep.first)
						{
							tb_input.Text = String.Empty; // Recurse <- sets '_pre'
						}
						else
						{
							_pos = tb_input.SelectionStart; // store caret position
							tb_input.Text = _pre; // Recurse <- sets '_pre'
							tb_input.SelectionStart = _pos - 1; // reposition caret
						}
					}
					else
						_pre = tb_input.Text;
				}
			}
		}
		#endregion Handlers


		#region Methods (static)
		/// <summary>
		/// Checks if a specified string is alphanumeric or underscore.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool islegal(string input)
		{
			foreach (char @char in input)
			{
				if (!isAsciiAlphanumericOrUnderscore(@char))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Helper for <c><see cref="islegal()">islegal()</see></c>.
		/// </summary>
		/// <param name="char"></param>
		/// <returns></returns>
		static bool isAsciiAlphanumericOrUnderscore(char @char)
		{
			int c = @char;
			return  c == 95					// _
				|| (c >= 48 && c <=  57)	// 0..9
				|| (c >= 65 && c <=  90)	// A..Z
				|| (c >= 97 && c <= 122);	// a..z
		}
		#endregion Methods (static)
	}
}
