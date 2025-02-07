using log4net;
using Newtonsoft.Json;
using System.Text;

namespace NtripShare.GNSS.Device.Config
{
	/// <summary>
	/// 配置参数
	/// </summary>
	public class SysConfig
	{
		static ILog log = LogManager.GetLogger("NtripShare", typeof(SysConfig));
		private static SysConfig instance = new SysConfig();

		public static SysConfig getInstance()
		{
			return instance;
		}
		/// <summary>
		/// 初始化
		/// </summary>
		public static void initConfig()
		{
			if (File.Exists("appsettings.json"))
			{
				StreamReader streamReader = new StreamReader("appsettings.json", Encoding.Default);
				string jsonRoot = streamReader.ReadToEnd();  //读全部json
				SysConfig sysConfig = JsonConvert.DeserializeObject<SysConfig>(jsonRoot);
				if (sysConfig != null)
				{
					instance = sysConfig;
				}
				streamReader.Close();
			}
			try
			{
				if (!File.Exists("/home/key"))
				{
					FileStream fs1 = new FileStream("/home/key", FileMode.Create, FileAccess.Write);//创建写入文件                //设置文件属性为隐藏
					StreamWriter sw = new StreamWriter(fs1);
					sw.WriteLine("NS" + DateTime.Now.ToString("yyMMddhhmmss"));//开始写入值
					sw.Close();
					fs1.Close();
				}
				if (File.Exists("/home/key"))
				{
					string str1 = File.ReadAllText("/home/key");
					instance.DeviceKey = str1.Replace("\r", "").Replace("\n", "");
					try
					{
						if (File.Exists("frpc.toml"))
						{
							ReplaceValue("frpc.toml", "XXXXXXXX", instance.DeviceKey);
						}
					}
					catch (Exception e)
					{
						log.Error(e);
					}
				}
			}
			catch (Exception e)
			{
				log.Error(e);
			}
		}

		/// <summary>
		/// 替换值
		/// </summary>
		/// <param name="strFilePath">txt等文件的路径</param>
		/// <param name="oldValue">索引的字符串，定位到某一行</param>
		/// <param name="newValue">替换新值</param>
		private static void ReplaceValue(string strFilePath, string oldValue, string newValue)
		{
			if (File.Exists(strFilePath))
			{
				string[] lines = System.IO.File.ReadAllLines(strFilePath);
				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].Contains(oldValue))
					{
						lines[i] = lines[i].Replace(oldValue, newValue);
					}
				}
				File.WriteAllLines(strFilePath, lines);
			}
		}


		/// <summary>
		/// 保存配置文件
		/// </summary>
		public void saveConfig()
		{
			File.WriteAllText("appsettings.json", JsonConvert.SerializeObject(instance));
		}
		/// <summary>
		/// 是否启用UPNP
		/// </summary>
		public bool UPNP { get; set; }
		/// <summary>
		/// 是否启用UPNP
		/// </summary>
		public int UPNPCloseTime { get; set; }

		/// <summary>
		/// 是否启用4G
		/// </summary>
		public bool SIM { get; set; }
		/// <summary>
		/// 是否启用wifi热点
		/// </summary>
		public bool WIFI { get; set; }

		/// <summary>
		/// 是否启用wifi热点
		/// </summary>
		public int WIFICloseTime { get; set; }
		/// <summary>
		/// wifi热点密码
		/// </summary>
		public string WIFIPassword { get; set; }
		/// <summary>
		/// 设备唯一码
		/// </summary>
		public string DeviceKey { get; set; }
		/// <summary>
		/// 注册码
		/// </summary>
		public string SoftKey { get; set; }
		/// <summary>
		/// 注册码
		/// </summary>
		public string HardwareVersion { get; set; }
		/// <summary>
		/// 型号
		/// </summary>
		public string Model { get; set; }
		/// <summary>
		/// 登录用户名
		/// </summary>
		public string UserName { get; set; }
		/// <summary>
		/// 登录密码
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// 板卡品牌
		/// </summary>
		public string SensorBrand { get; set; }

		public bool ConfigUseByte { get; set; } = false;

		/// <summary>
		/// 板卡配置串口号
		/// </summary>
		public string ConfigCom { get; set; } = "";
		/// <summary>
		/// 配置串口号
		/// </summary>
		public string ConfigSerialPort { get; set; } = "";

		/// <summary>
		/// 配置串口波特率
		/// </summary>
		public int ConfigBaudRate { get; set; } = 115200;

		/// <summary>
		/// 板卡配置串口号
		/// </summary>
		public string DataCom { get; set; } = "";
		/// <summary>
		/// 数据串口号
		/// </summary>
		public string DataSerialPort { get; set; } = "";
		/// <summary>
		/// 数据串口波特率
		/// </summary>
		public int DataBaudRate { get; set; } = 115200;

		/// <summary>
		/// 串口设置
		/// </summary>
		public ComConfig ComConfig { get; set; }
		/// <summary>
		/// mqttt设置
		/// </summary>
		public MqttConfig MqttConfig { get; set; }
		/// <summary>
		/// NtripClient设置
		/// </summary>
		public NtripClientConfig NtripClientConfig { get; set; }
		/// <summary>
		/// NtripClient设置
		/// </summary>
		//public NtripClientConfig NtripClientConfig2 { get; set; }
		/// <summary>
		/// NtripClient设置
		/// </summary>
		//public NtripClientConfig NtripClientConfig3 { get; set; }
		/// <summary>
		/// NtripServer设置
		/// </summary>
		public NtripServerConfig NtripServerConfig { get; set; }
		/// <summary>
		/// NtripServer设置
		/// </summary>
		public NtripServerConfig NtripServerConfig2 { get; set; }
		/// <summary>
		/// NtripServer设置
		/// </summary>
		public NtripServerConfig NtripServerConfig3 { get; set; }
		/// <summary>
		/// TCP设置
		/// </summary>
		/// 
		public NtripCasterConfig NtripCasterConfig { get; set; }
		public TcpConfig TcpConfig { get; set; }
		public TcpConfig TcpConfig2 { get; set; }
		public TcpConfig TcpConfig3 { get; set; }
		/// <summary>
		/// 卫星追踪
		/// </summary>
		public TrackConfig TrackConfig { get; set; }
		/// <summary>
		/// 卫星追踪
		/// </summary>
		public WorkModeConfig WorkModeConfig { get; set; }
		/// <summary>
		/// 天线设置
		/// </summary>
		public AntennaConfig AntennaConfig { get; set; }
		/// <summary>
		/// 数据输出设置
		/// </summary>
		public OutConfig OutConfig { get; set; }
	}
}
