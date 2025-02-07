using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;
using System.Text;

namespace NtripShare.GNSS.Device.Sensor
{
	public class Hemisphere : ISensor
	{

		static ILog log = LogManager.GetLogger("NtripShare", typeof(UniCore));
		private readonly object m_lockObject = new object();
		private string m_message = "";
		public void dealConfigMessage(byte[] bytes)
		{
			string message = System.Text.Encoding.ASCII.GetString(bytes);
			List<string> lines = new List<string>();
			lock (m_lockObject)
			{
				m_message += message;
				var lineEnd = m_message.IndexOf("\n", StringComparison.Ordinal);
				while (lineEnd > -1)
				{
					string line = m_message.Substring(0, lineEnd).Trim();
					m_message = m_message.Substring(lineEnd + 1);
					if (!string.IsNullOrEmpty(line))
					{
						lines.Add(line);
					}
					lineEnd = m_message.IndexOf("\n", StringComparison.Ordinal);
				}
			}
			foreach (var line in lines)
			{
				if (line.StartsWith("$G") || line.StartsWith("$G", StringComparison.Ordinal) || line.StartsWith("$B", StringComparison.Ordinal))
				{
					ConfigService.getInstance().dealNmeaData(line);
				}
				else
				{
					if (ConfigService.getInstance().SensorVersion == null)
					{
						log.Debug("ConfigService Data SensorVersion -- " + message);
						ResultType resultType = getResultType(line);
						if (resultType == ResultType.VERSION)
						{
							ConfigService.getInstance().SensorVersion = parseVersion(line);
						}
					}
				}
			}
		}

		/// <summary>
		/// 获取卫星跟踪配置指令
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		public List<byte[]> getTrackCommond(TrackConfig trackConfig)
		{
			List<byte[]> commond = new List<byte[]>();
			commond.Add(Encoding.Default.GetBytes($"$JMASK,{trackConfig.SatelliteElevation}" + "\r\n"));
			if (trackConfig.UseGPS)
			{
				commond.Add(Encoding.Default.GetBytes("$JMODE,GPSOFF,NO" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("$JMODE,GPSOFF,YES" + "\r\n"));
			}
			if (trackConfig.UseBDS)
			{
				commond.Add(Encoding.Default.GetBytes("$JMODE,BDSOFF,NO" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("$JMODE,BDSOFF,YES" + "\r\n"));
			}
			if (trackConfig.UseGLO)
			{
				commond.Add(Encoding.Default.GetBytes("$JMODE,GLOOFF,NO" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("$JMODE,GLOOFF,YES" + "\r\n"));
			}
			if (trackConfig.UseGAL)
			{
				commond.Add(Encoding.Default.GetBytes("$JMODE,GALOFF,NO" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("$JMODE,GALOFF,YES" + "\r\n"));
			}


			commond.Add(Encoding.Default.GetBytes("$JSAVE" + "\r\n"));
			return commond;
		}

		/// <summary>
		/// 获取工作模式指令
		/// </summary>
		/// <param name="workModeConfig"></param>
		/// <returns></returns>
		public List<byte[]> getWorkModeCommond(WorkModeConfig workModeConfig)
		{
			List<byte[]> commond = new List<byte[]>();

			if (workModeConfig.WorkMode == (int)WorkMode.ROVER)
			{
				commond.Add(Encoding.Default.GetBytes($"$JMODE,BASE,NO\r\n"));
			}
			if (workModeConfig.WorkMode == (int)WorkMode.BASE)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTK,28,{workModeConfig.BaseID}\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JMODE,BASE,YES\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JMODE,FIXLOC,YES\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JRTK,1,{workModeConfig.BaseLat * workModeConfig.BaseLatType},{workModeConfig.BaseLon * workModeConfig.BaseLonType},{workModeConfig.BaseHeight}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,RTCM3,1,PORTB\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,MSM4\r\n"));
			}
			if (workModeConfig.WorkMode == (int)WorkMode.AUTOBASE)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTK,28,{workModeConfig.BaseID}\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JMODE,BASE,YES\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JMODE,FIXLOC,NO\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JRTK,1,P" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,RTCM3,1,PORTB\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,MSM4\r\n"));
			}
			commond.Add(Encoding.Default.GetBytes("$JSAVE" + "\r\n"));
			return commond;
		}

