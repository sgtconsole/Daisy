using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Hijack
{
    public partial class Injector
    {
        private readonly int _processId;
        private readonly IntPtr _nullPtr = (IntPtr)null;
        private IntPtr _targetProcessHandle;
        
        public Injector(int processId)
        {
            _processId = processId;
            LoadBootstrapper();
        }

        private string BootstrapperName
        {
            get
            {
                var fi = new FileInfo("bootstrapper.dll");
                return fi.FullName;
            }
        }

        private IntPtr WriteString(IntPtr processHandle, string data, Encoding encoding)
        {
            var encodedData = encoding.GetBytes(data);
            var encodedDataLength = (uint)((encodedData.Length + 1) * Marshal.SizeOf(typeof(char)));

            var allocation = VirtualAllocEx(
                processHandle,
                _nullPtr,
                encodedDataLength,
                AllocationType.Reserve | AllocationType.Commit,
                MemoryProtection.ReadWrite);

            WriteProcessMemory(
                processHandle,
                allocation,
                encodedData,
                (int)encodedDataLength,
                out _);

            return allocation;
        }

        private void LoadBootstrapper()
        {
            
            var kernel32Handle = GetModuleHandle("kernel32.dll");
            _targetProcessHandle = OpenProcess(ProcessAccessFlags.All, false, _processId);
            
            if (_targetProcessHandle == _nullPtr)
                throw new Exception("Open process failed");

            var bootstrapperNameArgumentAllocation = WriteString(_targetProcessHandle, BootstrapperName, Encoding.ASCII);

            var loadLibraryDelegate =  GetProcAddress(kernel32Handle, "LoadLibraryA");

            var loadLibraryThread = CreateRemoteThread(
                _targetProcessHandle,
                _nullPtr,
                0,
                loadLibraryDelegate,
                bootstrapperNameArgumentAllocation, 0, _nullPtr
            );
            
            WaitForSingleObject(loadLibraryThread, 0xFFFFFFFF);
            IntPtr exitCode;
            GetExitCodeThread(loadLibraryThread, out exitCode);
            VirtualFreeEx(_targetProcessHandle, bootstrapperNameArgumentAllocation, 0, AllocationType.Release);
        }


        public void Invoke(string assemblyPath)
        {
            const string bootstrapperEntryPoint = "LoadManagedProject";

            var procOffset = GetProcedureOffset(BootstrapperName, bootstrapperEntryPoint);

            var mod = Process.GetProcessById(_processId).Modules.Cast<ProcessModule>()
                .Single(pm => pm.ModuleName.Contains("bootstrapper"));
           
            var procAddress = IntPtr.Add(mod.BaseAddress, procOffset);
            var argumentAllocation = WriteString(_targetProcessHandle, assemblyPath,Encoding.Unicode);

            var loadLibraryThread = CreateRemoteThread(
                _targetProcessHandle,
                _nullPtr,
                0,
                procAddress,
                argumentAllocation, 0, _nullPtr
            );

            WaitForSingleObject(loadLibraryThread, 0xFFFFFFFF);
            GetExitCodeThread(loadLibraryThread, out _);
            VirtualFreeEx(_targetProcessHandle, argumentAllocation, 0, AllocationType.Release);

        }

        private int GetProcedureOffset(string libName, string procedureName)
        {
            var handle = LoadLibrary(libName);
            var entryPoint = GetProcAddress(handle, procedureName);
            return (int)entryPoint - (int)handle;
        }
        
    }
}
