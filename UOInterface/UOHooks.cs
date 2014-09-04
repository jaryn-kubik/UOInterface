using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

namespace UOInterface
{
    internal class UOHooks
    {
        public int ProcessId { get; private set; }
        public Version Version { get; private set; }
        public IReadOnlyList<uint> PacketTable { get; private set; }

        private readonly unsafe byte* dataIn, dataOut;
        private readonly unsafe int* msgIn, msgOut;
        private readonly IntPtr sentIn, handledIn, sentOut, handledOut;
        private readonly object syncRoot = new object();
        private int nextIn, nextOut;
        private const int BufferSize = 0x80000;

        private unsafe UOHooks(int pId, IntPtr sharedMemory, OnUOMessage handler)
        {
            ProcessId = pId;
            FileVersionInfo v = Process.GetProcessById(pId).MainModule.FileVersionInfo;
            Version = new Version(v.FileMajorPart, v.FileMinorPart, v.FileBuildPart, v.FilePrivatePart);

            dataIn = (byte*)sharedMemory.ToPointer();
            dataOut = dataIn + BufferSize;

            msgIn = (int*)(dataOut + BufferSize);
            msgOut = msgIn + 4;

            uint[] packetTable = new uint[0x100];
            fixed (uint* p = packetTable)
                memcpy(p, dataIn, packetTable.Length * sizeof(uint));
            PacketTable = packetTable;

            sentIn = new IntPtr(msgIn[0]);
            handledIn = new IntPtr(msgIn[1]);
            sentOut = new IntPtr(msgIn[2]);
            handledOut = new IntPtr(msgIn[3]);

            new Thread(MessagePump) { IsBackground = true }.Start(handler);
        }

        public unsafe int Send(UOMessage msg, int arg1 = 0, int arg2 = 0, int arg3 = 0)
        {
            lock (syncRoot)
            {
                msgOut[0] = (int)msg;
                msgOut[1] = arg1;
                msgOut[2] = arg2;
                msgOut[3] = arg3;
                SignalObjectAndWait(sentOut, handledOut);
                return msgOut[0];
            }
        }

        public unsafe void SendData(UOMessage msg, byte[] buffer, int len)
        {
            lock (syncRoot)
            {
                if (nextOut + len > BufferSize)
                    throw new InternalBufferOverflowException();

                fixed (byte* b = buffer)
                    memcpy(dataOut + nextOut, b, len);

                msgOut[0] = (int)msg;
                msgOut[1] = len;
                msgOut[2] = nextOut;
                SignalObjectAndWait(sentOut, handledOut);
                nextOut = msgOut[0];
            }
        }

        public unsafe delegate int OnUOMessage(UOMessage msg, int arg1, int arg2, int arg3, byte* data);
        private unsafe void MessagePump(object handler)
        {
            OnUOMessage h = (OnUOMessage)handler;
            UOMessage msg;
            WaitForSingleObject(sentIn);
            do
            {
                msg = (UOMessage)msgIn[0];
                msgIn[0] = h(msg, msgIn[1], msgIn[2], msgIn[3], dataIn);
                SignalObjectAndWait(handledIn, sentIn);
            } while (msg != UOMessage.Closing);
        }

        public static unsafe UOHooks Start(string client, OnUOMessage handler)
        {
            PROCESS_INFORMATION pi = CreateSuspendedProcess(client, null);
            try
            {
                int id = Process.GetCurrentProcess().Id;
                IntPtr mmf = InjectLibrary(pi.processId, "UOHooks.dll", "OnAttach", (IntPtr)id);

                const int FILE_MAP_WRITE = 0x0002;
                IntPtr sharedMemory = MapViewOfFile(mmf, FILE_MAP_WRITE, 0, 0, IntPtr.Zero);
                if (sharedMemory == IntPtr.Zero)
                    throw new Win32Exception("MapViewOfFile");

                return new UOHooks(pi.processId, sharedMemory, handler);
            }
            finally { ResumeThread(pi.hThread); }
        }

