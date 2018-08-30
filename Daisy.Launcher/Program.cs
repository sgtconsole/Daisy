using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Daisy.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var notepadProcess = Process.GetProcessesByName("notepad++").FirstOrDefault();
            notepadProcess.Invoke(typeof(Foo), "DisplayMessage", "1234");
        }
    }

    class Foo
    {
        static int DisplayMessage(string arg)
        {
            MessageBox.Show($"Hello world from {Process.GetCurrentProcess().MainWindowTitle}. With args {arg}");
            return 1;
        }
    }
}
