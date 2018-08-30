# Daisy
A framework for injecting and executing .Net assemblies into unmanaged processes

## Summary
Daisy is an educational lightweight framework which demonstrates how to inject and execute managed code into a running unmanaged process. It achieves this by doing the following:

1. Loading an unmanaged cpp bootsrap module into a running x86 process by remotely calling LoadLibrary
2. Creating a new thread and executing the loaded bootstrap module which in turn loads the CLR
3. Instructing the bootstrap module to create a new app domain and execute a user specified method

## Example Usage
````c#
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
````


