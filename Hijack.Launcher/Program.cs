using System;
using System.Diagnostics;
using System.Linq;

namespace Hijack.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var notepadProcess = Process.GetProcessesByName("notepad++").FirstOrDefault();
            var injector = new Injector(notepadProcess.Id);
            
            injector.Invoke(@"C:\Git\Hijack\Hijack.Sample\bin\Debug\Hijack.Sample.exe");
            Console.ReadLine();

        }
    }
}
