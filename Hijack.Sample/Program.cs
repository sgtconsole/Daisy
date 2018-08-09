using System.Diagnostics;
using System.Linq;

namespace Hijack.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var injector = new Injector();
            //var notepadProcess = Process.GetProcessesByName("notepad++").FirstOrDefault();
            //if (notepadProcess != null) injector.Inject(notepadProcess.Id);
            injector.Invoke();

        }
    }
}
