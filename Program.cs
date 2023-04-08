using System;
using System.Windows.Forms;


namespace nwn2_Chatter
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			logfile.CreateLog();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Application.Run(new Chatter());
		}
	}
}
