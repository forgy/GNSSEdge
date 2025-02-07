using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;
using NtripShare.GNSS.Service;
using System.Runtime.CompilerServices;
using System.Text;

namespace NtripShare.GNSS.Device.Sensor
{
    public class UbloxF9P : ISensor
    {
        static ILog log = LogManager.GetLogger("NtripShare", typeof(UbloxF9P));
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
                string message = System.Text.Encoding.ASCII.GetString(bytes);

                if (bytes[0] == 0xB5 && bytes[1] == 0x62)
                {
                    buffer.Clear();
                }
                buffer.AddRange(bytes);

                ResultType resultType = getResultType(buffer.ToArray());
                if (resultType == ResultType.VERSION)
                {
                    ConfigService.getInstance().SensorVersion = parseVersion(buffer.ToArray());
                    buffer.Clear();
                }

                if (buffer.Count > 1024)
                {
                    buffer.Clear();
                }
            }
            else
            {
                if (bytes[0] == 0xB5 && bytes[1] == 0x62)
                {
                    log.Info("Ublox Commond");
                }
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
            if (trackConfig.UseGPS)
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x1F, 0x00, 0x31, 0x10, 0x01, 0xFC, 0x89 });
            }
            else
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x1F, 0x00, 0x31, 0x10, 0x00, 0xFB, 0x88 });
            }

            if (trackConfig.UseBDS)
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x22, 0x00, 0x31, 0x10, 0x01, 0xFF, 0x98 });
            }
            else
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x22, 0x00, 0x31, 0x10, 0x00, 0xFE, 0x97 });
            }

            if (trackConfig.UseGLO)
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x25, 0x00, 0x31, 0x10, 0x01, 0x02, 0xA7 });
            }
            else
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x25, 0x00, 0x31, 0x10, 0x00, 0x01, 0xA6 });
            }

            if (trackConfig.UseGAL)
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x21, 0x00, 0x31, 0x10, 0x01, 0xFE, 0x93 });
            }
            else
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x21, 0x00, 0x31, 0x10, 0x00, 0xFD, 0x92 });
            }

            if (trackConfig.UseQZSS)
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x24, 0x00, 0x31, 0x10, 0x01, 0x01, 0xA2 });
            }
            else
            {
                result.Add(new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x24, 0x00, 0x31, 0x10, 0x00, 0x00, 0xA1 });
            }

            return result;
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
                byte[] data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0x01, 0x00, 0x03, 0x20, 0x00, 0xC0, 0x90 };//CFG - TMODE - MODE DISABLED
                commond.Add(data);
                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x03, 0x20, 0x00, 0xBF, 0x88 };//CFG - TMODE - MODE DISABLED
                commond.Add(data);
            }
            if (workModeConfig.WorkMode == (int)WorkMode.BASE)
            {
                byte[] data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0x01, 0x00, 0x03, 0x20, 0x02, 0xC2, 0x92 };//CFG-TMODE-MODE FIX
                commond.Add(data);///basemode
				data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x03, 0x20, 0x02, 0xC1, 0x8A };//CFG-TMODE-MODE FIX
                commond.Add(data);


                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0x02, 0x00, 0x03, 0x20, 0x01, 0xC2, 0x96 };//CFG-TMODE-POS_TYPE 
                commond.Add(data);
                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x02, 0x00, 0x03, 0x20, 0x01, 0xC1, 0x8E };//CFG-TMODE-POS_TYPE 
                commond.Add(data);



                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x02, 0x00, 0x00, 0x09, 0x00, 0x03, 0x40, 0x00, 0x00, 0x00, 0x00, 0xEB, 0xDA };//CFG - TMODE - LAT
                byte[] BaseLat = intToBytes((int)(workModeConfig.BaseLat * workModeConfig.BaseLatType * 10000000));
                for (int i = 0; i < 4; i++)
                {
                    data[14 + i] = BaseLat[i];
                }
                data = updateChecksum(data);
                commond.Add(data);
                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x01, 0x00, 0x00, 0x09, 0x00, 0x03, 0x40, 0x00, 0x00, 0x00, 0x00, 0xEA, 0xCF };//CFG - TMODE - LAT
                BaseLat = intToBytes((int)(workModeConfig.BaseLat * workModeConfig.BaseLatType * 10000000));
                for (int i = 0; i < 4; i++)
                {
                    data[14 + i] = BaseLat[i];
                }
                data = updateChecksum(data);
                commond.Add(data);
                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x02, 0x00, 0x00, 0x0A, 0x00, 0x03, 0x40, 0x00, 0x00, 0x00, 0x00, 0xEC, 0xE2 };//CFG-TMODE-LON
                BaseLat = intToBytes((int)(workModeConfig.BaseLon * workModeConfig.BaseLonType * 10000000));
                for (int i = 0; i < 4; i++)
                {
                    data[14 + i] = BaseLat[i];
                }
                data = updateChecksum(data);
                commond.Add(data);
                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x01, 0x00, 0x00, 0x0A, 0x00, 0x03, 0x40, 0x00, 0x00, 0x00, 0x00, 0xEA, 0xCF };//CFG-TMODE-LON
                BaseLat = intToBytes((int)(workModeConfig.BaseLon * workModeConfig.BaseLonType * 10000000));
                for (int i = 0; i < 4; i++)
                {
                    data[14 + i] = BaseLat[i];
                }
                data = updateChecksum(data);
                commond.Add(data);

                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x02, 0x00, 0x00, 0x0B, 0x00, 0x03, 0x40, 0x00, 0x00, 0x00, 0x00, 0xED, 0xEA };//CFG-TMODE-HEIGHT
                BaseLat = intToBytes((int)(workModeConfig.BaseHeight * 100));
                for (int i = 0; i < 4; i++)
                {
                    data[14 + i] = BaseLat[i];
                }
                data = updateChecksum(data);
                commond.Add(data);

                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x01, 0x00, 0x00, 0x0B, 0x00, 0x03, 0x40, 0x00, 0x00, 0x00, 0x00, 0xEA, 0xCF };//CFG-TMODE-HEIGHT
                BaseLat = intToBytes((int)(workModeConfig.BaseHeight * 100));
                for (int i = 0; i < 4; i++)
                {
                    data[14 + i] = BaseLat[i];
                }
                data = updateChecksum(data);
                commond.Add(data);
            }
            if (workModeConfig.WorkMode == (int)WorkMode.AUTOBASE)
            {
                byte[] data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0x01, 0x00, 0x03, 0x20, 0x01, 0xC1, 0x91 };//CFG-TMODE-MODE 1 - SURVEY_IN
                commond.Add(data);///basemode
				data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x03, 0x20, 0x01, 0xC0, 0x89 };//CFG-TMODE-MODE 1 - SURVEY_IN
                commond.Add(data);///basemode


                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x02, 0x00, 0x00, 0x10, 0x00, 0x03, 0x40, 0x58, 0x02, 0x00, 0x00, 0x4C, 0x78 };//CFG-TMODE-SVIN_MIN_DUR 600S
                commond.Add(data);///basemode
                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x01, 0x00, 0x00, 0x10, 0x00, 0x03, 0x40, 0x58, 0x02, 0x00, 0x00, 0x4B, 0x6D };//CFG-TMODE-SVIN_MIN_DUR 600S
                commond.Add(data);///basemode


                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x02, 0x00, 0x00, 0x11, 0x00, 0x03, 0x40, 0x10, 0x27, 0x00, 0x00, 0x2A, 0xCF };//CFG-TMODE-SVIN_ACC_LIMIT 1M
                commond.Add(data);///basemode
                data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x0C, 0x00, 0x01, 0x01, 0x00, 0x00, 0x11, 0x00, 0x03, 0x40, 0x10, 0x27, 0x00, 0x00, 0x29, 0xC4 };//CFG-TMODE-SVIN_ACC_LIMIT 1M
                commond.Add(data);///basemode
			}
            return commond;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] intToBytes(int value)
        {
            byte[] src = new byte[4];
            src[3] = (byte)((value >> 24) & 0xFF);
            src[2] = (byte)((value >> 16) & 0xFF);
            src[1] = (byte)((value >> 8) & 0xFF);
            src[0] = (byte)(value & 0xFF);
            return src;
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
            List<byte[]> commond = new List<byte[]>();

            if (comConfig.SerialStream == "nmea")
            {
                byte[] re = new byte[] { 0xB5, 0x62, 0x06, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0xD0, 0x08, 0x00, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x23, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0xDB, 0x58 };
                commond.Add(re);
            }
            if (comConfig.SerialStream == "rtcm3")
            {
                byte[] re = new byte[] { 0xB5, 0x62, 0x06, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00, 0xD0, 0x08, 0x00, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x23, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF9, 0x0C };
                commond.Add(re);
            }

            return commond;
        }

        /// <summary>
        /// 获取版本信息指令
        /// </summary>
        /// <returns></returns>
        public List<byte[]> getVersionCommond(SysConfig sysConfig)
        {
            List<byte[]> commond = new List<byte[]>();

            byte[] re = new byte[] { 0xB5, 0x62, 0x0A, 0x04, 0x00, 0x00, 0x0E, 0x34 };
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
            //byte[] ANTENNAHEIGHT = doubleToByte(antennaConfig.Height);
            //byte[] data = new byte[14];
            //data[0 ] = 0x08;
            //data[1 ] = 0x0C;

            //for (int i = 0; i < 8; i++)
            //{
            //	data[2 + i] = ANTENNAHEIGHT[i];
            //}
            //data = genDataCollector64(data);
            //data = genDataCollector(0x64, data);

            //string hex = BitConverter.ToString(data, 0).Replace("-", " ");
            return new List<byte[]>();
        }

        /// <summary>
        /// 获取数据输出命令
        /// </summary>
        /// <param name="sysConfig"></param>
        /// <returns></returns>
        public List<byte[]> getOutCommond(SysConfig sysConfig) {
			List<byte[]> commond = new List<byte[]>();
			byte[] data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0x02, 0x00, 0x74, 0x10, 0x01, 0x23, 0xC9 };//CFG-UART1OUTPROT-NMEA
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x02, 0x00, 0x74, 0x10, 0x01, 0x23, 0xC9 };//CFG-UART1OUTPROT-NMEA
			data = updateChecksum(data);
			commond.Add(data);

			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0x01, 0x00, 0x74, 0x10, 0x00, 0x21, 0xC3 };//CFG-UART1OUTPROT-UBX
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x04, 0x00, 0x74, 0x10, 0x00, 0x23, 0xCA };//CFG-UART1OUTPROT-UBX
			commond.Add(data);

			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0x04, 0x00, 0x74, 0x10, 0x00, 0x24, 0xD2 };//CFG-UART1OUTPROT-RTCM3X
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x04, 0x00, 0x74, 0x10, 0x00, 0x23, 0xCA };//CFG-UART1OUTPROT-RTCM3X
			data = updateChecksum(data);
			commond.Add(data);

			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0xBB, 0x00, 0x91, 0x20, 0x01, 0x09, 0xDD };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0xCA, 0x00, 0x91, 0x20, 0x01, 0x18, 0x28 };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0xC0, 0x00, 0x91, 0x20, 0x01, 0x0E, 0xF6 };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0xD4, 0x00, 0x91, 0x20, 0x01, 0x22, 0x5A };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0xC5, 0x00, 0x91, 0x20, 0x01, 0x13, 0x0F };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0xAC, 0x00, 0x91, 0x20, 0x01, 0xFA, 0x92 };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0xB1, 0x00, 0x91, 0x20, 0x01, 0xFF, 0xAB };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x02, 0x00, 0x00, 0xD9, 0x00, 0x91, 0x20, 0x00, 0x26, 0x72 };
			commond.Add(data);


			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xBB, 0x00, 0x91, 0x20, 0x01, 0x09, 0xDD };
			data = updateChecksum(data);
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xCA, 0x00, 0x91, 0x20, 0x01, 0x18, 0x28 };
			data = updateChecksum(data);
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xC0, 0x00, 0x91, 0x20, 0x01, 0x0E, 0xF6 };
			data = updateChecksum(data);
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xD4, 0x00, 0x91, 0x20, 0x01, 0x22, 0x5A };
			data = updateChecksum(data);
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xC5, 0x00, 0x91, 0x20, 0x01, 0x13, 0x0F };
			data = updateChecksum(data);
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xAC, 0x00, 0x91, 0x20, 0x01, 0xFA, 0x92 };
			data = updateChecksum(data);
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xB1, 0x00, 0x91, 0x20, 0x01, 0xFF, 0xAB };
			data = updateChecksum(data);
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xD9, 0x00, 0x91, 0x20, 0x00, 0x26, 0x72 };
			data = updateChecksum(data);
			commond.Add(data);


			data = new byte[] { 0xB5, 0x62, 0x06, 0x00, 0x01, 0x00, 0x02, 0x09, 0x23, 0xB5, 0x62, 0x06, 0x00, 0x14, 0x00, 0x02, 0x00,
				0x00, 0x00, 0xD0, 0x08, 0x00, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x20, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF7, 0x08,
				0xB5, 0x62, 0x06, 0x00, 0x01, 0x00, 0x02, 0x09, 0x23 };//uART2 115200 RTCM IN and Out
			commond.Add(data);


			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xBF, 0x02, 0x91, 0x20, 0x05, 0x12, 0xF5 };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x60, 0x03, 0x91, 0x20, 0x01, 0xB0, 0x1A };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xCE, 0x02, 0x91, 0x20, 0x01, 0x1D, 0x3C };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x65, 0x03, 0x91, 0x20, 0x01, 0xB5, 0x33 };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xD3, 0x02, 0x91, 0x20, 0x01, 0x22, 0x55 };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x6A, 0x03, 0x91, 0x20, 0x01, 0xBA, 0x4C };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x1A, 0x03, 0x91, 0x20, 0x01, 0x6A, 0xBC };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x6F, 0x03, 0x91, 0x20, 0x01, 0xBF, 0x65 };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0xD8, 0x02, 0x91, 0x20, 0x01, 0x27, 0x6E };
			commond.Add(data);

			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x05, 0x03, 0x91, 0x20, 0x01, 0x55, 0x53 };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x03, 0x91, 0x20, 0x01, 0x50, 0x3A };
			commond.Add(data);
			data = new byte[] { 0xB5, 0x62, 0x06, 0x8A, 0x09, 0x00, 0x01, 0x01, 0x00, 0x00, 0x83, 0x03, 0x91, 0x20, 0x01, 0xD3, 0xC9 };
			commond.Add(data);

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
                string[] msg = Encoding.Default.GetString(data).Split(" ");
                SensorVersion sensorVersion = new SensorVersion();
                sensorVersion.Type = Encoding.Default.GetString(data, 4, 8);
                sensorVersion.SwVersion = "61b2dd";
                sensorVersion.Model = "ZED_F9P";
                sensorVersion.Pn = "00190000";
                sensorVersion.Sn = "00190000";
                sensorVersion.EfuseID = "00190000";
                sensorVersion.CompTime = "";
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
            if (str[0] == 0xB5 && str[1] == 0x62 && str[2] == 0x0A && str[3] == 0x04)
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
        private byte[] updateChecksum(byte[] data)
        {

            data[data.Length - 1] = 0;
            data[data.Length - 2] = 0;
            for (int i = 2; i < data.Length - 2; i++)
            {
                data[data.Length - 2] += data[i];
                data[data.Length - 1] += data[data.Length - 2];
            }
            return data;
        }
    }
}
