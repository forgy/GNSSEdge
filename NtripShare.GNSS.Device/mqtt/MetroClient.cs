
//using log4net;
//using System.Text;
//using MQTTnet.Client;
//using MQTTnet;
//using MQTTnet.Protocol;
//using MQTTnet.Server;
//using NtripShare.GNSS.Entity;
//using NtripShare.GNSS.Dao;

//namespace NtripShare.GNSS.Device.mqtt
//{

//    public class MqttService
//	{
//		private static ILog log = LogManager.GetLogger("NtripShare", typeof(MqttService));
//		IMqttClient _MqttClient;
//		bool IsConnected = false;
//		//bool _ToClose = false;
//		int _TryCount = 0;
//		string serverIp = "mos.ntripshare.com";
//		int serverPort = 1883;
//		string _MqttUsername = "ns";
//		string _MqttClientId;
//		string _MqttUserpass = "ntripshare2020";


//		public MqttService(string ip, int port, string clientId)
//		{
//			serverIp = ip;
//			serverPort = port;
//			_MqttClientId = clientId;
//		}


//		// 自动重连主体
//		public void _TryContinueConnect()
//		{
//			if (IsConnected) return;

//			Thread retryThread = new Thread(new ThreadStart(delegate
//			{
//				//while (_MqttClient == null || !_MqttClient.IsConnected)
//				//{
//					SysConfig mosConfig = MosConfigDao.getInstance().getConfig();
//					serverIp = mosConfig.MqttIP;

//					if (_MqttClient == null)
//					{
//						_BuildClient();
//						Thread.Sleep(3000);
//					}

//					try
//					{
//						_TryCount++;
//						_Connect();
//					}
//					catch (Exception ce)
//					{
//						log.Debug("re connect exception:" + ce.Message);
//					}

//					// 如果还没连接不符合结束条件则睡2秒
//					//if (!_MqttClient.IsConnected)
//					//{
//					//	Thread.Sleep(2000);
//					//}
//				//}
//			}));

//			retryThread.Start();
//		}

//		// 实例化客户端
//		private void _BuildClient()
//		{
//			try
//			{
//				_MqttClient = new MqttFactory().CreateMqttClient();
//				//_MqttClient = new MqttClient(serverIp, serverPort, false, null, null, MqttSslProtocols.TLSv1_2);

//			}
//			catch (Exception e)
//			{
//				log.Debug("build client error:" + e.Message);
//				return;
//			}

//			// 消息到达事件绑定
//			_MqttClient.ConnectedAsync += _mqttClient_ConnectedAsync; // 客户端连接成功事件
//			_MqttClient.DisconnectedAsync += _mqttClient_DisconnectedAsync; // 客户端连接关闭事件
//			_MqttClient.ApplicationMessageReceivedAsync += _MqttClient_ApplicationMessageReceivedAsync; // 收到消息事件

//		}

//		/// <summary>
//		/// 客户端连接关闭事件
//		/// </summary>
//		/// <param name="arg"></param>
//		/// <returns></returns>
//		private Task _mqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
//		{
//			log.Debug("MqttClient ConnectionClosed");
//			Thread.Sleep(3000);
//			_TryContinueConnect();
//			return Task.CompletedTask;
//		}

//		/// <summary>
//		/// 客户端连接成功事件
//		/// </summary>
//		/// <param name="arg"></param>
//		/// <returns></returns>
//		private Task _mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
//		{
//			log.Debug($"客户端已连接服务端……");

