using System.Diagnostics;

namespace NtripShare.GNSS.Device.Helper
{
	public class ShellHelper
	{
		public static void ExuteShell(string FileName, string arg) {
			//实例化一个process类
			Process process = new Process();
			//要执行的shell的名字
			process.StartInfo.FileName = FileName;
			//process.StartInfo.Arguments = "1,2,3";
			process.StartInfo.Arguments = arg;


			//获取或设置是否在新窗口中启动该进程
			process.StartInfo.CreateNoWindow = true;
			//该值指示不能启动进程时是否向用户显示错误的对话框
			process.StartInfo.ErrorDialog = true;
			//关闭shell的使用
			process.StartInfo.UseShellExecute = false;
			process.Start();
			process.WaitForExit();
			process.Close();
		}
	}
}
