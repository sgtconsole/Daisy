using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Hijack.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
     

        }

        static int EntryPoint(string jeff)
        {
            //throw new ApplicationException("nop");

            MessageBox.Show($"Fuck yeah from {Process.GetCurrentProcess().MainWindowTitle}");
            return 1;
        }
    }
}
