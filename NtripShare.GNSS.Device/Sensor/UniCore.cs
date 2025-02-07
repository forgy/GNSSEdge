using GeoAPI.DataStructures;
using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;
using System.Text;

namespace NtripShare.GNSS.Device.Sensor
{
	public class UniCore : ISensor
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
			if (trackConfig.UseGPS)
			{
				commond.Add(Encoding.Default.GetBytes("UNMASK GPS" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"MASK {trackConfig.SatelliteElevation} GPS" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("MASK GPS" + "\r\n"));
			}
			if (trackConfig.UseBDS)
			{
				commond.Add(Encoding.Default.GetBytes("UNMASK BDS" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"MASK {trackConfig.SatelliteElevation} BDS" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("MASK BDS" + "\r\n"));
			}
			if (trackConfig.UseGLO)
			{
				commond.Add(Encoding.Default.GetBytes("UNMASK GLO" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"MASK {trackConfig.SatelliteElevation} GLO" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("MASK GLO" + "\r\n"));
			}
			if (trackConfig.UseGAL)
			{
				commond.Add(Encoding.Default.GetBytes("UNMASK GAL" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"MASK {trackConfig.SatelliteElevation} GAL" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("MASK GAL" + "\r\n"));
			}
			if (trackConfig.UseQZSS)
			{
				commond.Add(Encoding.Default.GetBytes("UNMASK QZSS" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"MASK {trackConfig.SatelliteElevation} QZSS" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("MASK QZSS" + "\r\n"));
			}

			if (trackConfig.Smooth)
			{
				commond.Add(Encoding.Default.GetBytes("CONFIG SMOOTH RTKHEIGHT 10" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes("CONFIG SMOOTH HEADING 10" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("CONFIG SMOOTH RTKHEIGHT 0" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes("CONFIG SMOOTH HEADING 0" + "\r\n"));
			}

			if (trackConfig.B2BPPP)
			{
				commond.Add(Encoding.Default.GetBytes($"CONFIG PPP ENABLE B2b-PPP" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"CONFIG PPP CONVERGE 10 20" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes($"CONFIG PPP DISABLE" + "\r\n"));
			}

			commond.Add(Encoding.Default.GetBytes("SAVECONFIG" + "\r\n"));
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
				commond.Add(Encoding.Default.GetBytes("MODE ROVER" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes("CONFIG RTK TIMEOUT 60" + "\r\n"));
			}
			if (workModeConfig.WorkMode == (int)WorkMode.QUICKBASE)
			{
				commond.Add(Encoding.Default.GetBytes("MODE ROVER" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes("CONFIG RTK TIMEOUT 60" + "\r\n"));
			}
			if (workModeConfig.WorkMode == (int)WorkMode.BASE)
			{
				commond.Add(Encoding.Default.GetBytes($"CONFIG UNDULATION 0.000" + "\r\n"));
                commond.Add(Encoding.Default.GetBytes($"MODE BASE {workModeConfig.BaseID} {workModeConfig.BaseLat * workModeConfig.BaseLatType} {workModeConfig.BaseLon * workModeConfig.BaseLonType} {workModeConfig.BaseHeight}" + "\r\n"));
			}
			if (workModeConfig.WorkMode == (int)WorkMode.AUTOBASE)
			{
				commond.Add(Encoding.Default.GetBytes($"MODE BASE {workModeConfig.BaseID} TIME 60 1.5 2.5 0.5" + "\r\n"));
			}
			
			commond.Add(Encoding.Default.GetBytes("SAVECONFIG" + "\r\n"));
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

			commond.Add(Encoding.Default.GetBytes($"UNLOG {comConfig.SerialName}" + "\r\n"));
			if (comConfig.SerialStream == "nmea")
			{
				commond.Add(Encoding.Default.GetBytes($"CONFIG NMEAVERSION V41\r\n"));
				float interval = outConfig.NMEA0183Interval;
				if (interval < 1) { 
					interval = 1;
				}
				if (outConfig.GPGGA)
				{
					commond.Add(Encoding.Default.GetBytes($"GNGGA {comConfig.SerialName} {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPGSA)
				{
					commond.Add(Encoding.Default.GetBytes($"GPGSA {comConfig.SerialName} {interval}\r\n"));
				}
				if (outConfig.GPGST)
				{
					commond.Add(Encoding.Default.GetBytes($"GPGST {comConfig.SerialName} {interval}\r\n"));
				}
				if (outConfig.GPGSV)
				{
					commond.Add(Encoding.Default.GetBytes($"GPGSV {comConfig.SerialName} {interval}\r\n"));
				}
				if (outConfig.GPHDT)
				{
					commond.Add(Encoding.Default.GetBytes($"GPHDT {comConfig.SerialName} {interval}\r\n"));
				}
				if (outConfig.GPRMC)
				{
					commond.Add(Encoding.Default.GetBytes($"GNRMC {comConfig.SerialName} {interval}\r\n"));
				}
				if (outConfig.GPVTG)
				{
					commond.Add(Encoding.Default.GetBytes($"GPVTG {comConfig.SerialName} {interval}\r\n"));
				}
				if (outConfig.GPGLL)
				{
					commond.Add(Encoding.Default.GetBytes($"GPGLL {comConfig.SerialName} {interval}\r\n"));
				}
				if (outConfig.GPZDA)
				{
					commond.Add(Encoding.Default.GetBytes($"GPZDA {comConfig.SerialName} {interval}\r\n"));
				}
			}
			if (comConfig.SerialStream == "rtcm3")
			{
				if (outConfig.RTCM1005)
				{
					commond.Add(Encoding.Default.GetBytes($"rtcm1005 {comConfig.SerialName} {outConfig.BaseCoordInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1006)
				{
					commond.Add(Encoding.Default.GetBytes($"rtcm1006 {comConfig.SerialName} {outConfig.BaseCoordInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1033)
				{
					commond.Add(Encoding.Default.GetBytes($"rtcm1033 {comConfig.SerialName} {outConfig.BaseCoordInterval}" + "\r\n"));
				}

				if (outConfig.RTCM1074)
				{
					commond.Add(Encoding.Default.GetBytes($"rtcm1074 {comConfig.SerialName} {outConfig.OBSInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1084)
				{
					commond.Add(Encoding.Default.GetBytes($"rtcm1084 {comConfig.SerialName} {outConfig.OBSInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1094)
				{
					commond.Add(Encoding.Default.GetBytes($"rtcm1094 {comConfig.SerialName} {outConfig.OBSInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1114)
				{
					commond.Add(Encoding.Default.GetBytes($"RTCM1114 {comConfig.SerialName} {outConfig.OBSInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1124)
				{
					commond.Add(Encoding.Default.GetBytes($"rtcm1124 {comConfig.SerialName} {outConfig.OBSInterval}" + "\r\n"));
				}

				if (outConfig.RTCM1019)
				{
					commond.Add(Encoding.Default.GetBytes($"RTCM1019 {comConfig.SerialName} {outConfig.NavInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1020)
				{
					commond.Add(Encoding.Default.GetBytes($"RTCM1020 {comConfig.SerialName} {outConfig.NavInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1042)
				{
					commond.Add(Encoding.Default.GetBytes($"RTCM1042 {comConfig.SerialName} {outConfig.NavInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1044)
				{
					commond.Add(Encoding.Default.GetBytes($"RTCM1044 {comConfig.SerialName} {outConfig.NavInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1045)
				{
					commond.Add(Encoding.Default.GetBytes($"RTCM1045 {comConfig.SerialName} {outConfig.NavInterval}" + "\r\n"));
				}
				if (outConfig.RTCM1046)
				{
					commond.Add(Encoding.Default.GetBytes($"RTCM1046 {comConfig.SerialName} {outConfig.NavInterval}" + "\r\n"));
				}
			}
			commond.Add(Encoding.Default.GetBytes("SAVECONFIG" + "\r\n"));
			return commond;
		}

		/// <summary>
		/// 获取版本信息指令
		/// </summary>
		/// <returns></returns>
		public List<byte[]> getVersionCommond(SysConfig sysConfig)
		{
			List<byte[]> commond = new List<byte[]>();
			commond.Add(Encoding.Default.GetBytes($"UNLOG {sysConfig.ConfigCom}\r\n"));
			commond.Add(Encoding.Default.GetBytes("SAVECONFIG" + "\r\n"));
			commond.Add(Encoding.Default.GetBytes("VERSIONA\r\n"));
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
			commond.Add(Encoding.Default.GetBytes($"ANTENNADELTAHEN {antennaConfig.Height} 0 0\r\n"));
			commond.Add(Encoding.Default.GetBytes("SAVECONFIG" + "\r\n"));
			return commond;
		}

		/// <summary>
		/// 获取数据输出命令
		/// </summary>
		/// <param name="sysConfig"></param>
		/// <returns></returns>
		public List<byte[]> getOutCommond(SysConfig sysConfig) {
			List<byte[]> commond = new List<byte[]>();
			commond.Add(Encoding.Default.GetBytes($"UNLOG {sysConfig.ConfigCom}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"CONFIG NMEAVERSION V41\r\n"));
			float interval = sysConfig.OutConfig.NMEA0183Interval;
			if (interval < 1)
			{
				interval = 1;
			}
			if (sysConfig.OutConfig.GPGGA)
			{
				commond.Add(Encoding.Default.GetBytes($"GNGGA {sysConfig.OutConfig.NMEA0183Interval}\r\n"));
			}
			if (sysConfig.OutConfig.GPGSA)
			{
				commond.Add(Encoding.Default.GetBytes($"GPGSA {interval}\r\n"));
			}
			if (sysConfig.OutConfig.GPGST)
			{
				commond.Add(Encoding.Default.GetBytes($"GPGST {interval}\r\n"));
			}
			if (sysConfig.OutConfig.GPGSV)
			{
				commond.Add(Encoding.Default.GetBytes($"GPGSV {interval}\r\n"));
			}
			if (sysConfig.OutConfig.GPHDT)
			{
				commond.Add(Encoding.Default.GetBytes($"GPHDT {interval}\r\n"));
			}
			if (sysConfig.OutConfig.GPRMC)
			{
				commond.Add(Encoding.Default.GetBytes($"GNRMC {interval}\r\n"));
			}
			if (sysConfig.OutConfig.GPVTG)
			{
				commond.Add(Encoding.Default.GetBytes($"GPVTG {interval}\r\n"));
			}
			if (sysConfig.OutConfig.GPGLL)
			{
				commond.Add(Encoding.Default.GetBytes($"GPGLL {interval}\r\n"));
			}
			if (sysConfig.OutConfig.GPZDA)
			{
				commond.Add(Encoding.Default.GetBytes($"GPZDA {interval}\r\n"));
			}

			commond.Add(Encoding.Default.GetBytes($"UNLOG {sysConfig.DataCom}\r\n"));
			if (sysConfig.OutConfig.RTCM1005)
			{
				commond.Add(Encoding.Default.GetBytes($"rtcm1005 {sysConfig.DataCom} {sysConfig.OutConfig.BaseCoordInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1006)
			{
				commond.Add(Encoding.Default.GetBytes($"rtcm1006 {sysConfig.DataCom} {sysConfig.OutConfig.BaseCoordInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1033)
			{
				commond.Add(Encoding.Default.GetBytes($"rtcm1033 {sysConfig.DataCom} {sysConfig.OutConfig.BaseCoordInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1074)
			{
				commond.Add(Encoding.Default.GetBytes($"rtcm1074 {sysConfig.DataCom} {sysConfig.OutConfig.OBSInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1084)
			{
				commond.Add(Encoding.Default.GetBytes($"rtcm1084 {sysConfig.DataCom} {sysConfig.OutConfig.OBSInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1094)
			{
				commond.Add(Encoding.Default.GetBytes($"rtcm1094 {sysConfig.DataCom} {sysConfig.OutConfig.OBSInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1114)
			{
				commond.Add(Encoding.Default.GetBytes($"rtcm1114 {sysConfig.DataCom} {sysConfig.OutConfig.OBSInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1124)
			{
				commond.Add(Encoding.Default.GetBytes($"rtcm1124 {sysConfig.DataCom} {sysConfig.OutConfig.OBSInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1019)
			{
				commond.Add(Encoding.Default.GetBytes($"RTCM1019 {sysConfig.DataCom} {sysConfig.OutConfig.NavInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1020)
			{
				commond.Add(Encoding.Default.GetBytes($"RTCM1020 {sysConfig.DataCom} {sysConfig.OutConfig.NavInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1042)
			{
				commond.Add(Encoding.Default.GetBytes($"RTCM1042 {sysConfig.DataCom} {sysConfig.OutConfig.NavInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1044)
			{
				commond.Add(Encoding.Default.GetBytes($"RTCM1044 {sysConfig.DataCom} {sysConfig.OutConfig.NavInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1045)
			{
				commond.Add(Encoding.Default.GetBytes($"RTCM1045 {sysConfig.DataCom} {sysConfig.OutConfig.NavInterval}" + "\r\n"));
			}
			if (sysConfig.OutConfig.RTCM1046)
			{
				commond.Add(Encoding.Default.GetBytes($"RTCM1046 {sysConfig.DataCom} {sysConfig.OutConfig.NavInterval}" + "\r\n"));
			}

			commond.Add(Encoding.Default.GetBytes("SAVECONFIG" + "\r\n"));
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
				string data = str.Split(";")[1].Replace("\"", "");
				string[] datas = data.Split(",");
				if (datas.Length >= 6)
				{
					SensorVersion sensorVersion = new SensorVersion();
					sensorVersion.Type = datas[0];
					sensorVersion.SwVersion = datas[1];
					sensorVersion.Model = datas[2];
					sensorVersion.Pn = datas[3].Split("-")[0];
					sensorVersion.Sn = datas[3].Split("-")[1];
					sensorVersion.EfuseID = datas[4];
					sensorVersion.CompTime = datas[5].Split("*")[0];
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
			if (str.ToUpper().StartsWith("#VERSIONA"))
			{
				return ResultType.VERSION;
			}
			return ResultType.UNKNOW;
		}
	}
}
