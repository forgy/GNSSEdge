using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;
using System.Text;

namespace NtripShare.GNSS.Device.Sensor
{
	public class ComNav : ISensor
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
			commond.Add(Encoding.Default.GetBytes($"ECUTOFF {trackConfig.SatelliteElevation}" + "\r\n"));

			if (trackConfig.UseGPS)
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL GPS ON" + "\r\n"));
            }
            else
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL GPS OFF" + "\r\n"));
            }
            if (trackConfig.UseBDS)
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL BD2 ON" + "\r\n"));
                commond.Add(Encoding.Default.GetBytes($"SET SIGNAL BD3 ON" + "\r\n"));
            }
            else
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL BD2 OFF" + "\r\n"));
                commond.Add(Encoding.Default.GetBytes($"SET SIGNAL BD3 OFF" + "\r\n"));
            }
            if (trackConfig.UseGLO)
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL GLONASS ON" + "\r\n"));
            }
            else
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL GLONASS OFF" + "\r\n"));
            }
            if (trackConfig.UseGAL)
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL GALILEO ON" + "\r\n"));
            }
            else
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL GALILEO OFF" + "\r\n"));
            }
            if (trackConfig.UseQZSS)
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL QZSS ON" + "\r\n"));
            }
            else
            {
				commond.Add(Encoding.Default.GetBytes($"SET SIGNAL QZSS OFF" + "\r\n"));
            }

            if (trackConfig.Smooth)
            {
                commond.Add(Encoding.Default.GetBytes("SET CPSMOOTHPR ON 60 15" + "\r\n"));
            }
            else
            {
				commond.Add(Encoding.Default.GetBytes("SET CPSMOOTHPR OFF 60 15" + "\r\n"));
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
				//commond.Add(会员未登录，无法保存或复制，谢谢!\r\n");
				//commond.Add("CONFIG RTK TIMEOUT 60" + "\r\n");
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
        public List<byte[]> getComOutCommond(ComConfig comConfig,OutConfig outConfig)
        {
            List<byte[]> commond = new List<byte[]>();

            commond.Add(Encoding.Default.GetBytes($"UNLOGALL {comConfig.SerialName}" + "\r\n"));
            if (comConfig.SerialStream == "nmea")
            {
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
					commond.Add(Encoding.Default.GetBytes($"log {comConfig.SerialName} GPZDA ontime {outConfig.NMEA0183Interval}1\r\n"));
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
			commond.Add(Encoding.Default.GetBytes($"UNLOGALL {sysConfig.ConfigCom}\r\n"));
			commond.Add(Encoding.Default.GetBytes("SAVECONFIG" + "\r\n"));
			commond.Add(Encoding.Default.GetBytes("log VERSIONA ONCE\r\n"));
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
            commond.Add(Encoding.Default.GetBytes($"SET ANTHIGH  {antennaConfig.Height}\r\n"));
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
			OutConfig outConfig = sysConfig.OutConfig;
			commond.Add(Encoding.Default.GetBytes($"NMEAVERSION V41\r\n"));
			commond.Add(Encoding.Default.GetBytes($"UNLOGALL \r\n"));
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
				commond.Add(Encoding.Default.GetBytes($"log GPZDA ontime {outConfig.NMEA0183Interval}1\r\n"));
			}

			commond.Add(Encoding.Default.GetBytes($"UNLOGALL {sysConfig.DataCom}\r\n"));
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
                string data = str.Split("")[1].Replace("\"", "");
                string[] datas = data.Split(",");
                if (datas.Length >= 6)
                {
                    SensorVersion sensorVersion = new SensorVersion();
                    sensorVersion.Type = datas[0];
                    sensorVersion.SwVersion = datas[4];
                    sensorVersion.Model = datas[1];
                    sensorVersion.Pn = datas[2];
                    sensorVersion.Sn = datas[2];
                    sensorVersion.EfuseID = datas[2];
                    sensorVersion.CompTime = datas[6]+ datas[7].Split("*")[0];
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
            if (str.ToUpper().StartsWith("#VERSION"))
            {
                return ResultType.VERSION;
            }
            return ResultType.UNKNOW;
        }
    }
}
