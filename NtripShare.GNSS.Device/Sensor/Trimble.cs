using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;
using System.Runtime.CompilerServices;
using System.Text;

namespace NtripShare.GNSS.Device.Sensor
{
	public class Trimble : ISensor
	{
		static ILog log = LogManager.GetLogger("NtripShare", typeof(Trimble));
		private readonly object m_lockObject = new object();
		private string m_message = "";
		private List<byte> buffer = new List<byte>();

		/// <summary>
		/// 处理配置串口数据
		/// </summary>
		/// <param name="bytes"></param>
		public void dealConfigMessage(byte[] bytes)
		{
			if (ConfigService.getInstance().SensorVersion == null)
			{
				if (bytes[0] == 0x02)
				{
					buffer.Clear();
				}
				buffer.AddRange(bytes);
				if (bytes[bytes.Length - 1] == 0x03)
				{
					ResultType resultType = getResultType(buffer.ToArray());
					if (resultType == ResultType.VERSION)
					{
						ConfigService.getInstance().SensorVersion = parseVersion(buffer.ToArray());
					}
					buffer.Clear();
				}

			}
			else
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
			List<byte[]> result = new List<byte[]>();

			List<byte> commond = new List<byte>();
			byte[] gps = new byte[34];
			gps[0] = 0x06;
			gps[1] = 0x20;
			if (trackConfig.UseGPS)
			{
				for (int i = 0; i < 32; i++)
				{
					gps[2 + i] = 0x00;
				}
			}
			else
			{
				for (int i = 0; i < 32; i++)
				{
					gps[2 + i] = 0x01;
				}
			}
			//byte[] data = genDataCollector64(gps);
			commond.AddRange(gps);


			byte[] bds = new byte[32];
			bds[0] = 0x50;
			bds[1] = 0x1E;
			if (trackConfig.UseBDS)
			{
				for (int i = 0; i < 30; i++)
				{
					bds[2 + i] = 0x00;
				}
			}
			else
			{
				for (int i = 0; i < 30; i++)
				{
					bds[2 + i] = 0x01;
				}
			}
			commond.AddRange(bds);

			byte[] glo = new byte[26];
			glo[0] = 0x37;
			glo[1] = 0x18;
			if (trackConfig.UseGLO)
			{
				for (int i = 0; i < 24; i++)
				{
					glo[2 + i] = 0x00;
				}
			}
			else
			{
				for (int i = 0; i < 24; i++)
				{
					glo[2 + i] = 0x01;
				}
			}
			commond.AddRange(glo);

			byte[] gal = new byte[54];
			gal[0] = 0x4F;
			gal[1] = 0x34;
			if (trackConfig.UseGAL)
			{
				for (int i = 0; i < 52; i++)
				{
					gal[2 + i] = 0x00;
				}
			}
			else
			{
				for (int i = 0; i < 52; i++)
				{
					gal[2 + i] = 0x01;
				}
			}
			commond.AddRange(gal);

			byte[] qzss = new byte[7];
			qzss[0] = 0x5F;
			qzss[1] = 0x05;
			if (trackConfig.UseQZSS)
			{
				for (int i = 0; i < 5; i++)
				{
					qzss[2 + i] = 0x00;
				}
			}
			else
			{
				for (int i = 0; i < 5; i++)
				{
					qzss[2 + i] = 0x01;
				}
			}
			commond.AddRange(qzss);

			byte[] data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			string hex = BitConverter.ToString(data, 0).Replace("-", " ");
			result.Add(data);

			return result;
		}

		/// <summary>
		/// 获取工作模式指令
		/// </summary>
		/// <param name="workModeConfig"></param>
		/// <returns></returns>
		public List<byte[]> getWorkModeCommond(WorkModeConfig workModeConfig)
		{
			List<byte> commond = new List<byte>();
			if (workModeConfig.WorkMode == (int)WorkMode.ROVER)
			{
				byte[] data = new byte[6];
				data[0] = 0x17;
				data[1] = 0x04;
				data[5] = 0x00;
				commond.AddRange(data);

				data = new byte[] { 0x0A, 0x01, 0x00 };
				commond.AddRange(data);
			}
			if (workModeConfig.WorkMode == (int)WorkMode.BASE || workModeConfig.WorkMode == (int)WorkMode.AUTOBASE)
			{
				byte[] data = new byte[6];
				data[0] = 0x17;
				data[1] = 0x04;
				data[5] = 0x01;
				commond.AddRange(data);///basemode

				data = new byte[] { 0x0A, 0x01, 0x01 };
				commond.AddRange(data);

				data = new byte[39];
				data[0] = 0x03;
				data[1] = 0x25;
				data[4] = 0x42;
				data[5] = 0x41;
				data[6] = 0x53;
				data[7] = 0x45;
				data[8] = 0x20;
				data[9] = 0x20;
				data[10] = 0x20;
				data[11] = 0x20;

				byte[] BaseLat = doubleToByte(workModeConfig.BaseLat * workModeConfig.BaseLatType * Math.PI / 180);
				byte[] BaseLon = doubleToByte(workModeConfig.BaseLon * workModeConfig.BaseLonType * Math.PI / 180);
				byte[] height = doubleToByte(workModeConfig.BaseHeight);

				for (int i = 0; i < 8; i++)
				{
					data[12 + i] = BaseLat[i];
					data[20 + i] = BaseLon[i];
					data[28 + i] = height[i];
				}
				data[36] = 0x02;
				data[37] = 0xD1;
				data[38] = 0x01;
				commond.AddRange(data);
			}
			byte[] datas = genDataCollector64(commond.ToArray());
			datas = genDataCollector(0x64, datas);
			string hex = BitConverter.ToString(datas, 0).Replace("-", " ");
			return new List<byte[]> { datas };
		}

