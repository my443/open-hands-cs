using System;
using Gtk;

namespace open_hands
{
	class MainClass
	{
		public static void Main (string[] args)
		{

			Application.Init ();
			

			MainWindow win = new MainWindow ();
			win.Title = "With Open Hands I Come";
			
			win.Show ();
			Application.Run ();
			
			
		}
	}
}
