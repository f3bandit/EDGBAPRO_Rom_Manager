using System;
using System.Windows.Forms;

namespace EDGBAPRO_Rom_Manager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