		/// <summary>
		/// 转数据
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static byte[] doubleToByte(double val)
		{
			long bits = AsInt64(val);
			string mmm = $"{bits:X}";
			byte[] b = StrToHexByte(mmm);
			return b;
		}

		/// <summary>
		/// 将16进制的字符串转为byte[]
		/// </summary>
		/// <param name="hexString"></param>
		/// <returns></returns>
		public static byte[] StrToHexByte(string hexString)
		{
			hexString = hexString.Replace(" ", "");
			if ((hexString.Length % 2) != 0)
				hexString += " ";
			byte[] returnBytes = new byte[hexString.Length / 2];
			for (int i = 0; i < returnBytes.Length; i++)
				returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
			return returnBytes;
		}


		public static long AsInt64<Tfrom>(Tfrom from) => Unsafe.As<Tfrom, long>(ref from);

		/// <summary>
		/// 串口输出数据配置
		/// </summary>
		/// <param name="comConfig"></param>
		/// <returns></returns>
		public List<byte[]> getComOutCommond(ComConfig comConfig, OutConfig outConfig)
		{
			List<byte> commond = new List<byte>();
			byte[] data = new byte[] { 0x07, 0x05, 0xFF, 0x00, 0x00, 0x00, 0x00 };
			if (comConfig.SerialName == "Com1")
			{
				data[3] = 0x00;
			}
			if (comConfig.SerialName == "Com2")
			{
				data[3] = 0x01;
			}
			if (comConfig.SerialName == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);

			if (comConfig.SerialStream == "nmea")
			{
				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 0x06;//GGA
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);

				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 38;//GPGSA
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);

				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 13;//GPGST
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);

				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 18;//GPGSV
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);

				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 31;//GPHDT
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);

				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 40;//GPHDT
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);

				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 12;//GPVTG
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);

				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 44;//GPGLL
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);

				data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
				data[2] = 8;//GPZDA
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);
			}
			if (comConfig.SerialStream == "rtcm3")
			{
				data = new byte[] { 0x07, 0x06, 0x03, 0x00, 0x03, 0x00, 0x21, 0x00 };
				if (comConfig.SerialName == "Com1")
				{
					data[3] = 0x00;
				}
				if (comConfig.SerialName == "Com2")
				{
					data[3] = 0x01;
				}
				if (comConfig.SerialName == "Com3")
				{
					data[3] = 0x02;
				}
				commond.AddRange(data);
			}
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			return new List<byte[]> { data };
		}

		/// <summary>
		/// 获取版本信息指令
		/// </summary>
		/// <returns></returns>
		public List<byte[]> getVersionCommond(SysConfig sysConfig)
		{
			List<byte[]> commond = new List<byte[]>();
			byte type = 0x06;
			byte[] re = genDataCollector(type, new byte[0]);
			commond.Add(re);
			return commond;
		}

		/// <summary>
		/// 获取天线模式命令
		/// </summary>
		/// <param name="antennaConfig"></param>
		/// <returns></returns>
		public List<byte[]> getAntennaCommond(AntennaConfig antennaConfig)
		{
			byte[] ANTENNAHEIGHT = doubleToByte(antennaConfig.Height);
			byte[] data = new byte[14];
			data[0] = 0x08;
			data[1] = 0x0C;

			for (int i = 0; i < 8; i++)
			{
				data[2 + i] = ANTENNAHEIGHT[i];
			}
			data = genDataCollector64(data);
			data = genDataCollector(0x64, data);

			string hex = BitConverter.ToString(data, 0).Replace("-", " ");
			return new List<byte[]> { data };
		}

		/// <summary>
		/// 获取数据输出命令
		/// </summary>
		/// <param name="sysConfig"></param>
		/// <returns></returns>
		public List<byte[]> getOutCommond(SysConfig sysConfig)
		{
			string mm = "";
			List<byte> commond = new List<byte>();
			byte[] data = new byte[] { 0x07, 0x05, 0xFF, 0x00, 0x00, 0x00, 0x00 };
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			var hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 0x06;//GGA
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 38;//GPGSA
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 13;//GPGST
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 18;//GPGSV
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 31;//GPHDT
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 40;//GPHDT
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 12;//GPVTG
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 44;//GPGLL
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			data = new byte[] { 0x07, 0x04, 0x06, 0x01, 0x03, 0x00 };
			data[2] = 8;//GPZDA
			if (sysConfig.ConfigCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.ConfigCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.ConfigCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");

			///RTCM
			data = new byte[] { 0x07, 0x06, 0x03, 0x00, 0x03, 0x00, 0x21, 0x00 };
			if (sysConfig.DataCom == "Com1")
			{
				data[3] = 0x00;
			}
			if (sysConfig.DataCom == "Com2")
			{
				data[3] = 0x01;
			}
			if (sysConfig.DataCom == "Com3")
			{
				data[3] = 0x02;
			}
			commond.AddRange(data);
			data = genDataCollector64(commond.ToArray());
			data = genDataCollector(0x64, data);
			hex = BitConverter.ToString(data, 0).Replace("-", " ");
			mm += hex;
			log.Debug(hex + "----");
			List<byte[]> re = new List<byte[]>();
			re.Add(data);
			return re;
		}

		/// <summary>
		/// 获取初始化板卡指令
		/// </summary>
		/// <param name="sysConfig"></param>
		/// <returns></returns>
		public List<byte[]> getInitCommond(SysConfig sysConfig)
		{
			List<byte[]> re = new List<byte[]>();
			re.AddRange(getOutCommond(sysConfig));
			re.AddRange(getAntennaCommond(sysConfig.AntennaConfig));
			re.AddRange(getTrackCommond(sysConfig.TrackConfig));
			re.AddRange(getWorkModeCommond(sysConfig.WorkModeConfig));

			return re;
		}


		/// <summary>
		/// 解析版本信息
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public SensorVersion parseVersion(string str)
		{
			byte[] data = Encoding.Default.GetBytes(str);
			try
			{
				SensorVersion sensorVersion = new SensorVersion();
				sensorVersion.Type = Encoding.Default.GetString(data, 4, 11);
				sensorVersion.SwVersion = Encoding.Default.GetString(data, 20, 24);
				sensorVersion.Model = Encoding.Default.GetString(data, 12, 19);
				sensorVersion.Pn = Encoding.Default.GetString(data, 4, 11);
				sensorVersion.Sn = Encoding.Default.GetString(data, 49, 58);
				sensorVersion.EfuseID = Encoding.Default.GetString(data, 4, 11);
				sensorVersion.CompTime = Encoding.Default.GetString(data, 30, 34);
				return sensorVersion;

			}
			catch (Exception e)
			{

			}
			return null;
		}

		public SensorVersion parseVersion(byte[] data)
		{
			try
			{
				SensorVersion sensorVersion = new SensorVersion();
				sensorVersion.Type = Encoding.Default.GetString(data, 4, 8);
				sensorVersion.SwVersion = Encoding.Default.GetString(data, 20, 5);
				sensorVersion.Model = Encoding.Default.GetString(data, 12, 8);
				sensorVersion.Pn = Encoding.Default.GetString(data, 4, 8);
				sensorVersion.Sn = Encoding.Default.GetString(data, 49, 10);
				sensorVersion.EfuseID = Encoding.Default.GetString(data, 4, 8);
				sensorVersion.CompTime = Encoding.Default.GetString(data, 30, 4);
				return sensorVersion;

			}
			catch (Exception e)
			{

			}
			return null;
		}

		public ResultType getResultType(string str)
		{
			return getResultType(Encoding.Default.GetBytes(str));
		}

		public ResultType getResultType(byte[] str)
		{
			if (str[0] == 0x02 && str[str.Length - 1] == 0x03 && str[2] == 0x07)
			{
				return ResultType.VERSION;
			}
			return ResultType.UNKNOW;
		}


		/// <summary>
		/// 生成消息
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private byte[] genDataCollector(byte type, byte[] data)
		{

			byte[] ret = new byte[data.Length + 6];
			ret[0] = 0x02;
			ret[1] = 0x00;
			ret[2] = type;
			ret[3] = (byte)data.Length;

			for (int i = 0; i < data.Length; i++)
			{
				ret[4 + i] = data[i];
			}
			byte CHECKSUM = 0;
			for (int i = 1; i < ret.Length - 2; i++)
			{
				CHECKSUM += ret[i];
			}
			ret[ret.Length - 2] = CHECKSUM;
			ret[ret.Length - 1] = 0x03;
			BitConverter.ToString(ret).Replace("-", " ");
			return ret;
		}

		/// <summary>
		/// 生成消息
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private byte[] genDataCollector64(byte[] data)
		{

			byte[] ret = new byte[data.Length + 7];
			ret[3] = 0x03;
			ret[5] = 0x01;

			for (int i = 0; i < data.Length; i++)
			{
				ret[7 + i] = data[i];
			}
			return ret;
		}
	}
}
