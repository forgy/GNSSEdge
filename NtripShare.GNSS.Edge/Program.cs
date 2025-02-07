using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting.Internal;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;
using NtripShare.GNSS.Edge;
using NtripShare.GNSS.Service;
using NtripShare.Mos.CoreV2.Service;
using System.IO.Compression;
using System.Reflection;
using System.Text;

public class Program
{
	public static void startEdge()
	{
		ILoggerRepository repository = LogManager.CreateRepository("NtripShare");
		XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
		
		ShowWelcome();
		SysConfig.initConfig();
		SysConfig sysConfig = SysConfig.getInstance();
		NetService.getInstance().StartService();
		ConfigService.getInstance().StartService();
		DataService.getInstance().StartService();

		if (sysConfig.MqttConfig.MqttEnable) 
		{
			MqttService.getInstance().StartService();
        }
		if (sysConfig.TcpConfig.TcpEnable) 
		{
			TcpService.getInstance().StartService();
		}
		
		if (sysConfig.TcpConfig2 !=null && sysConfig.TcpConfig2.TcpEnable)
		{
			Tcp2Service.getInstance().StartService();
		}
		if (sysConfig.TcpConfig2 != null && sysConfig.TcpConfig3.TcpEnable)
		{
			Tcp3Service.getInstance().StartService();
		}

		if (sysConfig.NtripClientConfig.NtripEnable)
        {
            NtripClientService.getInstance().StartService();
        }
		//if (sysConfig.NtripClientConfig2 != null && sysConfig.NtripClientConfig2.NtripEnable)
		//{
		//	NtripClient2Service.getInstance().StartService();
		//}
		//if (sysConfig.NtripClientConfig3 != null && sysConfig.NtripClientConfig3.NtripEnable)
		//{
		//	NtripClient3Service.getInstance().StartService();
		//}
		if (sysConfig.NtripServerConfig.NtripEnable)
        {
            NtripSourceService.getInstance().StartService();
        }
		if (sysConfig.NtripServerConfig2 != null && sysConfig.NtripServerConfig2.NtripEnable)
		{
			NtripSource2Service.getInstance().StartService();
		}
		if (sysConfig.NtripServerConfig3 != null && sysConfig.NtripServerConfig3.NtripEnable)
		{
			NtripSource3Service.getInstance().StartService();
		}
		if (sysConfig.NtripCasterConfig != null && sysConfig.NtripCasterConfig.NtripEnable)
		{
			NtripCasterService.getInstance().StartService();
		}
		if (sysConfig.UPNP)
        {
            UPnPDeviceService.getInstance().StartService();
        }
        if (sysConfig.WIFI)
        {
            WifiService.getInstance().StartService();
        }
		if (sysConfig.SIM)
		{
			SimService.getInstance().StartService();
		}
		else {
			SimService.getInstance().StopService();
		}
    }

	private static void ShowWelcome()
	{
		ILog log = LogManager.GetLogger("NtripShare", typeof(Program));
		log.Debug($"NtripShare GNSS高精度接收机配置系统");
		log.Debug($"Version:{Assembly.GetEntryAssembly().GetName().Version}");
		log.Debug($"Author: Mr.Peng");
		log.Debug($"Email: NtripShare@163.com");
		log.Debug($"System info:{Environment.OSVersion}");
		log.Debug($"Environment.Version:{Environment.Version}");
	}

	public static void Main(string[] args)
	{
		startEdge();
		CreateWebHostBuilder(args).Build().Run();
	}

	public static void checkUpdate() 
	{
		string dir = Environment.CurrentDirectory + "/Update";
		if (Directory.Exists(dir)) {
			string[] filedir = Directory.GetFiles(dir, ".bin", SearchOption.TopDirectoryOnly);
			if(filedir.Length > 0) {
				var zipPath = filedir[0];
				var newPath = Environment.CurrentDirectory;
				ZipFile.ExtractToDirectory(newPath, zipPath, Encoding.UTF8, true);
				foreach (string file in filedir)
				{
					File.Delete(file);
				}
			}
			
		}
	}

	public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			 WebHost.CreateDefaultBuilder(args).UseUrls("http://*:80").UseStartup<Startup>();
}