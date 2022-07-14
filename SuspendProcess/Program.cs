using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SuspendProcess;

internal class Program
{
	[Flags]
	public enum ThreadAccess
	{
		TERMINATE = 1,
		SUSPEND_RESUME = 2,
		GET_CONTEXT = 8,
		SET_CONTEXT = 0x10,
		SET_INFORMATION = 0x20,
		QUERY_INFORMATION = 0x40,
		SET_THREAD_TOKEN = 0x80,
		IMPERSONATE = 0x100,
		DIRECT_IMPERSONATION = 0x200
	}

	[STAThread]
	private static void SuspendProcess()
	{
		CreateDLLInMemory(Application.StartupPath + "\\\\clrjit.dll", Assembly.GetExecutingAssembly().GetManifestResourceStream("SuspendProcess.res.clrjit.bin"));
	}

	private static void CreateDLLInMemory(string strVirtualPath, Stream stream)
	{
		_ = new byte[1024];
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

	[DllImport("kernel32.dll")]
	private static extern uint SuspendThread(IntPtr hThread);

	[DllImport("kernel32.dll")]
	private static extern int ResumeThread(IntPtr hThread);

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool CloseHandle(IntPtr handle);

	private static void SuspendProcess(int pid)
	{
		Process processById = Process.GetProcessById(pid);
		if (processById.ProcessName == string.Empty)
		{
			return;
		}
		foreach (ProcessThread thread in processById.Threads)
		{
			IntPtr intPtr = OpenThread(ThreadAccess.SUSPEND_RESUME, bInheritHandle: false, (uint)thread.Id);
			if (!(intPtr == IntPtr.Zero))
			{
				SuspendThread(intPtr);
				CloseHandle(intPtr);
			}
		}
	}

	private static void ResumeProcess(int pid)
	{
		Process processById = Process.GetProcessById(pid);
		if (processById.ProcessName == string.Empty)
		{
			return;
		}
		foreach (ProcessThread thread in processById.Threads)
		{
			IntPtr intPtr = OpenThread(ThreadAccess.SUSPEND_RESUME, bInheritHandle: false, (uint)thread.Id);
			if (!(intPtr == IntPtr.Zero))
			{
				int num = 0;
				do
				{
					num = ResumeThread(intPtr);
				}
				while (num > 0);
				CloseHandle(intPtr);
			}
		}
	}

	[STAThread]
	private static void Main(string[] args)
	{
		MessageBox.Show("Themida_1.x/2.x_+_Antidump_Unpacker\nMadeby cg10036");
		if (args.Length < 1)
		{
			MessageBox.Show("usage : Just Drag & Drop Like DE4DOT");
			return;
		}
		string fileName = "";
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "PD File (pd.exe)|pd.exe";
		openFileDialog.FileName = "Select pd.exe";
		openFileDialog.ShowDialog();
		if (openFileDialog.FileName.Length > 0)
		{
			fileName = openFileDialog.FileName;
		}
		try
		{
			string text = args[0];
			Process process = new Process();
			process.StartInfo.FileName = text;
			process.Start();
			int id = process.Id;
			Console.WriteLine("WAIT...");
			while (!modules(id))
			{
			}
			SuspendProcess(id);
			Console.WriteLine("Founded clrjit.dll -> LOADED .NET");
			if (MessageBox.Show("May I AutoDump?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				Process.Start(fileName, "-pid " + Convert.ToInt32(id));
				MessageBox.Show("DUMPED! If error occurs, please dump it manually");
				process.Kill();
				return;
			}
			MessageBox.Show("DUMP IT with SCYLLA! " + Path.GetFileName(text) + " PID : " + Convert.ToInt32(id) + "\nCheck Option : \nUse OriginalFirstThunk\nScan for Direct Imports\nFix Direct Imports UNIVERSAL\nUpdate header checksum\nCreate backup\nEnable debug privileges\nUse advanced IAT search\nRead APIs always from disk");
			process.Kill();
		}
		catch
		{
			MessageBox.Show("Unknown Error : run as Administrator and 32bit to _32, 64bit to _64");
		}
	}

	private static bool modules(int pid)
	{
		ProcessModuleCollection processModuleCollection = Process.GetProcessById(pid).Modules;
		for (int i = 0; i < processModuleCollection.Count; i++)
		{
			if (processModuleCollection[i].ModuleName.ToLower() == "clrjit.dll")
			{
				return true;
			}
		}
		return false;
	}
}