//			// 订阅消息主题
//			// MqttQualityOfServiceLevel: （QoS）:  0 最多一次，接收者不确认收到消息，并且消息不被发送者存储和重新发送提供与底层 TCP 协议相同的保证。
//			// 1: 保证一条消息至少有一次会传递给接收方。发送方存储消息，直到它从接收方收到确认收到消息的数据包。一条消息可以多次发送或传递。
//			// 2: 保证每条消息仅由预期的收件人接收一次。级别2是最安全和最慢的服务质量级别，保证由发送方和接收方之间的至少两个请求/响应（四次握手）。
//			_MqttClient.SubscribeAsync("Commond@turnOn", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@turnOff", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@openLaser", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@closeLaser", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@changeFace", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@search", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@study", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@study2", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@rotate", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@start", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@tilt", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@setStudyMode", MqttQualityOfServiceLevel.AtLeastOnce);
//			_MqttClient.SubscribeAsync("Commond@setSurveyMode", MqttQualityOfServiceLevel.AtLeastOnce);

//			log.Debug("MqttClient Connected！");
//			return Task.CompletedTask;
//		}

//		private Task _MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
//		{
//            log.Debug("MqttClient Topic:" + e.ApplicationMessage.Topic);
//            Task.Factory.StartNew(() =>
//			{
//				lock (loc)
//				{
//					string[] para = Encoding.Default.GetString(e.ApplicationMessage.Payload).Split(',');
					

//				}
//			});
//			return Task.CompletedTask;
//		}

//		object loc = new object();

//		//private void _MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
//		//{
//		//	lock (loc)
//		//	{
//		//		log.Debug("MqttClient Topic:" + e.Topic);
//		//		string[] para = Encoding.Default.GetString(e.Message).Split(',');
//		//		GeoComResult result = null;
//		//		try
//		//		{
//		//			if (e.Topic == "Commond@turnOn")
//		//			{
//		//				try
//		//				{
//		//					result = new GeoComResult();
//		//					MosConfig mosConfig = MosConfigDao.getInstance().getConfig();
//		//					RTUSensor.getInstance().ConnectSerial(mosConfig.RtuSerialPort, mosConfig.RtuBaudRate, mosConfig.RtuDataBits, StringHelper.strToStopBits(mosConfig.RtuStopBits.ToString()));

//		//					if (RTUSensor.getInstance().openJD1())
//		//					{
//		//						result.RC = RETURN_CODES.RC_OK;
//		//					}
//		//					else
//		//					{
//		//						RTUSensor.getInstance().openJD1();
//		//						result.RC = RETURN_CODES.RC_OK;
//		//					}
//		//					Thread.Sleep(1000);
//		//					RTUSensor.getInstance().DisconnectSerial();
//		//					LeicaSensor.Instacne.COM_SwitchOnTPS(COM_TPS_STARTUP_MODE.COM_TPS_REMOTE);

//		//				}
//		//				catch (Exception ex)
//		//				{
//		//					log.Error(ex.Message);
//		//					result.RC = RETURN_CODES.RC_COM_SYSTEM_ERR;
//		//				}
//		//			}
//		//			if (e.Topic == "Commond@turnOff")
//		//			{
//		//				try
//		//				{
//		//					result = new GeoComResult();
//		//					MosConfig mosConfig = MosConfigDao.getInstance().getConfig();
//		//					RTUSensor.getInstance().ConnectSerial(mosConfig.RtuSerialPort, mosConfig.RtuBaudRate, mosConfig.RtuDataBits, StringHelper.strToStopBits(mosConfig.RtuStopBits.ToString()));

