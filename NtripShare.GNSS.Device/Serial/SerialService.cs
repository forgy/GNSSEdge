//using log4net;
//using NtripShare.GNSS.Entity;
//using System.Diagnostics;

//namespace NtripShare.GNSS.Device.Serial
//{
//    /// <summary>
//    /// 串口服务
//    /// </summary>
//    public class ConfigSerialService
//    {
//        private static object loc = new object();
//        static ILog log = LogManager.GetLogger("NtripShare", typeof(ConfigSerialService));
//        /// <summary>
//        /// 监测站串口
//        /// </summary>
//        public Dictionary<string, CustomSerialPort> SerialPortDic { get; set; } = new Dictionary<string, CustomSerialPort>();

//        private static ConfigSerialService _serialService;

//        // Do this when you start your application
//        static int mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

//        public static bool IsMainThread()
//        {
//            return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;
//        }


//        public static ConfigSerialService getInstance()
//        {
//            if (_serialService == null)
//            {
//                _serialService = new ConfigSerialService();
//            }
//            return _serialService;
//        }

//        private byte[] tempData = new byte[0];

//        /// <summary>
//        /// 启动串口服务
//        /// </summary>
//        /// <param name="baseComName"></param>
//        /// <param name="roverComNames"></param>
//        /// <param name="baudRate"></param>
//        public void StartService()
//        {
//            try
//            {
//                log.Info("Run Mode:Service\r\n");
//                SerialPortDic.Clear();
//                CustomSerialPort BaseSerialPort = new CustomSerialPort(0, MosConfig.getInstance().BaseSerialPort, SysConfig.getInstance().BaseBaudRate);
//                BaseSerialPort.ReceivedEvent += Data_ReceivedEvent;// Csp_DataReceived;
//                SerialPortDic.Add(SysConfig.getInstance().BaseSerialPort, BaseSerialPort);
//                try
//                {
//                    BaseSerialPort.Open();
//                    log.Info($"Service Open BaseStation Uart [{SysConfig.getInstance().BaseSerialPort}] Succful!");
//                }
//                catch (Exception ex)
//                {
//                    log.Info($"RunService Open BaseStation Uart [{SysConfig.getInstance().BaseSerialPort}] Exception:{ex}");
//                }
//                if (!SerialPortDic.ContainsKey(SysConfig.getInstance().OutSerialPort))
//                {
//                    BaseSerialPort = new CustomSerialPort(0, SysConfig.getInstance().OutSerialPort, SysConfig.getInstance().OutBaudRate);
//                    BaseSerialPort.ReceivedEvent += Out_ReceivedEvent;// Csp_DataReceived;
//                    SerialPortDic.Add(SysConfig.getInstance().OutSerialPort, BaseSerialPort);
//                    try
//                    {
//                        BaseSerialPort.Open();
//                        log.Info($"Service Open LogSerial Uart [{SysConfig.getInstance().OutSerialPort}] Succful!");
//                    }
//                    catch (Exception ex)
//                    {
//                        log.Info($"RunService Open LogSerial Uart [{SysConfig.getInstance().OutSerialPort}] Exception:{ex}");
//                    }
//                }
//                else
//                {
//                    SerialPortDic[SysConfig.getInstance().OutSerialPort].ReceivedEvent += Out_ReceivedEvent;// Csp_DataReceived;
//                }


//                foreach (RoverConfig roverConfig in SysConfig.getInstance().RoverConfigs)
//                {
//                    if (SerialPortDic.ContainsKey(roverConfig.RoverSerialPort))
//                    {
//                        continue;
//                    }
//                    try
//                    {
//                        CustomSerialPort RoverSerialPort = new CustomSerialPort(roverConfig.ID, roverConfig.RoverSerialPort, roverConfig.BaudRate);
//                        RoverSerialPort.ReceivedEvent += Rover_ReceivedEvent;// Csp_DataReceived;
//                        RoverSerialPort.Open();
//                        SerialPortDic.Add(roverConfig.RoverSerialPort, RoverSerialPort);
//                        log.Info($"Service Open RoverStation Uart [{roverConfig.RoverSerialPort}] Succful!");
//                    }
//                    catch (Exception ex)
//                    {
//                        log.Info($"RunService Open RoverStation Uart [{roverConfig.RoverSerialPort}] Exception:{ex}");
//                    }
//                }

