using System.Diagnostics;
using System.Windows.Forms;

namespace Daisy.Sample
{
    public class Program
    {
        static void Main(string[] args)
        {
     

        }

        static int EntryPoint(string arg)
        {
            MessageBox.Show($"Hello world from {Process.GetCurrentProcess().MainWindowTitle}");
            return 1;
        }
    }
}