//		//					if (RTUSensor.getInstance().closeJD1())
//		//					{
//		//						result.RC = RETURN_CODES.RC_OK;
//		//					}
//		//					else
//		//					{
//		//						RTUSensor.getInstance().closeJD1();
//		//						result.RC = RETURN_CODES.RC_OK;
//		//					}
//		//					//Thread.Sleep(1000);
//		//					RTUSensor.getInstance().DisconnectSerial();
//		//				}
//		//				catch (Exception ex)
//		//				{
//		//					log.Error(ex.Message);
//		//					result.RC = RETURN_CODES.RC_COM_SYSTEM_ERR;
//		//				}
//		//			}
//		//			if (e.Topic == "Commond@sdudyMode")
//		//			{
//		//				AutoSurveyService.getInstance().StopService();
//		//				LeicaSensor.Instacne.setStudyMode();
//		//				AutoSurveyService.getInstance().StartService();
//		//				result = new GeoComResult();
//		//				result.RC = RETURN_CODES.RC_OK;
//		//			}
//		//			if (e.Topic == "Commond@surveyMode")
//		//			{
//		//				LeicaSensor.Instacne.setSurveryMode();
//		//				LeicaSurveyProcess.getInstance().clearCache();
//		//				result = new GeoComResult();
//		//				result.RC = RETURN_CODES.RC_OK;
//		//			}
//		//			if (e.Topic == "Commond@start")
//		//			{
//		//				LeicaSensor.Instacne.setSurveryMode();
//		//				result = new GeoComResult();
//		//				AutoSurveyService.getInstance().StopService();
//		//				AutoSurveyService.getInstance().StartService();
//		//				result.RC = RETURN_CODES.RC_OK;
//		//			}
//		//			if (e.Topic == "Commond@openLaser")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					result = new GeoComResult();
//		//					bool s = TopconSensor.Instacne.lightOpen();
//		//					if (s)
//		//					{
//		//						result.RC = RETURN_CODES.RC_OK;
//		//					}
//		//					else
//		//					{
//		//						result.RC = RETURN_CODES.RC_COM_SYSTEM_ERR;
//		//					}
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");
//		//					result = LeicaSensor.Instacne.EDM_Laserpointer(ON_OFF_TYPE.ON);
//		//				}
//		//			}
//		//			if (e.Topic == "Commond@changeFace")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					result = new GeoComResult();
//		//					bool s = TopconSensor.Instacne.lightOpen();
//		//					if (s)
//		//					{
//		//						result.RC = RETURN_CODES.RC_OK;
//		//					}
//		//					else
//		//					{
//		//						result.RC = RETURN_CODES.RC_COM_SYSTEM_ERR;
//		//					}
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");
//		//					result = LeicaSensor.Instacne.AUT_ChangeFace();
//		//				}
//		//			}
//		//			if (e.Topic == "Commond@closeLaser")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					result = new GeoComResult();
//		//					bool s = TopconSensor.Instacne.lightClose();
//		//					if (s)
//		//					{
//		//						result.RC = RETURN_CODES.RC_OK;
//		//					}
//		//					else
//		//					{
//		//						result.RC = RETURN_CODES.RC_COM_SYSTEM_ERR;
//		//					}
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");
//		//					result = LeicaSensor.Instacne.EDM_Laserpointer(ON_OFF_TYPE.OFF);
//		//				}
//		//			}

//		//			if (e.Topic == "Commond@start")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					TopconSensor.Instacne.setSurveryMode();
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");
//		//					LeicaSensor.Instacne.setSurveryMode();
//		//				}
//		//				result = new GeoComResult();
//		//				AutoSurveyService.getInstance().StopService();
//		//				AutoSurveyService.getInstance().StartService();
//		//				result.RC = RETURN_CODES.RC_OK;
//		//			}
//		//			if (e.Topic == "Commond@setStudyMode")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					result = new GeoComResult();
//		//					TopconSensor.Instacne.setStudyMode();
//		//					result.RC = RETURN_CODES.RC_OK;
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");
//		//					result = new GeoComResult();
//		//					LeicaSensor.Instacne.setStudyMode();
//		//					result.RC = RETURN_CODES.RC_OK;
//		//				}
//		//			}
//		//			if (e.Topic == "Commond@setSurveyMode")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					result = new GeoComResult();
//		//					TopconSensor.Instacne.setSurveryMode();
//		//					result.RC = RETURN_CODES.RC_OK;
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");
//		//					result = new GeoComResult();
//		//					LeicaSensor.Instacne.setSurveryMode();
//		//					result.RC = RETURN_CODES.RC_OK;
//		//				}
//		//			}