//                addBaseLine(SysConfig.getInstance());
//            }
//            catch (Exception ex)
//            {
//                log.Error(ex);
//            }
//        }

//        /// <summary>
//        /// 停止串口服务
//        /// </summary>
//        public void StopService()
//        {
//            foreach (var csp in SerialPortDic.Values)
//            {
//                if (csp.IsOpen)
//                {
//                    csp.Close();
//                }
//                csp.ReceivedEvent -= Rover_ReceivedEvent;//Csp_DataReceived;
//            }
//            SerialPortDic.Clear();
//        }

//        /// <summary>
//        /// 清除缓存与数据文件
//        /// </summary>
//        private void clear()
//        {

//        }

//        /// <summary>
//        /// 设置监测站参数
//        /// </summary>
//        private bool setRoverConfig(CustomSerialPort customSerialPort, string message)
//        {
//            try
//            {
//                string[] date = message.TrimEnd().Split(" ");
//                string[] ro = date[date.Length - 1].Split(",");
//                int id = int.Parse(ro[0]);
//                foreach (RoverConfig roverConfig in SysConfig.getInstance().RoverConfigs)
//                {
//                    if (roverConfig.ID == id)
//                    {
//                        if (message.StartsWith("$NS-SET-ROVER-StationName"))
//                        {
//                            if (ro.Length > 1)
//                            {
//                                roverConfig.StationName = ro[ro.Length - 1];
//                                customSerialPort.WriteLine("$NS-OK");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-RoverFormat"))
//                        {
//                            if (date.Length > 1)
//                            {
//                                roverConfig.RoverFormat = ro[ro.Length - 1];
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-RoverTCPPort"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc == 0 && vc <= 65536)
//                            {
//                                roverConfig.RoverTCPPort = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-ProcessTime"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc >= 1 && vc <= 1440)
//                            {
//                                roverConfig.ProcessTime = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-DataTimeLength"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc >= 20 && vc <= 1440)
//                            {
//                                roverConfig.DataTimeLength = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-SamplingInterval"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc >= 1)
//                            {
//                                roverConfig.SamplingInterval = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-MaskingAngle"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc >= 0 && vc < 45)
//                            {
//                                roverConfig.MaskingAngle = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-UseGPS"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc == 0 || vc == 1)
//                            {
//                                roverConfig.UseGPS = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-UseBD"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc == 0 || vc == 1)
//                            {
//                                roverConfig.UseBD = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-UseGlonass"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc == 0 || vc == 1)
//                            {
//                                roverConfig.UseGlonass = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-UseGlonass"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc == 0 || vc == 1)
//                            {
//                                roverConfig.UseGalileo = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-UseGlonass"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc >= 1 && vc < 50)
//                            {
//                                roverConfig.Precision = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else if (message.StartsWith("$NS-SET-ROVER-UseGlonass"))
//                        {
//                            int vc = int.Parse(ro[ro.Length - 1]);
//                            if (vc == 0 || vc == 1)
//                            {
//                                roverConfig.UserFilter = vc;
//                                customSerialPort.WriteLine("$NS-OK\r\n");
//                                return true;
//                            }
//                            else
//                            {
//                                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                            }
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                customSerialPort.WriteLine("$NS-ERROR\r\n");
//                log.Info(e);
//            }
//            return false;
//        }

