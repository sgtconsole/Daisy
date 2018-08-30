using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Daisy
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

        private string _bootstrapperName;
        private string BootstrapperName
        {
            get
            {

                if (_bootstrapperName != null) return _bootstrapperName;

                var tempFileName = $"{Path.GetTempPath()}{Guid.NewGuid():N}.dll";
                using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Daisy.Libs.Bootstrap.dll"))
                {
                    using (var file = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }

                _bootstrapperName = tempFileName;
                return _bootstrapperName;
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


        internal void Invoke(Type type, string methodName, string argument)
        {
            const string bootstrapperEntryPoint = "ImplantDotNetAssembly";
            
            var assemblyLocation = CreateReadableFile(type.Assembly.Location);
            var methodInfo = $"{assemblyLocation}\t{type.FullName}\t{methodName}\t{argument}";
            
            var bootstrapperInstanceName = new FileInfo(BootstrapperName).Name;
            var mod = Process.GetProcessById(_processId).Modules.Cast<ProcessModule>()
                .Single(pm => pm.ModuleName.Contains(bootstrapperInstanceName));

            var procOffset = GetProcedureOffset(BootstrapperName, bootstrapperEntryPoint);
            var procAddress = IntPtr.Add(mod.BaseAddress, procOffset);
            var argumentAllocation = WriteString(_targetProcessHandle, methodInfo, Encoding.Unicode);
            
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

        private string CreateReadableFile(string path)
        {
            var tempFileName = $"{Path.GetTempPath()}{Guid.NewGuid():N}.asm";
            File.Copy(path, tempFileName);
            return tempFileName;
        }

        private int GetProcedureOffset(string libName, string procedureName)
        {
            var handle = LoadLibrary(libName);
            var entryPoint = GetProcAddress(handle, procedureName);
            return (int)entryPoint - (int)handle;
        }
        
    }
}