//		//			if (e.Topic == "Commond@study")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					result = new GeoComResult();
//		//					int reflectorType = int.Parse(para[1]);
//		//					int prismType = int.Parse(para[2]);
//		//					double prismCon = double.Parse(para[3]);
//		//					double dhz = 0, v = 0, dis = 0, e2 = 0, n = 0, u = 0;
//		//					double xTilt = 0, yTilt = 0;
//		//					bool succ = TopconSensor.Instacne.AutoSurvey((Topcon.EnumSokkiaReflectorType)prismType, prismCon, ref dhz, ref v, ref dis, ref e2, ref n, ref u, true, ref xTilt, ref yTilt);///自动测量
//		//					if (succ)
//		//					{
//		//						SdudyResult sdudyResult = new SdudyResult();
//		//						sdudyResult.Hz = dhz / Math.PI * 180;
//		//						sdudyResult.V = v / Math.PI * 180;
//		//						sdudyResult.D = dis;
//		//						sdudyResult.E = e2;
//		//						sdudyResult.N = n;
//		//						sdudyResult.H = u;
//		//						result.RawResult = JsonConvert.SerializeObject(sdudyResult);
//		//					}
//		//					else
//		//					{
//		//						result.RC = RETURN_CODES.RC_UNDEFINED;
//		//						result.Message = "命令执行失败！";
//		//						log.Info("TYPE_GEOMCOM_SDUDY命令执行失败！");
//		//					}
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");

//		//					if (!LeicaSensor.StudyMode)
//		//					{
//		//						//AutoSurveyService.getInstance().StopService();
//		//						LeicaSensor.Instacne.setStudyMode();
//		//						//AutoSurveyService.getInstance().StartService();
//		//					}

//		//					result = new GeoComResult();
//		//					int reflectorType = int.Parse(para[1]);
//		//					int prismType = int.Parse(para[2]);
//		//					double prismCon = double.Parse(para[3]);
//		//					BAP_PRISMTYPE bAP_PRISMTYPE = (BAP_PRISMTYPE)prismType;
//		//					if (prismType >= 9)
//		//					{
//		//						bAP_PRISMTYPE = BAP_PRISMTYPE.BAP_PRISM_USER;
//		//					}
//		//					double dhz = 0, v = 0, dis = 0;
//		//					TMC_COORDINATE tMC_COORDINATE = new TMC_COORDINATE();
//		//					result = LeicaSensor.Instacne.AutoStudy(bAP_PRISMTYPE, prismCon / 10 / 100, ref dhz, ref v, ref dis, 0.001, ref tMC_COORDINATE);///自动测量
//		//					if (result != null && result.RC == RETURN_CODES.RC_OK)
//		//					{
//		//						SdudyResult sdudyResult = new SdudyResult();
//		//						sdudyResult.Hz = dhz / Math.PI * 180;
//		//						sdudyResult.V = v / Math.PI * 180;
//		//						sdudyResult.D = dis;
//		//						sdudyResult.E = tMC_COORDINATE.E;
//		//						sdudyResult.N = tMC_COORDINATE.N;
//		//						sdudyResult.H = tMC_COORDINATE.H;
//		//						result.RawResult = JsonConvert.SerializeObject(sdudyResult);
//		//					}
//		//				}
//		//			}

//		//			if (e.Topic == "Commond@tilt")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					result = new GeoComResult();
//		//					result.RC = RETURN_CODES.RC_OK;
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");
//		//					double xTilt = 0, yTilt = 0;
//		//					result = LeicaSensor.Instacne.TMC_GetTilt(ref xTilt, ref yTilt);///自动测量
//		//					if (result != null && result.RC == RETURN_CODES.RC_OK)
//		//					{
//		//						SdudyResult sdudyResult = new SdudyResult();
//		//						sdudyResult.Hz = xTilt / Math.PI * 180;
//		//						sdudyResult.V = yTilt / Math.PI * 180;
//		//						result.RawResult = JsonConvert.SerializeObject(sdudyResult);
//		//					}
//		//				}
//		//			}

