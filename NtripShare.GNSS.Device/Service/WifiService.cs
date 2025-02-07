using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Helper;
using System.Net.NetworkInformation;

namespace NtripShare.GNSS.Device.Service
{
	public class WifiService : BaseService
	{
		private static ILog log = LogManager.GetLogger("NtripShare", typeof(WifiService));

		private static WifiService Instance = null;

		private System.Diagnostics.Process p = new System.Diagnostics.Process();

		/// <summary>
		/// 设置自停止时间
		/// </summary>
		public int CloseTime { get; set; }

		System.Timers.Timer timer = null;

		public static WifiService getInstance()
		{
			if (Instance == null)
			{
				Instance = new WifiService();
			}
			return Instance;
		}

		private WifiService()
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
				ShellHelper.ExuteShell(Directory.GetCurrentDirectory() + "/Shell/stopWifi.sh", "");
				log.Info("/Shell/stopWifi---");

				if (timer != null)
				{
					timer.Stop();
				}
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
					ShellHelper.ExuteShell(Directory.GetCurrentDirectory() + "/Shell/startWifi.sh", "");
					Thread.Sleep(5000);
					log.Info("/Shell/startWifi---");
					string wifiEth = getEthName();
					string wifi = getHotSpotName();
					
					string key = "0000";
					if (mosConfig.DeviceKey != null)
					{
						key = mosConfig.DeviceKey;
					}
					if (key.Length > 4)
					{
						key = key.Substring(key.Length - 4);
					}

					//string arg = $" device wifi hotspot con-name ap001 ifname {wifi} ssid NS_GNSS_{key} password {mosConfig.WIFIPassword}";
					//log.Info("StartService---" + arg);
					//p.StartInfo.FileName = "nmcli";
					string arg = $" {wifi} {wifiEth} NS_GNSS_{key} {mosConfig.WIFIPassword} --no-virt";
					log.Info("StartService---" + arg);
					p.StartInfo.FileName = "create_ap";
					p.StartInfo.Arguments = arg;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.Start();
					p.WaitForExit();
				}
				catch (Exception e)
				{
					log.Error(e);
				}
			});
			if (SysConfig.getInstance().WIFICloseTime == 0)
			{
				return;
			}
			if (timer == null)
			{
				timer = new System.Timers.Timer(SysConfig.getInstance().WIFICloseTime * 60 * 1000);
				timer.AutoReset = false;
				timer.Elapsed += Timer_Elapsed;
			}
			timer.Interval = SysConfig.getInstance().WIFICloseTime * 60 * 1000;
			timer.Start();
		}

		private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
		{
			StopService();
		}

		/// <summary>
		/// 获取本地Ip地址
		/// </summary>
		/// <returns></returns>
		public static string getHotSpotName()
		{
			//获取说有网卡信息
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in nics)
			{
				//判断是否为以太网卡
				//Wireless80211         无线网卡    Ppp     宽带连接
				//Ethernet              以太网卡   
				//这里篇幅有限贴几个常用的，其他的返回值大家就自己百度吧！
				if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
				{
					log.Info("Wireless80211----" + adapter.Id + "---" + adapter.Name + "---" + adapter.Description + "---");
					if (adapter.Id.StartsWith("wlx") || adapter.Id.StartsWith("wlan"))
					{
						return adapter.Id;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// 获取本地Ip地址
		/// </summary>
		/// <returns></returns>
		public static string getEthName()
		{
			//获取说有网卡信息
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface adapter in nics)
			{
				//判断是否为以太网卡
				//Wireless80211         无线网卡    Ppp     宽带连接
				//Ethernet              以太网卡   
				//这里篇幅有限贴几个常用的，其他的返回值大家就自己百度吧！
				if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
				{
					log.Info("Wireless80211----" + adapter.Id + "---" + adapter.Name + "---" + adapter.Description + "---");
					if (adapter.Id.StartsWith("eth"))
					{
						return adapter.Id;
					}
				}
			}
			return null;
		}
	}
}
