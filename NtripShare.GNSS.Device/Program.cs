//using log4net;
//using log4net.Config;
//using log4net.Repository;
//using NtripShare.GNSS.Device.Helper;
//using System.Reflection;

//namespace NtripShare.GNSS.Device.Serial
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {


//            ILoggerRepository repository = LogManager.CreateRepository("NtripShare");
//            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));



//            DataChecker lc = new DataChecker();

//            DbLocalProvider.UseSqlLite = false;
//            MosConfig mosConfig = MosConfigDao.getInstance().getConfig();
//            while (mosConfig == null || mosConfig.DeviceKey == "" || mosConfig.DeviceKey == null)
//            {
//                Thread.Sleep(5000);
//            }
//            //if (mosConfig.Key == null) {
//            //    mosConfig.Key = lc.GetMachineCode();
//            //}

//            ILog log = LogManager.GetLogger("NtripShare", typeof(Program));
//            //MosConfigDao.getInstance().updateConfig(MosConfig.getInstance());
//            ShowWelcome(mosConfig);
//            try
//            {
//                MqttService metroClient = new MetroClient(mosConfig.MqttIP, 1883, mosConfig.DeviceKey);
//                metroClient._TryContinueConnect();
//            }
//            catch (Exception ex)
//            {
//                log.Error(ex.Message);
//            }

//            NtripService.getInstance().RunClientAsync().Wait();
//            //NettyClientProxy.getInstance().RunClientAsync().Wait();
//        }


//        private static void ShowWelcome(MosConfig mosConfig)
//        {
//            ILog log = LogManager.GetLogger("NtripShare", typeof(Program));
//            log.Debug($"NtripShare MosEdge边缘计算软件");
//            log.Debug($"Version:{Assembly.GetEntryAssembly().GetName().Version}");
//            log.Debug($"Author: Mr.Peng");
//            log.Debug($"Email: NtripShare@163.com");
//            log.Debug($"System info:{Environment.OSVersion}");
//            log.Debug($"Environment.Version:{Environment.Version}");
//            log.Info($"Key:{mosConfig.DeviceKey}");
//            log.Debug($"Server:{mosConfig.ServerIP}");
//            log.Debug($"Port:{mosConfig.ServerPort}");
//        }
//    }
//}