using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.NMEA;
using NtripShare.GNSS.Device.Sensor;
using NtripShare.GNSS.Device.Serial;
using NtripShare.GNSS.Service;
using System.Text;
using System.Timers;
using static NtripShare.GNSS.Device.NMEA.GPGGA;
using static System.Net.Mime.MediaTypeNames;

namespace NtripShare.GNSS.Device.Service
{
	public class ConfigService
	{
		static ILog log = LogManager.GetLogger("NtripShare", typeof(ConfigService));
		private static ConfigService _configService = null;
		private CustomSerialPort _customSerialPort = null;
		private ISensor _sensor = null;
		public bool ParseNmea { get; set; } = false;

		public NMEAHelper NMEAHelper { get; } = new NMEAHelper();
		public static ConfigService getInstance()
		{
			if (_configService == null)
			{
				_configService = new ConfigService();
			}
			return _configService;
		}

		/// <summary>
		/// 模块版本信息
		/// </summary>
		public SensorVersion SensorVersion { get; set; }

		/// <summary>
		/// 启动服务
		/// </summary>l
		public void StartService()
		{
			try
			{
				_customSerialPort = new CustomSerialPort(SysConfig.getInstance().ConfigSerialPort, SysConfig.getInstance().ConfigBaudRate);
				_customSerialPort.BufSize = 4096 * 100;
				_customSerialPort.ReceivedEvent += Data_ReceivedEvent;// Csp_DataReceived;
				_customSerialPort.Open();
				log.Info($"Service Open ConfigService Uart [{SysConfig.getInstance().ConfigSerialPort}] Succful!");

				Task.Run(() =>
				{
					initSensorV2(SysConfig.getInstance().SensorBrand);
					Thread.Sleep(2000);
					while (SensorVersion == null)
					{
						initSensorV2("UNICORE");
						Thread.Sleep(2000);
						if (SensorVersion != null)
						{
							SysConfig.getInstance().SensorBrand = "UNICORE";
							SysConfig.getInstance().saveConfig();
							break;
						}
						initSensorV2("COMNAV");
						Thread.Sleep(2000);
						if (SensorVersion != null)
						{
							SysConfig.getInstance().SensorBrand = "COMNAV";
							SysConfig.getInstance().saveConfig();
							break;
						}
						initSensorV2("NOVATEL");
						Thread.Sleep(2000);
						if (SensorVersion != null)
						{
							SysConfig.getInstance().SensorBrand = "NOVATEL";
							SysConfig.getInstance().saveConfig();
							break;
						}
						initSensorV2("HEMISHPERE");
						Thread.Sleep(2000);
						if (SensorVersion != null)
						{
							SysConfig.getInstance().SensorBrand = "HEMISHPERE";
							SysConfig.getInstance().saveConfig();
							break;
						}
						initSensorV2("TRIMBLE");
						Thread.Sleep(2000);
						if (SensorVersion != null)
						{
							SysConfig.getInstance().SensorBrand = "TRIMBLE";
							SysConfig.getInstance().saveConfig();
							break;
						}
						initSensorV2("UBLOXF9P");
						Thread.Sleep(2000);
						if (SensorVersion != null)
						{
							SysConfig.getInstance().SensorBrand = "UBLOXF9P";
							SysConfig.getInstance().saveConfig();
							break;
						}
					}
					initGNSSBoard();
					if (SysConfig.getInstance().WorkModeConfig.WorkMode == (int)WorkMode.QUICKBASE)
					{
						System.Timers.Timer timer = new System.Timers.Timer();
						timer.Enabled = true;
						timer.Interval = 2 * 60 * 1000; //执行间隔时间,单位为毫秒; 这里实际间隔为10分钟  
						timer.Start();
						timer.Elapsed += new System.Timers.ElapsedEventHandler(setQuickBase);
					}
					string msg = "";
					bool isCheck = LicenceCheck.Check(SysConfig.getInstance().SoftKey, SensorVersion.EfuseID, out msg);
					if (!isCheck)
					{
						log.Info($"Invalid Licence EfuseID [{SensorVersion.EfuseID}]");
					}
				});

			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}

		/// <summary>
		/// 设置快速基站
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void setQuickBase(object source, ElapsedEventArgs e)
		{
			List<GPGGA> list3 = NMEAHelper.GPGGABffer.Where(e => e.Quality == FixQuality.Rtk).ToList();
			if (list3.Count > 0)
			{
				double x = list3.Average(t => t.Longitude);
				double y = list3.Average(t => t.Latitude);
				double z = list3.Average(t => t.Altitude)+ list3.Average(t => t.GeoidalSeparation);
				WorkModeConfig workModeConfig = new WorkModeConfig();
				workModeConfig.BaseID = SysConfig.getInstance().WorkModeConfig.BaseID;
				workModeConfig.BaseLat = y;
				workModeConfig.BaseLon = x;
				workModeConfig.BaseHeight = z;
				workModeConfig.WorkMode = (int)WorkMode.BASE;
				workModeConfig.BaseLatType = SysConfig.getInstance().WorkModeConfig.BaseLatType;
				workModeConfig.BaseLonType = SysConfig.getInstance().WorkModeConfig.BaseLonType;
				List<byte[]> commond = _sensor.getWorkModeCommond(workModeConfig);
				SendCommond(commond);
				NtripClientService.getInstance().StopService();
				//NtripClient2Service.getInstance().StopService();
				//NtripClient3Service.getInstance().StopService();
				SysConfig.getInstance().WorkModeConfig.BaseLat = y;
				SysConfig.getInstance().WorkModeConfig.BaseLon = x;
				SysConfig.getInstance().WorkModeConfig.BaseHeight = z;
				SysConfig.getInstance().saveConfig();
				if (SysConfig.getInstance().NtripServerConfig.NtripEnable)
				{
					NtripSourceService.getInstance().StopService();
					Thread.Sleep(1000);
					NtripSourceService.getInstance().StartService();
				}

				if (SysConfig.getInstance().NtripServerConfig2.NtripEnable)
				{
					NtripSource2Service.getInstance().StopService();
					Thread.Sleep(1000);
					NtripSource2Service.getInstance().StartService();
				}

				if (SysConfig.getInstance().NtripServerConfig3.NtripEnable)
				{
					NtripSource3Service.getInstance().StopService();
					Thread.Sleep(1000);
					NtripSource3Service.getInstance().StartService();
				}
				return;
			}
			list3 = NMEAHelper.GPGGABffer.Where(e => e.Quality == FixQuality.Float).ToList();
			if (list3.Count > 0)
			{
				double x = list3.Average(t => t.Longitude);
				double y = list3.Average(t => t.Latitude);
				double z = list3.Average(t => t.Altitude)+list3.Average(t => t.GeoidalSeparation);
				WorkModeConfig workModeConfig = new WorkModeConfig();
				workModeConfig.BaseID = SysConfig.getInstance().WorkModeConfig.BaseID;
				workModeConfig.BaseLat = y;
				workModeConfig.BaseLon = x;
				workModeConfig.BaseHeight = z;
				workModeConfig.WorkMode = (int)WorkMode.BASE;
				workModeConfig.BaseLatType = SysConfig.getInstance().WorkModeConfig.BaseLatType;
				workModeConfig.BaseLonType = SysConfig.getInstance().WorkModeConfig.BaseLonType;
				List<byte[]> commond = _sensor.getWorkModeCommond(workModeConfig);
				SendCommond(commond);
				NtripClientService.getInstance().StopService();
				//NtripClient2Service.getInstance().StopService();
				//NtripClient3Service.getInstance().StopService();
				SysConfig.getInstance().WorkModeConfig.BaseLat = y;
				SysConfig.getInstance().WorkModeConfig.BaseLon = x;
				SysConfig.getInstance().WorkModeConfig.BaseHeight = z;
				SysConfig.getInstance().saveConfig();

				if (SysConfig.getInstance().NtripServerConfig.NtripEnable) 
				{
					NtripSourceService.getInstance().StopService();
					NtripSourceService.getInstance().StartService();
				}

				if (SysConfig.getInstance().NtripServerConfig2.NtripEnable)
				{
					NtripSource2Service.getInstance().StopService();
					NtripSource2Service.getInstance().StartService();
				}

				if (SysConfig.getInstance().NtripServerConfig3.NtripEnable)
				{
					NtripSource3Service.getInstance().StopService();
					NtripSource3Service.getInstance().StartService();
				}
				return;
			}
			System.Timers.Timer timer = new System.Timers.Timer();
			timer.Enabled = true;
			timer.Interval = 2 * 60 * 1000; //执行间隔时间,单位为毫秒; 这里实际间隔为10分钟  
			timer.Start();
			timer.Elapsed += new System.Timers.ElapsedEventHandler(setQuickBase);
		}

		/// <summary>
		/// 保存卫星追踪命令至接收机
		/// </summary>
		public void initGNSSBoard()
		{
			SysConfig mosConfig = SysConfig.getInstance();

			List<byte[]> commond = _sensor.getInitCommond(mosConfig);
			SendCommond(commond);
		}

		/// <summary>
		/// 保存卫星追踪命令至接收机
		/// </summary>
		public void setTracConfig()
		{
			SysConfig mosConfig = SysConfig.getInstance();

			List<byte[]> commond = _sensor.getTrackCommond(mosConfig.TrackConfig);
			SendCommond(commond);
		}

		/// <summary>
		/// 设置工作模式
		/// </summary>
		public void setWorkModeConfig()
		{
			SysConfig mosConfig = SysConfig.getInstance();
			List<byte[]> commond = _sensor.getWorkModeCommond(mosConfig.WorkModeConfig);
			SendCommond(commond);
			if (mosConfig.WorkModeConfig.WorkMode == (int)WorkMode.QUICKBASE)
			{
				System.Timers.Timer timer = new System.Timers.Timer();
				timer.Enabled = true;
				timer.Interval = 2 * 60 * 1000; //执行间隔时间,单位为毫秒; 这里实际间隔为10分钟  
				timer.Start();
				timer.Elapsed += new System.Timers.ElapsedEventHandler(setQuickBase);
			}
		}

		/// <summary>
		/// 设置工作模式
		/// </summary>
		public void setComConfig()
		{
			SysConfig mosConfig = SysConfig.getInstance();
			List<byte[]> commond = _sensor.getComOutCommond(mosConfig.ComConfig, mosConfig.OutConfig);
			SendCommond(commond);
		}

		/// <summary>
		/// 设置输出
		/// </summary>
		public void setOutConfig()
		{
			SysConfig mosConfig = SysConfig.getInstance();
			List<byte[]> commond = _sensor.getOutCommond(mosConfig);
			SendCommond(commond);
			commond = _sensor.getComOutCommond(mosConfig.ComConfig, mosConfig.OutConfig);
			SendCommond(commond);
		}


		/// <summary>
		/// 设置工作模式
		/// </summary>
		public void setAntennaConfig()
		{
			SysConfig mosConfig = SysConfig.getInstance();
			List<byte[]> commond = _sensor.getAntennaCommond(mosConfig.AntennaConfig);
			SendCommond(commond);
		}


		/// <summary>
		/// 初始化模块指令
		/// </summary>
		private void initSensor()
		{
			SysConfig mosConfig = SysConfig.getInstance();
			if (mosConfig.SensorBrand.ToUpper().StartsWith("UNICORE"))
			{
				log.Info($"initSensor UNICORE");
				_sensor = new UniCore();
			}
			if (mosConfig.SensorBrand.ToUpper().StartsWith("TRIMBLE"))
			{
				log.Info($"initSensor TRIMBLE");
				_sensor = new Trimble();
			}
			if (mosConfig.SensorBrand.ToUpper().StartsWith("NOVATEL"))
			{
				log.Info($"initSensor NOVALTEL");
				_sensor = new NovAtel();
			}
			if (mosConfig.SensorBrand.ToUpper().StartsWith("UBLOXF9P"))
			{
				log.Info($"initSensor UBLOXF9P");
				_sensor = new UbloxF9P();
			}
			if (mosConfig.SensorBrand.ToUpper().StartsWith("HEMISHPERE"))
			{
				log.Info($"initSensor HEMISHPERE");
				_sensor = new Hemisphere();
			}
			if (_sensor != null)
			{
				SendCommond(_sensor.getVersionCommond(mosConfig));
			}

		}

		/// <summary>
		/// 初始化
		/// </summary>
		private void initSensorV2(string brand)
		{
			SysConfig mosConfig = SysConfig.getInstance();
			if (brand.ToUpper().StartsWith("UNICORE"))
			{
				log.Info($"initSensor UNICORE");
				_sensor = new UniCore();
			}
			if (brand.StartsWith("TRIMBLE"))
			{
				log.Info($"initSensor TRIMBLE");
				_sensor = new Trimble();
			}
			if (brand.StartsWith("NOVATEL"))
			{
				log.Info($"initSensor NOVALTEL");
				_sensor = new NovAtel();
			}
			if (brand.StartsWith("UBLOXF9P"))
			{
				log.Info($"initSensor UBLOXF9P");
				_sensor = new UbloxF9P();
			}
			if (brand.StartsWith("HEMISHPERE"))
			{
				log.Info($"initSensor HEMISHPERE");
				_sensor = new Hemisphere();
			}
			if (_sensor != null)
			{
				SendCommond(_sensor.getVersionCommond(mosConfig));
			}
		}

		/// <summary>
		/// 配置串口收到数据
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="bytes"></param>
		private void Data_ReceivedEvent(object sender, byte[] bytes)
		{
			try
			{
				if (_sensor != null)
				{
					//string sss = "";
					//for (int i = 0; i < bytes.Length; i++) {
					//	sss += bytes[i];
					//}
					string mm = Encoding.Default.GetString(bytes);
					log.Info(mm);
					_sensor.dealConfigMessage(bytes);
				}
			}
			catch (Exception ex)
			{
				log.Error(ex.Message);
			}
		}


		/// <summary>
		/// 发送数据
		/// </summary>
		/// <param name="bytes"></param>
		public void SendCommond(List<byte[]> commond)
		{
			foreach (byte[] bytes in commond)
			{
				log.Info("SendCommond:" + Encoding.Default.GetString(bytes));
				_customSerialPort.Write(bytes);
			}
		}

		/// <summary>
		/// 发送数据
		/// </summary>
		/// <param name="bytes"></param>
		public void SendData(string bytes)
		{
			_customSerialPort.Write(bytes);
		}

		/// <summary>
		/// 发送数据
		/// </summary>
		/// <param name="bytes"></param>
		public void SendData(byte[] bytes)
		{
			//log.Info(Encoding.Default.GetString(bytes));
			_customSerialPort.Write(bytes);
		}

		/// <summary>
		/// 处理NMEA数据
		/// </summary>
		/// <param name="data"></param>
		public void dealNmeaData(string line)
		{
			if (line.Contains("GGA"))
			{
				NtripClientService.getInstance().SendData(line + "\r\n");
				//NtripClient2Service.getInstance().SendData(line + "\r\n");
				//NtripClient3Service.getInstance().SendData(line + "\r\n");
			}

			NMEAHelper.ParseNmeaString(line);

			if (MqttService.getInstance().IsServiceStarted && MqttService.getInstance().IsConnected)
			{
				if ("nmea".Equals(SysConfig.getInstance().MqttConfig.MqttStream))
				{
					MqttService.getInstance().SendData(line + "\r\n");
				}
			}
			if (TcpService.getInstance().IsServiceStarted && TcpService.getInstance().IsConnected)
			{
				if ("nmea".Equals(SysConfig.getInstance().TcpConfig.TcpStream))
				{
					TcpService.getInstance().SendData(line + "\r\n");
				}
			}
			if (Tcp2Service.getInstance().IsServiceStarted && Tcp2Service.getInstance().IsConnected)
			{
				if ("nmea".Equals(SysConfig.getInstance().TcpConfig2.TcpStream))
				{
					Tcp2Service.getInstance().SendData(line + "\r\n");
				}
			}
			if (Tcp3Service.getInstance().IsServiceStarted && Tcp3Service.getInstance().IsConnected)
			{
				if ("nmea".Equals(SysConfig.getInstance().TcpConfig3.TcpStream))
				{
					Tcp3Service.getInstance().SendData(line + "\r\n");
				}
			}
		}
	}
}
