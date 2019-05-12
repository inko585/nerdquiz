using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NerdQuizWPF
{
    static public class WindowExt
    {

        // NB : Best to call this function from the windows Loaded event or after showing the window
        // (otherwise window is just positioned to fill the secondary monitor rather than being maximised).
        public static void MaximizeToSpecificMonitor(this Window window, System.Windows.Forms.Screen screen, bool maximized = true)
        {
            if (screen != null)
            {
                if (!window.IsLoaded)
                    window.WindowStartupLocation = WindowStartupLocation.Manual;

                var workingArea = screen.WorkingArea;
                window.Left = workingArea.Left;
                window.Top = workingArea.Top;
                // If window isn't loaded then maxmizing will result in the window displaying on the primary monitor
                if (window.IsLoaded && maximized)
                {
                    window.Width = workingArea.Width;
                    window.Height = workingArea.Height;
                    window.WindowState = WindowState.Maximized;
                }
            }
        }
    }
}
