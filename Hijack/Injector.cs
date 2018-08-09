using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Hijack
{
    public partial class Injector
    {
        private const string BootstrapperName = @"C:\Git\COB\Hijack\Hijack.Sample\bin\x64\Debug\Bootstrapper.dll";

        public void Inject(int processId)
        {
            
            var kernel32Handle = GetModuleHandle("kernel32.dll");
            var targetProcessHandle = OpenProcess(ProcessAccessFlags.All, false, processId);
            var nullPtr = (IntPtr) null;

            if (targetProcessHandle == nullPtr)
                throw new Exception("Open process failed");

            var boostrapperNameData = Encoding.ASCII.GetBytes(BootstrapperName);
            var bootstrapperLength = (uint) ((BootstrapperName.Length + 1) * Marshal.SizeOf(typeof(char)));
            
            var bootstrapperNameArgumentAllocation = VirtualAllocEx(
                targetProcessHandle,
                nullPtr,
                bootstrapperLength,
                AllocationType.Reserve | AllocationType.Commit,
                MemoryProtection.ReadWrite);

            WriteProcessMemory(
                targetProcessHandle,
                bootstrapperNameArgumentAllocation,
                boostrapperNameData,
                (int) bootstrapperLength,
                out _);

            var loadLibraryDelegate =  GetProcAddress(kernel32Handle, "LoadLibraryA");

            var loadLibraryThread = CreateRemoteThread(
                targetProcessHandle,
                nullPtr,
                0,
                loadLibraryDelegate,
                bootstrapperNameArgumentAllocation, 0, nullPtr
            );
            
            WaitForSingleObject(loadLibraryThread, 0xFFFFFFFF);
            GetExitCodeThread(loadLibraryThread, out _);
            VirtualFreeEx(targetProcessHandle, bootstrapperNameArgumentAllocation, 0, AllocationType.Release);

            var proc = Process.GetProcessById(processId);
            ProcessModule foundModule;
            foreach (ProcessModule mod in proc.Modules)
            {
                if (!mod.ModuleName.ToLower().Contains("bootstapper.dll")) continue;

                foundModule = mod;
                break;
            }
            

        }


        public void Invoke()
        {
            var handle = LoadLibraryExA(BootstrapperName, IntPtr.Zero, LoadLibraryFlags.DontResolveDllReferences);
            var dosHeader = Marshal.PtrToStructure<IMAGE_DOS_HEADER>(handle);
            var ntHeader = Marshal.PtrToStructure<IMAGE_NT_HEADERS32>(IntPtr.Add(handle, dosHeader.e_lfanew));
            var exportDirectory = Marshal.PtrToStructure<IMAGE_EXPORT_DIRECTORY>(IntPtr.Add(handle, (int) ntHeader.OptionalHeader.ExportTable.VirtualAddress));
            


            var error = Marshal.GetLastWin32Error();
        }





    }
}