		/// <summary>
		/// 串口输出数据配置
		/// </summary>
		/// <param name="comConfig"></param>
		/// <returns></returns>
		public List<byte[]> getComOutCommond(ComConfig comConfig, OutConfig outConfig)
		{
			List<byte[]> commond = new List<byte[]>();

			if (comConfig.SerialStream == "nmea")
			{
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPGGA,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GNGSA,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPGSA,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPGST,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPGSV,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GLGSV,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GBGSV,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GQGSV,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GAGSV,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPHDT,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GNRMC,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPVTG,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPGLL,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPZDA,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"$JASC,GPTRA,{1.0 / outConfig.NMEA0183Interval},{comConfig.SerialName}" + "\r\n"));
			}
			commond.Add(Encoding.Default.GetBytes($"$JASC,RTCM3,1,{comConfig.SerialName}" + "\r\n"));
			if (comConfig.SerialStream == "rtcm3")
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,MSM4,{comConfig.SerialName}" + "\r\n"));
				if (outConfig.RTCM1005)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1005,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1006)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1006,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1033)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1033,{comConfig.SerialName}" + "\r\n"));
				}

				if (outConfig.RTCM1074)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1074,{comConfig.SerialName}" + "\r\n"));
				}

				if (outConfig.RTCM1084)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1084,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1094)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1094,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1114)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1114,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1124)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1124,{comConfig.SerialName}" + "\r\n"));
				}

				if (outConfig.RTCM1019)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1019,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1020)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1020,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1042)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1042,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1044)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1044,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1045)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1045,{comConfig.SerialName}" + "\r\n"));
				}
				if (outConfig.RTCM1046)
				{
					commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1046,{comConfig.SerialName}" + "\r\n"));
				}
			}
			commond.Add(Encoding.Default.GetBytes("$JSAVE" + "\r\n"));
			return commond;
		}

		/// <summary>
		/// 获取版本信息指令
		/// </summary>
		/// <returns></returns>
		public List<byte[]> getVersionCommond(SysConfig sysConfig)
		{
			List<byte[]> commond = new List<byte[]>();
			commond.Add(Encoding.Default.GetBytes("$JI\r\n"));
			return commond;
		}

		/// <summary>
		/// 获取天线模式命令
		/// </summary>
		/// <param name="antennaConfig"></param>
		/// <returns></returns>
		public List<byte[]> getAntennaCommond(AntennaConfig antennaConfig)
		{
			List<byte[]> commond = new List<byte[]>();
			//commond.Add(Encoding.Default.GetBytes($"$ANTENNADELTAHEN {antennaConfig.Height} 0 0\r\n"));
			//commond.Add(Encoding.Default.GetBytes("$JSAVE" + "\r\n"));
			return commond;
		}

		/// <summary>
		/// 获取数据输出命令
		/// </summary>
		/// <param name="sysConfig"></param>
		/// <returns></returns>
		public List<byte[]> getOutCommond(SysConfig sysConfig)
		{
			List<byte[]> commond = new List<byte[]>();
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPGGA,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GNGSA,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPGSA,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPGST,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPGSV,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GLGSV,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GBGSV,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GQGSV,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GAGSV,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPHDT,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GNRMC,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPVTG,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPGLL,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPZDA,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,GPTRA,{1.0 / sysConfig.OutConfig.NMEA0183Interval}\r\n"));

			commond.Add(Encoding.Default.GetBytes($"$JBAUD,{sysConfig.DataBaudRate},{sysConfig.DataCom}" + "\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JASC,RTCM3,1,{sysConfig.DataCom}" + "\r\n"));
			commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,MSM4" + "\r\n"));
			if (sysConfig.OutConfig.RTCM1005)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1005\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1006)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1006\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1033)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1033\r\n"));
			}

			if (sysConfig.OutConfig.RTCM1074)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1074\r\n"));
			}

			if (sysConfig.OutConfig.RTCM1084)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1084\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1094)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1094\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1114)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1114\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1124)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1124\r\n"));
			}

			if (sysConfig.OutConfig.RTCM1019)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1019\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1020)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1020\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1042)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1042\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1044)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1044\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1045)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1045\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1046)
			{
				commond.Add(Encoding.Default.GetBytes($"$JRTCM3,INCLUDE,1046\r\n"));
			}
			commond.Add(Encoding.Default.GetBytes("$JSAVE" + "\r\n"));
			return commond;
		}

		/// <summary>
		/// 获取初始化板卡指令
		/// </summary>
		/// <param name="sysConfig"></param>
		/// <returns></returns>
		public List<byte[]> getInitCommond(SysConfig sysConfig)
		{

			List<byte[]> commond = new List<byte[]>();
			commond.AddRange(getOutCommond(sysConfig));
			commond.AddRange(getAntennaCommond(sysConfig.AntennaConfig));
			commond.AddRange(getTrackCommond(sysConfig.TrackConfig));
			commond.AddRange(getWorkModeCommond(sysConfig.WorkModeConfig));

			return commond;
		}


		/// <summary>
		/// 解析版本信息
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public SensorVersion parseVersion(string str)
		{
			try
			{
				string[] datas = str.Split(",");
				if (datas.Length >= 6)
				{
					SensorVersion sensorVersion = new SensorVersion();
					sensorVersion.Type = datas[2];
					sensorVersion.SwVersion = datas[7];
					sensorVersion.Model = datas[3];
					sensorVersion.Pn = datas[1];
					sensorVersion.Sn = datas[1];
					sensorVersion.EfuseID = datas[1];
					sensorVersion.CompTime = datas[4];
					return sensorVersion;
				}
				else
				{
					return null;
				}
			}
			catch (Exception e)
			{

			}
			return null;
		}

		public ResultType getResultType(string str)
		{
			if (str.ToUpper().StartsWith("$>JI"))
			{
				return ResultType.VERSION;
			}
			return ResultType.UNKNOW;
		}
	}
}
