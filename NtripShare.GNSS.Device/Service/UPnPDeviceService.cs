using log4net;
using NtripShare.GNSS.Device.Config;
using OpenSource.UPnP;
using System.Dynamic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

namespace NtripShare.GNSS.Device.Service
{
    public class UPnPDeviceService : BaseService
    {
        private static ILog log = LogManager.GetLogger("NtripShare", typeof(UPnPDeviceService));

        private UPnPDevice localDevice = null;

        private static UPnPDeviceService Instance = null;

		/// <summary>
		/// 设置自停止时间
		/// </summary>
		//public int CloseTime { get; set; }
		System.Timers.Timer timer = null;

		private System.Timers.Timer t = new System.Timers.Timer(1000 * 30);//实例化Timer类，设置间隔时间为10000毫秒；
        public static UPnPDeviceService getInstance()
        {
            if (Instance == null)
            {
                Instance = new UPnPDeviceService();
            }
            return Instance;
        }

        private UPnPDeviceService()
        {
            try
            {
                SysConfig mosConfig = SysConfig.getInstance();
                localDevice = UPnPDevice.CreateRootDevice( /* expiration */ 1000 * 30, /* version*/ 1, /* web dir */ null);
                localDevice.StandardDeviceType = "urn:NtripShare:device:controllee";
                localDevice.UniqueDeviceName = $"NtripShare-{mosConfig.SoftKey}";
              
                //localDevice.Icon = null;
                //localDevice.HasPresentation = true;
                localDevice.PresentationURL = $"http://{GetLocalIp()}:80";
                localDevice.Major = 1;
                localDevice.Minor = 0;
                if (ConfigService.getInstance().SensorVersion != null)
                {
                    localDevice.FriendlyName = $"{mosConfig.Model},{ConfigService.getInstance().SensorVersion.EfuseID}:NtripShare";
                    localDevice.SerialNumber = ConfigService.getInstance().SensorVersion.EfuseID;
                }
                else {
                    localDevice.FriendlyName = $"{mosConfig.Model}:NtripShare";
                    localDevice.SerialNumber = mosConfig.SoftKey;
                }
              
                localDevice.ModelNumber = Assembly.GetEntryAssembly().GetName().Version.ToString();
                localDevice.Manufacturer = "Mr.Peng@NtripShare";
                localDevice.ManufacturerURL = "http://www.ntripshare.com";
                localDevice.ModelName = $"GNSS Reciever {mosConfig.Model}";
                localDevice.ModelDescription = $"NtripShare GNSS Edge Sys {Assembly.GetEntryAssembly().GetName().Version.ToString()}";

                localDevice.ModelURL = new Uri("http://www.ntripshare.com");

                localDevice.UserAgentTag = "NtripShare";
                localDevice.BaseURL = new Uri($"http://{GetLocalIp()}:80");

                // Create an instance of the BasicEvent service
                dynamic instance = new ExpandoObject();

                // Declare the "BasicEvent1" service
                var service = new UPnPService(
                    // Version
                    1.0,
                    // Service ID
                    "urn:NtripShare:serviceId:basicevent1",
                    // Service Type
                    "urn:NtripShare:service:basicevent:1",
                    // Standard Service?
                    true,
                    // Service Object Instance
                    instance
                );
                service.ControlURL = "/upnp/control/basicevent1";
                service.EventURL = "/upnp/event/basicevent1";
                service.SCPDURL = "/eventservice.xml";

                string stateVarName = "BinaryState";
                var stateVariable = new UPnPStateVariable(stateVarName, typeof(bool), true);
                stateVariable.AddAssociation("GetBinaryState", stateVarName);
                stateVariable.AddAssociation("SetBinaryState", stateVarName);
                stateVariable.Value = false;
                service.AddStateVariable(stateVariable);

                instance.GetBinaryState = new Func<bool>(() => (bool)service.GetStateVariable(stateVarName));
                instance.SetBinaryState = new Action<int>((BinaryState) =>
                {
                    Console.WriteLine("SetBinaryState({0})", BinaryState);
                    service.SetStateVariable(stateVarName, BinaryState != 0);
                });

                // Add the methods
                service.AddMethod("GetBinaryState", stateVarName);
                service.AddMethod("SetBinaryState", stateVarName);

                // Add the service
                localDevice.AddService(service);
                // Start the WeMo switch device UPnP simulator
                t.Elapsed += new System.Timers.ElapsedEventHandler(Execute);//到达时间的时候执行事件；
                t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
                t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }
            catch (Exception e) { 
                log.Error(e);
                log.Error(e.StackTrace);
            }

		}

		public override void StopService()
        {
            IsServiceStarted = false;
            t.Stop();
            try
            {
                localDevice.StopDevice();
				if (timer != null)
				{
					timer.Stop();
				}
			}
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public override void StartService()
        {
            IsServiceStarted = true;
            try
            {
                localDevice.StartDevice();
            }
            catch (Exception e)
            {

            }
            t.Start();
			if (SysConfig.getInstance().UPNPCloseTime == 0)
			{
				return;
			}
			if (timer == null)
			{
				timer = new System.Timers.Timer(SysConfig.getInstance().UPNPCloseTime * 30 * 1000);
				timer.AutoReset = false;
				timer.Elapsed += Timer_Elapsed;
			}
			timer.Interval = SysConfig.getInstance().UPNPCloseTime * 30 * 1000;
			timer.Start();
		}

		private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
		{
			StopService();
		}

		/// <summary>
		/// 官博信息
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		public void Execute(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                localDevice.Advertise();
            }
            catch (Exception e2)
            {

            }
        }

        /// <summary>
        /// 获取本地Ip地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIp()
        {
			string output2 = "";
			string output = "";
			foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (item.Name.ToLower() == "eth0")
				{
					foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
					{
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							output = ip.Address.ToString();
						}
					}
				}
				if (item.Name.ToLower() == "wlan0")
				{
					foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
					{
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							output2 = ip.Address.ToString();
						}
					}
				}
			}
			log.Info("IP Address = " + output+"-"+ output2);
            if (string.IsNullOrEmpty(output)) {
                return output2;

			}
            return output;
        }
    }
}
