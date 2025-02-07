using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;
using System.Text;

namespace NtripShare.GNSS.Device.Sensor
{
	public class NovAtel : ISensor
	{
		static ILog log = LogManager.GetLogger("NtripShare", typeof(UniCore));
		private readonly object m_lockObject = new object();
		private string m_message = "";
		private SysConfig m_sysConfig;
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
			foreach (var lineMM in lines)
			{
				var line = lineMM.Trim();
				if (line.Contains("$G"))
				{
					line = line.Substring(line.IndexOf("$G"));
				}
				if (line.StartsWith("$G") || line.StartsWith("$G", StringComparison.Ordinal) || line.StartsWith("$B", StringComparison.Ordinal))
				{
					ConfigService.getInstance().dealNmeaData(line);
				}
				else
				{
					if (ConfigService.getInstance().SensorVersion == null)
					{
						ResultType resultType = getResultType(line);
						if (resultType == ResultType.VERSION)
						{
							log.Debug("ConfigService Data SensorVersion -- " + message);
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
			commond.Add(Encoding.Default.GetBytes($"ELEVATIONCUTOFF ALL {trackConfig.SatelliteElevation}" + "\r\n"));

			if (trackConfig.UseGPS)
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV GPS 0 ALWAYS" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV GPS 0 NEVER" + "\r\n"));
			}
			if (trackConfig.UseBDS)
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV BeiDou 0 ALWAYS" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV BeiDou 0 NEVER" + "\r\n"));
			}
			if (trackConfig.UseGLO)
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV GLONASS 0 ALWAYS" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV GLONASS 0 NEVER" + "\r\n"));
			}
			if (trackConfig.UseGAL)
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV GALILEO 0 ALWAYS" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV GALILEO 0 NEVER" + "\r\n"));
			}
			if (trackConfig.UseQZSS)
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV QZSS 0 ALWAYS" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes($"TRACKSV QZSS 0 NEVER" + "\r\n"));
			}

			if (trackConfig.Smooth)
			{
				commond.Add(Encoding.Default.GetBytes("PDPFILTER enable" + "\r\n"));
			}
			else
			{
				commond.Add(Encoding.Default.GetBytes("PDPFILTER disable" + "\r\n"));
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
				commond.Add(Encoding.Default.GetBytes($"INTERFACEMODE {m_sysConfig.ConfigCom} RTCMV3 NOVATEL OFF\r\n"));
			}
			if (workModeConfig.WorkMode == (int)WorkMode.BASE)
			{
				commond.Add(Encoding.Default.GetBytes($"FIX POSITION {workModeConfig.BaseLat * workModeConfig.BaseLatType} {workModeConfig.BaseLon * workModeConfig.BaseLonType} {workModeConfig.BaseHeight}" + "\r\n"));
				commond.Add(Encoding.Default.GetBytes($"DGPSTXID RTCMV3 {workModeConfig.BaseID}\r\n"));

			}
			if (workModeConfig.WorkMode == (int)WorkMode.AUTOBASE)
			{
				commond.Add(Encoding.Default.GetBytes($"Fix auto\r\n"));
				commond.Add(Encoding.Default.GetBytes($"DGPSTXID RTCMV3 {workModeConfig.BaseID}\r\n"));
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

			commond.Add(Encoding.Default.GetBytes($"UNLOGALL {comConfig.SerialName}" + "\r\n"));
			if (comConfig.SerialStream == "nmea")
			{
				commond.Add(Encoding.Default.GetBytes($"NMEATALKER auto\r\n"));
				commond.Add(Encoding.Default.GetBytes($"NMEAVERSION V41\r\n"));
				if (outConfig.GPGGA)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} gpgga ontime {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPGLL)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPGLL ontime {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPGSA)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPGSA ontime {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPGST)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPGST ontime {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPGSV)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPGSV ontime {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPHDT)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPHDT ontime {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPRMC)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPRMC ontime {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPVTG)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPVTG ontime {outConfig.NMEA0183Interval}\r\n"));
				}
				if (outConfig.GPZDA)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPZDA ontime {outConfig.NMEA0183Interval}\r\n"));
				}
			}
			if (comConfig.SerialStream == "rtcm3")
			{
				if (outConfig.RTCM1005)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1005b ontime {outConfig.BaseCoordInterval}\r\n"));
				}
				if (outConfig.RTCM1006)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1006b ontime {outConfig.BaseCoordInterval}\r\n"));
				}
				if (outConfig.RTCM1033)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1033b ontime {outConfig.BaseCoordInterval}\r\n"));

				}
				if (outConfig.RTCM1074)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1074b ontime {outConfig.OBSInterval}\r\n"));
				}
				if (outConfig.RTCM1114)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1014b ontime {outConfig.OBSInterval}\r\n"));
				}
				if (outConfig.RTCM1124)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1024b ontime {outConfig.OBSInterval}\r\n"));
				}
				if (outConfig.RTCM1084)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1084b ontime {outConfig.OBSInterval}\r\n"));
				}
				if (outConfig.RTCM1094)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1094b ontime {outConfig.OBSInterval}\r\n"));
				}

				if (outConfig.RTCM1019)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1019b ontime {outConfig.NavInterval}\r\n"));
				}
				if (outConfig.RTCM1042)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1042b ontime {outConfig.NavInterval}\r\n"));
				}
				if (outConfig.RTCM1020)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1020b ontime {outConfig.NavInterval}\r\n"));
				}
				if (outConfig.RTCM1044)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1044b ontime {outConfig.NavInterval}\r\n"));
				}
				if (outConfig.RTCM1045)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1045b ontime {outConfig.NavInterval}\r\n"));
				}
				if (outConfig.RTCM1046)
				{
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} rtcm1046b ontime {outConfig.NavInterval}\r\n"));
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
			commond.Add(Encoding.Default.GetBytes($"INTERFACEMODE THISPORT NOVATEL NOVATEL\r\n"));
			commond.Add(Encoding.Default.GetBytes($"INTERFACEMODE {sysConfig.ConfigCom} NOVATEL NOVATEL\r\n"));
			//commond.Add(Encoding.Default.GetBytes($"SERIALCONFIG THISPORT 9600 N 8 1 N ON\r\n"));
			//commond.Add(Encoding.Default.GetBytes($"SERIALCONFIG THISPORT {sysConfig.ConfigBaudRate} N 8 1 N ON\r\n"));
			commond.Add(Encoding.Default.GetBytes("log version ONCE\r\n"));
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
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GPSL1 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GPSL2 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GLONASSL1 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GLONASSL2 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GPSL5 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GALILEOE1 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GALILEOE5A {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GALILEOE5B {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO GALILEOALTBOC {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO BEIDOUB1 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO BEIDOUB2 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO QZSSL1 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO QZSSL2 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes($"THISANTENNAPCO QZSSL5 {antennaConfig.North} {antennaConfig.East} {antennaConfig.Height}\r\n"));
			commond.Add(Encoding.Default.GetBytes("SAVECONFIG" + "\r\n"));
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
			commond.Add(Encoding.Default.GetBytes($"UNLOGALL\r\n"));
			commond.Add(Encoding.Default.GetBytes($"NMEATALKER auto\r\n"));
			commond.Add(Encoding.Default.GetBytes($"NMEAVERSION V41\r\n"));
			OutConfig outConfig = sysConfig.OutConfig;
			if (outConfig.GPGGA)
			{
				commond.Add(Encoding.Default.GetBytes($"log gpgga ontime {outConfig.NMEA0183Interval}\r\n"));
			}
			if (outConfig.GPGLL)
			{
				commond.Add(Encoding.Default.GetBytes($"log GPGLL ontime {outConfig.NMEA0183Interval}\r\n"));
			}
			if (outConfig.GPGSA)
			{
				commond.Add(Encoding.Default.GetBytes($"log GPGSA ontime {outConfig.NMEA0183Interval}\r\n"));
			}
			if (outConfig.GPGST)
			{
				commond.Add(Encoding.Default.GetBytes($"log GPGST ontime {outConfig.NMEA0183Interval}\r\n"));
			}
			if (outConfig.GPGSV)
			{
				commond.Add(Encoding.Default.GetBytes($"log GPGSV ontime {outConfig.NMEA0183Interval}\r\n"));
			}
			if (outConfig.GPHDT)
			{
				commond.Add(Encoding.Default.GetBytes($"log GPHDT ontime {outConfig.NMEA0183Interval}\r\n"));
			}
			if (outConfig.GPRMC)
			{
				commond.Add(Encoding.Default.GetBytes($"log GPRMC ontime {outConfig.NMEA0183Interval}\r\n"));
			}
			if (outConfig.GPVTG)
			{
				commond.Add(Encoding.Default.GetBytes($"log GPVTG ontime {outConfig.NMEA0183Interval}\r\n"));
			}
			if (outConfig.GPZDA)
			{
				commond.Add(Encoding.Default.GetBytes($"log GPZDA ontime {outConfig.NMEA0183Interval}\r\n"));
			}


			commond.Add(Encoding.Default.GetBytes($"UNLOGALL {sysConfig.DataCom}\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1005b ontime 10\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1006b ontime 10\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1033b ontime 10\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1074b ontime 1\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1084b ontime 1\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1094b ontime 1\r\n"));
			//commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1124b ontime 1\r\n"));


			//commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1019b ontime 300\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1042b ontime 300\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1020b ontime 300\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1044b ontime 300\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1045b ontime 300\r\n"));
			//         commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1046b ontime 300\r\n"));

			if (outConfig.RTCM1005)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1005b ontime {outConfig.BaseCoordInterval}\r\n"));
			}
			if (outConfig.RTCM1006)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1006b ontime {outConfig.BaseCoordInterval}\r\n"));
			}
			if (outConfig.RTCM1033)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1033b ontime {outConfig.BaseCoordInterval}\r\n"));

			}
			if (outConfig.RTCM1074)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1074b ontime {outConfig.OBSInterval}\r\n"));
			}
			if (outConfig.RTCM1114)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1014b ontime {outConfig.OBSInterval}\r\n"));
			}
			if (outConfig.RTCM1124)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1024b ontime {outConfig.OBSInterval}\r\n"));
			}
			if (outConfig.RTCM1084)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1084b ontime {outConfig.OBSInterval}\r\n"));
			}
			if (outConfig.RTCM1094)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1094b ontime {outConfig.OBSInterval}\r\n"));
			}

			if (outConfig.RTCM1019)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1019b ontime {outConfig.NavInterval}\r\n"));
			}
			if (outConfig.RTCM1042)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1042b ontime {outConfig.NavInterval}\r\n"));
			}
			if (outConfig.RTCM1020)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1020b ontime {outConfig.NavInterval}\r\n"));
			}
			if (outConfig.RTCM1044)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1044b ontime {outConfig.NavInterval}\r\n"));
			}
			if (outConfig.RTCM1045)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1045b ontime {outConfig.NavInterval}\r\n"));
			}
			if (outConfig.RTCM1046)
			{
				commond.Add(Encoding.Default.GetBytes($"log {sysConfig.DataCom} rtcm1046b ontime {outConfig.NavInterval}\r\n"));
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
			m_sysConfig = sysConfig;



			List<byte[]> commond = new List<byte[]>();
			commond.AddRange(getOutCommond(sysConfig));
			string mm = "";
			foreach (byte[] ss in commond)
			{
				mm += Encoding.Default.GetString(ss);
			}
			commond.AddRange(getAntennaCommond(sysConfig.AntennaConfig));
			mm = "";
			foreach (byte[] ss in commond)
			{
				mm += Encoding.Default.GetString(ss);
			}
			commond.AddRange(getTrackCommond(sysConfig.TrackConfig));
			mm = "";
			foreach (byte[] ss in commond)
			{
				mm += Encoding.Default.GetString(ss);
			}
			commond.AddRange(getWorkModeCommond(sysConfig.WorkModeConfig));
			mm = "";
			foreach (byte[] ss in commond)
			{
				mm += Encoding.Default.GetString(ss);
			}
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
				string[] datas = str.Replace("\"", "").Split(" ");
				if (datas.Length >= 9)
				{
					SensorVersion sensorVersion = new SensorVersion();
					sensorVersion.Type = datas[11];
					sensorVersion.SwVersion = datas[14];
					sensorVersion.Model = datas[13];
					sensorVersion.Pn = datas[12];
					sensorVersion.Sn = datas[12];
					sensorVersion.EfuseID = datas[12];
					sensorVersion.CompTime = datas[16] + " " + datas[17];
					return sensorVersion;
				}
				else
				{
					log.Debug("ConfigService Data SensorVersion -- null");
					return null;
				}
			}
			catch (Exception e)
			{
				log.Error(e);

			}
			return null;
		}

		public ResultType getResultType(string str)
		{
			if (str.ToUpper().Contains("GPSCARD"))
			{
				return ResultType.VERSION;
			}
			return ResultType.UNKNOW;
		}
	}
}