//        /// <summary>
//        /// 设置系统参数
//        /// </summary>
//        /// <param name="customSerialPort"></param>
//        /// <param name="message"></param>
//        private bool setSysConfig(CustomSerialPort customSerialPort, string message)
//        {
//            try
//            {
//                if (message.StartsWith("$NS-SET-SYS-SoftKey"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        SysConfig.getInstance().SoftKey = date[date.Length - 1];
//                        customSerialPort.WriteLine("$NS-OK\r\n");
//                        return true;
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-BaseFormat"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        SysConfig.getInstance().BaseFormat = date[date.Length - 1];
//                        customSerialPort.WriteLine("$NS-OK\r\n");
//                        return true;
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-IsUseTcp"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);
//                        if (vc == 0 && vc <= 65536)
//                        {
//                            SysConfig.getInstance().IsUseTcp = vc;
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-BaseTCPPort"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);
//                        if (vc == 0 && vc <= 65536)
//                        {
//                            SysConfig.getInstance().BaseTCPPort = vc;
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-OutTCPPort"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);
//                        if (vc == 0 || vc == 1)
//                        {
//                            SysConfig.getInstance().OutTCPPort = vc;
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-DataProtocol"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);
//                        if (vc == 0 || vc == 1)
//                        {
//                            SysConfig.getInstance().DataProtocol = vc;
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-ProjMode"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);
//                        if (vc == 0 || vc == 1)
//                        {
//                            SysConfig.getInstance().ProjMode = vc;
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                            return true;
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-CentralMeridian"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);

//                        SysConfig.getInstance().CentralMeridian = vc;
//                        customSerialPort.WriteLine("$NS-OK\r\n");
//                        return true;
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-ProjH"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);

//                        SysConfig.getInstance().ProjH = vc;
//                        customSerialPort.WriteLine("$NS-OK\r\n");
//                        return true;
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-IsStatic"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);
//                        if (vc == 0 || vc == 1)
//                        {
//                            SysConfig.getInstance().IsStatic = vc;
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                            return true;
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-IsDynamic"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);
//                        if (vc == 0)
//                        {
//                            SysConfig.getInstance().IsDynamic = vc;
//                            RTKService.getInstance().StopService();
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                            return true;
//                        }
//                        else if (vc == 1)
//                        {
//                            SysConfig.getInstance().IsDynamic = vc;
//                            RTKService.getInstance().StartService();
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                            return true;
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }
//                else if (message.StartsWith("$NS-SET-SYS-IsLogOut"))
//                {
//                    string[] date = message.TrimEnd().Split(" ");
//                    if (date.Length > 1)
//                    {
//                        int vc = int.Parse(date[date.Length - 1]);
//                        if (vc == 0)
//                        {
//                            SysConfig.getInstance().IsLogOut = vc;
//                            StaticService.getInstance().StopService();
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                            return true;
//                        }
//                        else if (vc == 1)
//                        {
//                            SysConfig.getInstance().IsLogOut = vc;
//                            StaticService.getInstance().StartService();
//                            customSerialPort.WriteLine("$NS-OK\r\n");
//                            return true;
//                        }
//                        else
//                        {
//                            customSerialPort.WriteLine("$NS-ERROR\r\n");
//                        }
//                    }
//                    else
//                    {
//                        customSerialPort.WriteLine("$NS-ERROR\r\n");
//                    }
//                }

//            }
//            catch (Exception e)
//            {
//                customSerialPort.WriteLine("$NS-ERROR\r\n");
//            }
//            return false;
//        }

//        /// <summary>
//        /// 基站串口收到数据
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="bytes"></param>
//        private void Data_ReceivedEvent(object sender, byte[] bytes)
//        {
//            try
//            {
//                if (SysConfig.getInstance().DataProtocol == 0)
//                {
//                    FileSaver.getInstance().saveBaseFileData(0, bytes);
//                    if (DateTime.Now < SysConfig.getInstance().LastPositionTime.AddHours(1))
//                    {
//                        return;
//                    }
//                    RTKStation station = StationManager.getStation("Base0");
//                    if (station != null)
//                    {
//                        station.IsRun = true;
//                        station.ReceiveNetData(bytes);
//                    }
//                }
//                else
//                {
//                    lock (loc)
//                    {
//                        DealRawData(bytes);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.WriteLine(ex.Message);
//            }
//        }
//    }
//}
