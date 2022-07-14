using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SuspendProcess
{
    class Program
    {
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);


        static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            MessageBox.Show("Themida_1.x/2.x_+_Antidump_Unpacker\nMadeby cg10036");
            if (args.Length < 1)
            {
                MessageBox.Show("usage : Just Drag & Drop Like DE4DOT");
                return;
            }
            string pd_path = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "PD File (pd.exe)|pd.exe";
            ofd.FileName = "Select pd.exe";
            ofd.ShowDialog();
            if(ofd.FileName.Length > 0)
            {
                pd_path = ofd.FileName;
            }
            try
            {
                int pid;
                string path = args[0];
                Process p = new Process();
                p.StartInfo.FileName = path;
                p.Start();
                pid = p.Id;
                Console.WriteLine("WAIT...");
                while (true)
                {
                    if (modules(pid))
                    {
                        SuspendProcess(pid);
                        Console.WriteLine("Founded clrjit.dll -> LOADED .NET");
                        if(MessageBox.Show("May I AutoDump?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Process.Start(pd_path, "-pid " + Convert.ToInt32(pid));
                            MessageBox.Show("DUMPED! If error occurs, please dump it manually");
                            p.Kill();
                            return;
                        }
                        else
                        {
                            MessageBox.Show("DUMP IT with SCYLLA! " + Path.GetFileName(path) + " PID : " + Convert.ToInt32(pid) + "\nCheck Option : \nUse OriginalFirstThunk\nScan for Direct Imports\nFix Direct Imports UNIVERSAL\nUpdate header checksum\nCreate backup\nEnable debug privileges\nUse advanced IAT search\nRead APIs always from disk");
                            p.Kill();
                            return;
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unknown Error : run as Administrator and 32bit to _32, 64bit to _64");
            }
        }

        static bool modules(int pid)
        {
            Process p = Process.GetProcessById(pid);
            ProcessModule pm;
            ProcessModuleCollection pmc = p.Modules;
            for (int i = 0; i < pmc.Count; i++)
            {
                pm = pmc[i];
                if (pm.ModuleName.ToLower() == "clrjit.dll")
                    return true;
            }
            return false;
        }
    }
}
