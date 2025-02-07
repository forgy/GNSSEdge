using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Helper;

namespace NtripShare.GNSS.Device.Service
{
	public class SimService : BaseService
	{
		private static ILog log = LogManager.GetLogger("NtripShare", typeof(WifiService));

		private static SimService Instance = null;


		public static SimService getInstance()
		{
			if (Instance == null)
			{
				Instance = new SimService();
			}
			return Instance;
		}

		private SimService()
		{

		}

		/// <summary>
		/// 停止
		/// </summary>
		public override void StopService()
		{
			IsServiceStarted = false;
			try
			{
				ShellHelper.ExuteShell(Directory.GetCurrentDirectory() + "/Shell/stop4G.sh", "");
				log.Info("/Shell/stop4G---");
			}
			catch (Exception e)
			{
				log.Error(e.Message);
			}
		}

		/// <summary>
		/// 启动服务
		/// </summary>
		public override void StartService()
		{
			SysConfig mosConfig = SysConfig.getInstance();
			if (!mosConfig.WIFI) 
			{
				return;
			}
			IsServiceStarted = true;

			Task.Run(() =>
			{
				try
				{
					//ShellHelper.ExuteShell(Directory.GetCurrentDirectory() + "/Shell/stopWifi.sh","");
					//log.Info("/Shell/stopWifi---");
					ShellHelper.ExuteShell(Directory.GetCurrentDirectory() + "/Shell/start4G.sh", "");
					Thread.Sleep(5000);
					log.Info("/Shell/start4G---");
					
				}
				catch (Exception e)
				{
					log.Error(e);
				}
			});
		
		}
	}
}