//		//			if (e.Topic == "Commond@rotate")
//		//			{
//		//				Device device = DeviceDao.getInstance().getCurrentDevice();
//		//				double h = double.Parse(para[1]) / 180 * Math.PI;
//		//				double v = double.Parse(para[2]) / 180 * Math.PI;
//		//				if (device.tpsBrand.Contains("topcon"))
//		//				{
//		//					log.Debug("拓普康收到指令！");
//		//					TopconSensor.Instacne.rotateToAngle(h, v);
//		//					result = new GeoComResult();
//		//					result.RC = RETURN_CODES.RC_OK;
//		//				}
//		//				else
//		//				{
//		//					log.Debug("徕卡收到指令！");
//		//					result = LeicaSensor.Instacne.AUT_MakePositioning(h, v, AUT_POSMODE.AUT_NORMAL, AUT_ATRMODE.AUT_POSITION);
//		//				}
//		//			}

//		//			if (result == null)
//		//			{
//		//				result = new GeoComResult();
//		//				result.RC = RETURN_CODES.RC_COM_SYSTEM_ERR;
//		//			}

//		//			result.Key = int.Parse(para[0]);
//		//			IsoDateTimeConverter dtConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
//		//			string json = JsonConvert.SerializeObject(result, dtConverter);
//		//			byte[] data = System.Text.Encoding.Default.GetBytes(json);
//		//			log.Debug(e.Topic.Replace("Commond", "Response"));
//		//			_MqttClient.Publish(e.Topic.Replace("Commond", "Response"), data, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

//		//		}
//		//		catch (Exception ex)
//		//		{
//		//			log.Error(ex.Message);
//		//		}

//		//	}
//		//}

//		// 发起一次连接，连接成功则订阅相关主题 
//		private void _Connect()
//		{
//			if (String.IsNullOrEmpty(_MqttUsername))
//			{
//				var optionsBuilder = new MqttClientOptionsBuilder()
//			.WithTcpServer(serverIp, serverPort) // 要访问的mqtt服务端的 ip 和 端口号
//			.WithClientId(_MqttClientId) // 设置客户端id
//			.WithCleanSession()
//			.WithTls(new MqttClientOptionsBuilderTlsParameters
//			{
//				UseTls = false  // 是否使用 tls加密
//			});

//				var clientOptions = optionsBuilder.Build();
//				_MqttClient.ConnectAsync(clientOptions);
//			}
//			else
//			{
//				var optionsBuilder = new MqttClientOptionsBuilder()
//		.WithTcpServer(serverIp, serverPort) // 要访问的mqtt服务端的 ip 和 端口号
//		.WithCredentials(_MqttUsername, _MqttUserpass) // 要访问的mqtt服务端的用户名和密码
//		.WithClientId(_MqttClientId) // 设置客户端id
//		.WithCleanSession()
//		.WithTls(new MqttClientOptionsBuilderTlsParameters
//		{
//			UseTls = false  // 是否使用 tls加密
//		});

//				var clientOptions = optionsBuilder.Build();
//				_MqttClient.ConnectAsync(clientOptions);
//			}
//		}

//		//if (_MqttClient.IsConnected)
//		//{
//		//	_MqttClient.Subscribe(new string[] {
//		//		"Commond@turnOn",
//		//		"Commond@turnOff",
//		//		"Commond@openLaser",
//		//		"Commond@changeFace"
//		//		, "Commond@closeLaser"
//		//		, "Commond@search"
//		//		, "Commond@study"
//		//		, "Commond@study2"
//		//		, "Commond@rotate"
//		//		, "Commond@start"
//		//		, "Commond@tilt"
//		//		, "Commond@setStudyMode"
//		//		, "Commond@setSurveyMode"},
//		//	  new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE ,
//		//		  MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE});
//		//	log.Debug("MqttClient Connected！");
//		//}
//		//}
//	}
//}