        private static IntPtr InjectLibrary(int pId, string dllName, string function, IntPtr arg)
        {
            FileInfo dll = new FileInfo(dllName);
            if (!dll.Exists)
                throw new DllNotFoundException(dllName);

            const int CREATE_THREAD = 0x0002;
            const int VM_OPERATION = 0x0008;
            const int VM_READ = 0x0010;
            const int VM_WRITE = 0x0020;
            const int QUERY_INFORMATION = 0x0400;
            IntPtr hProcess = OpenProcess(CREATE_THREAD | VM_OPERATION | VM_READ | VM_WRITE | QUERY_INFORMATION, false, pId);
            if (hProcess == IntPtr.Zero)
                throw new Win32Exception("OpenProcess");

            byte[] asm = { 0xC2, 0x04, 0x00 };//retn 4
            byte[] data = Encoding.Unicode.GetBytes(dll.FullName + '\0');

            //allocate memory
            const int MEM_COMMIT = 0x1000;
            const int PAGE_EXECUTE_READWRITE = 0x40;
            IntPtr pRemote = VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)data.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            if (pRemote == IntPtr.Zero)
                throw new Win32Exception("VirtualAllocEx");

            //execute dummy function, forces the process to load modules
            WriteProcessMemory(hProcess, pRemote, asm);
            CreateRemoteThread(hProcess, pRemote, IntPtr.Zero);

            //load target library into the process
            IntPtr hKernel32 = GetRemoteModuleHandle(hProcess, "kernel32.dll");
            IntPtr pLoadLibrary = GetRemoteProcAddress(hProcess, hKernel32, "LoadLibraryW");
            WriteProcessMemory(hProcess, pRemote, data);
            CreateRemoteThread(hProcess, pLoadLibrary, pRemote);

            //call specified function in target library
            IntPtr hDll = GetRemoteModuleHandle(hProcess, dllName);
            IntPtr pFunction = GetRemoteProcAddress(hProcess, hDll, function);
            IntPtr result = CreateRemoteThread(hProcess, pFunction, arg);

            //release resources
            const int MEM_RELEASE = 0x8000;
            VirtualFreeEx(hProcess, pRemote, (IntPtr)data.Length, MEM_RELEASE);
            CloseHandle(hProcess);

            return result;
        }

