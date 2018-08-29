using System;
using System.Diagnostics;
using System.Linq;

namespace Daisy.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var notepadProcess = Process.GetProcessesByName("notepad++").FirstOrDefault();
            notepadProcess.Invoke(typeof(Sample.Program),"EntryPoint");
            Console.ReadLine();
        }
    }
}
