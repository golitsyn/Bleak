using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static Bleak.Etc.Native;
using static Bleak.Etc.Wrapper;

namespace Bleak.Extensions
{
    internal static class EraseHeaders
    {
        internal static bool Erase(string dllPath, string processName)
        {
            // Ensure both parameters are valid

            if (string.IsNullOrEmpty(dllPath) || string.IsNullOrEmpty(processName))
            {
                return false;
            }

            // Get an instance of the specified process

            Process process;

            try
            {
                process = Process.GetProcessesByName(processName).FirstOrDefault();
            }

            catch (IndexOutOfRangeException)
            {
                return false;
            }

            // Erase the headers

            return Erase(dllPath, process);
        }

        internal static bool Erase(string dllPath, int processId)
        {
            // Ensure both parameters are valid

            if (string.IsNullOrEmpty(dllPath) || processId == 0)
            {
                return false;
            }

            // Get an instance of the specified process

            Process process;

            try
            {
                process = Process.GetProcessById(processId);
            }

            catch (IndexOutOfRangeException)
            {
                return false;
            }

            // Erase the headers

            return Erase(dllPath, process);
        }

        private static bool Erase(string dllPath, Process process)
        {
            // Get the handle of the specified process

            var processHandle = process.SafeHandle;

            if (processHandle == null)
            {
                return false;
            }

            // Find the dll base address

            var moduleBaseAddress = process.Modules.Cast<ProcessModule>().First(module => module.ModuleName == Path.GetFileName(dllPath)).BaseAddress;

            if (moduleBaseAddress == IntPtr.Zero)
            {
                return false;
            }

            // Get the information about the header region of the dll

            var memoryInformationSize = Marshal.SizeOf(typeof(MemoryBasicInformation));

            if (!VirtualQueryEx(processHandle, moduleBaseAddress, out var memoryInformation, memoryInformationSize))
            {
                return false;
            }

            // Generate a buffer to write over the header region with

            var buffer = new byte[(int) memoryInformation.RegionSize];

            // Write over the header region

            return WriteMemory(processHandle, moduleBaseAddress, buffer);
        }
    }
}