        private static IntPtr GetRemoteModuleHandle(IntPtr hProcess, string dllName)
        {
            uint needed;
            const int LIST_MODULES_32BIT = 0x01;
            if (!EnumProcessModulesEx(hProcess, new IntPtr[0], 0, out needed, LIST_MODULES_32BIT))
                throw new Win32Exception("EnumProcessModulesEx");

            IntPtr[] hModules = new IntPtr[needed / IntPtr.Size];
            if (!EnumProcessModulesEx(hProcess, hModules, needed, out needed, LIST_MODULES_32BIT))
                throw new Win32Exception("EnumProcessModulesEx");

            StringBuilder sb = new StringBuilder(1024);
            foreach (IntPtr hModule in hModules)
            {
                if (GetModuleBaseName(hProcess, hModule, sb, sb.Capacity) == 0)
                    throw new Win32Exception("GetModuleBaseName");
                if (dllName.Equals(sb.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    return hModule;
            }
            throw new Win32Exception("GetRemoteModuleHandle");
        }

        private static IntPtr GetRemoteProcAddress(IntPtr hProcess, IntPtr hModule, string function)
        {
            //fuck it, i am not importing thousands of data structures
            IntPtr ntHeader = hModule + ReadInt(hProcess, hModule + 60);
            IntPtr optionalHeader = ntHeader + 24;
            IntPtr dataDirectory = optionalHeader + 96;
            IntPtr exports = hModule + ReadInt(hProcess, dataDirectory);

            int count = ReadInt(hProcess, exports + 20);
            IntPtr functions = hModule + ReadInt(hProcess, exports + 28);
            IntPtr names = hModule + ReadInt(hProcess, exports + 32);

            for (int i = 0; i < count; i++)
            {
                IntPtr namePtr = hModule + ReadInt(hProcess, names + i * 4);
                string name = ReadString(hProcess, namePtr);

                int index = name.IndexOf('@');
                if (index != -1)
                    name = name.Substring(1, index - 1);

                if (function == name)
                    return hModule + ReadInt(hProcess, functions + i * 4);
            }
            throw new Win32Exception("GetRemoteProcAddress");
        }

        private static void WriteProcessMemory(IntPtr process, IntPtr address, byte[] buffer)
        {
            if (!WriteProcessMemory(process, address, buffer, (IntPtr)buffer.Length, IntPtr.Zero))
                throw new Win32Exception("WriteProcessMemory");
        }

        private static int ReadInt(IntPtr process, IntPtr address)
        {
            byte[] buffer = new byte[4];
            if (!ReadProcessMemory(process, address, buffer, (IntPtr)buffer.Length, IntPtr.Zero))
                throw new Win32Exception("ReadProcessMemory");
            return BitConverter.ToInt32(buffer, 0);
        }

        private static string ReadString(IntPtr process, IntPtr address)
        {
            StringBuilder sb = new StringBuilder();
            for (byte[] buffer = new byte[1]; ; address += 1)
            {
                if (!ReadProcessMemory(process, address, buffer, (IntPtr)buffer.Length, IntPtr.Zero))
                    throw new Win32Exception("ReadProcessMemory");
                if (buffer[0] == 0)
                    return sb.ToString();
                sb.Append((char)buffer[0]);
            }
        }

        private static IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpStartAddress, IntPtr lpParameter)
        {
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, IntPtr.Zero, lpStartAddress, lpParameter, 0, IntPtr.Zero);
            if (hThread == IntPtr.Zero)
                throw new Win32Exception("CreateRemoteThread");
            WaitForSingleObject(hThread);
            uint result;
            GetExitCodeThread(hThread, out result);
            CloseHandle(hThread);
            return (IntPtr)result;
        }

        private static PROCESS_INFORMATION CreateSuspendedProcess(string path, string dir)
        {
            PROCESS_INFORMATION pi;
            byte[] si = new byte[10 * 4 + 7 * IntPtr.Size];
            si[0] = (byte)si.Length;

            const int CREATE_SUSPENDED = 0x00000004;
            if (!CreateProcess(path, null, IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED, IntPtr.Zero, dir, si, out pi))
                throw new Win32Exception("CreateProcess");
            return pi;
        }

        #region Imports
        [DllImport("msvcrt.dll"), SuppressUnmanagedCodeSecurity]
        static extern unsafe void memcpy(void* to, void* from, int len);

        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
        static extern int WaitForSingleObject(IntPtr hHandle, int timeout = -1);

        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
        static extern uint SignalObjectAndWait(IntPtr hToSignal, IntPtr hToWaitOn, int timeout = -1, bool alertable = false);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CreateProcess(string path, string cmd, IntPtr pAttributes, IntPtr tAttributes,
            bool inherit, uint flags, IntPtr environment, string dir, byte[] sInfo, out PROCESS_INFORMATION pInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool inherit, int pId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr address, IntPtr size, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr address, byte[] buffer, IntPtr size, IntPtr written);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr address, [Out] byte[] buffer, IntPtr size, IntPtr read);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, IntPtr dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, IntPtr size, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr mmf, uint access, uint offsetHigh, uint offsetLow, IntPtr toMap);

        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool EnumProcessModulesEx(IntPtr hProcess, [In][Out] IntPtr[] modules, uint size, out uint needed, uint flags);

        [DllImport("psapi.dll", SetLastError = true)]
        static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, int nSize);

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int processId;
            public int threadId;
        }
        #endregion
    }

    public enum UOMessage
    {
        Ready, Connected, Disconnecting, Closing, Focus, Visibility,
        KeyDown, PacketToClient, PacketToServer,
        ConnectionInfo, Pathfinding, GameSize
    }
}