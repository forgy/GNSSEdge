using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.NTRIP;
using System.Globalization;

namespace NtripShare.GNSS.Device.NMEA
{
    public class NMEAHelper
    {
        static ILog log = LogManager.GetLogger("NtripShare", typeof(NMEAHelper));
        internal static NumberFormatInfo NumberFormatEnUs = new CultureInfo("en-US", false).NumberFormat;
        DateTime GSATime = DateTime.Now;
        int gsaNum = 0;
        /// <summary>
        /// Recommended minimum specific GPS/Transit data
        /// </summary>
        public GPRMC GPRMC;
        /// <summary>
        /// Global Positioning System Fix Data
        /// </summary>
        public GPGGA GPGGA { get; set; }

        public List<GPGGA> GPGGABffer { get; set; }= new List<GPGGA>();
        /// <summary>
        /// Satellites in view
        /// </summary>
        public GPGSV GPGSV;
        /// <summary>
        /// Satellites in view
        /// </summary>
        public BDGSV BDGSV;
        /// <summary>
        /// Satellites in view
        /// </summary>
        public GAGSV GAGSV;
        /// <summary>
        /// Satellites in view
        /// </summary>
        public GLGSV GLGSV;
        /// <summary>
        /// Satellites in view
        /// </summary>
        public GQGSV GQGSV;
        /// <summary>
        /// GPS DOP and active satellites
        /// </summary>
        public GPGSA GPGSA;
        /// <summary>
        /// Geographic position, Latitude and Longitude
        /// </summary>
        public GPGLL GPGLL;
        /// <summary>
        /// Estimated Position Error - Garmin proprietary sentence(!)
        /// </summary>
        public GPRME PGRME;
		public GPZDA GPZDA;
		public GPGST GPGST;
		public GPVTG GPVTG;

		///// <summary>
		///// Overridden. Fires when the GpsHandler has received data from the GPS device.
		///// </summary>
		//public event EventHandler<GPSEventArgs> NewGPSFix;

		///// <summary>
		///// Event fired whenever new GPS data has been processed. Runs in GPS thread
		///// </summary>
		//private event EventHandler<GPSEventArgs> _NewGPSFix;


		public NMEAHelper()
        {
            GPRMC = new GPRMC();
            GPGGA = new GPGGA();
            GPGSA = new GPGSA();
            GPRMC = new GPRMC();
            PGRME = new GPRME();
            GPGSV = new GPGSV();

            GPGSV = new GPGSV();
            BDGSV = new BDGSV();
            GAGSV = new GAGSV();
            GLGSV = new GLGSV();
            GQGSV = new GQGSV();
            GPZDA = new GPZDA();
		}

        /// <summary>
        /// 获取卫星列表
        /// </summary>
        /// <returns></returns>
        public List<Satellite> GetSatellites()
        {
            List<Satellite> list = new List<Satellite>();
            list.AddRange(GPGSV.Satellites);
            list.AddRange(BDGSV.Satellites);
            list.AddRange(GAGSV.Satellites);
            list.AddRange(GLGSV.Satellites);
            list.AddRange(GQGSV.Satellites);
            return list;
        }

        /// <summary>
        /// Eventtype invoked when a new message is received from the GPS.
        /// String GPSEventArgs.TypeOfEvent specifies eventtype.
        /// </summary>
        public class GPSEventArgs : EventArgs
        {
            /// <summary>
            /// Type of event
            /// </summary>
            public GPSEventType TypeOfEvent;
            /// <summary>
            /// Full NMEA sentence
            /// </summary>
            public string Sentence;
        }

        /// <summary>
        /// Returns Garmin estimated horisontal error. This is Garmin proprietary message and may not function with all GPS devices.
        /// </summary>
        public double GPSAccuracy
        {
            get
            {
                return PGRME.EstHorisontalError;
            }
        }


        /// <summary>
        /// Method called when a GPS event occured.
        /// This is where we call the methods that parses each kind of NMEA sentence
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GPSDataEventHandler(object sender, GPSEventArgs e)
        {
            switch (e.TypeOfEvent)
            {
                case GPSEventType.GPRMC:
                    ParseRMC(e.Sentence);
                    break;
                case GPSEventType.GPGGA:
                    ParseGGA(e.Sentence);
                    break;
                case GPSEventType.GPGLL:
                    ParseGLL(e.Sentence);
                    break;
                case GPSEventType.GPGSA:
                    ParseGSA(e.Sentence);
                    break;
                case GPSEventType.GPGSV:
                    ParseGPGSV(e.Sentence);
                    break;
                case GPSEventType.GAGSV:
                    ParseGAGSV(e.Sentence);
                    break;
                case GPSEventType.GLGSV:
                    ParseGLGSV(e.Sentence);
                    break;
                case GPSEventType.GQGSV:
                    ParseGQGSV(e.Sentence);
                    break;
                case GPSEventType.BDGSV:
                    ParseBDGSV(e.Sentence);
                    break;
				case GPSEventType.GPZDA:
					ParseGPZDA(e.Sentence);
					break;
				case GPSEventType.PGRME:
                    ParseRME(e.Sentence);
                    break;
				case GPSEventType.GPVTG:
					ParseVTG(e.Sentence);
					break;
				//case GPSEventType.TimeOut:
				//    FireTimeOut();
				//    break;
				case GPSEventType.Unknown:
                    GPSEventArgs e2 = new GPSEventArgs();
                    e2.TypeOfEvent = e.TypeOfEvent;
                    e2.Sentence = e.Sentence;
                    //_NewGPSFix(this, e2);
                    log.Debug($"GPSEventType.Unknown:{e.Sentence}");
                    break;
                default: break;
            }
        }

		public void ParseVTG(string strGLL)
		{
			//log.Debug("ParseVTG");
			GPVTG = new GPVTG(strGLL);
			//GPSEventArgs e = new GPSEventArgs();
			//e.TypeOfEvent = GPSEventType.GPGLL;
			//e.Sentence = strGLL;
			//_NewGPSFix(this, e);
		}
		/// <summary>
		/// Private method for Firing a serialport timeout event
		/// </summary>
		//private void FireTimeOut()
		//{
		//    GPGGA.FixQuality = NMEA.GPGGA.FixQualityEnum.Invalid;
		//    GPSEventArgs e = new GPSEventArgs();
		//    e.TypeOfEvent = GPSEventType.TimeOut;
		//    //_NewGPSFix(this, e);
		//}
		/// <summary>
		/// Private method for parsing the GPGLL NMEA sentence
		/// </summary>
		/// <param name="strGLL">GPGLL sentence</param>
		public void ParseGST(string strGLL)
		{
			//log.Debug("ParseGST");
			GPGST = new GPGST(strGLL);
			//GPSEventArgs e = new GPSEventArgs();
			//e.TypeOfEvent = GPSEventType.GPGLL;
			//e.Sentence = strGLL;
			//_NewGPSFix(this, e);
		}

		/// <summary>
		/// Private method for parsing the GPGLL NMEA sentence
		/// </summary>
		/// <param name="strGLL">GPGLL sentence</param>
		public void ParseGLL(string strGLL)
        {
            //log.Debug("ParseGLL");
            GPGLL = new GPGLL(strGLL);
            //GPSEventArgs e = new GPSEventArgs();
            //e.TypeOfEvent = GPSEventType.GPGLL;
            //e.Sentence = strGLL;
            //_NewGPSFix(this, e);
        }

		public void ParseGPZDA(string strGLL)
		{
			//log.Debug("ParseGPZDA");
			GPZDA = new GPZDA(strGLL);
			//GPSEventArgs e = new GPSEventArgs();
			//e.TypeOfEvent = GPSEventType.GPGLL;
			//e.Sentence = strGLL;
			//_NewGPSFix(this, e);
		}

		/// <summary>
		/// Private method for parsing the GPGSV NMEA sentence
		/// GPGSV is a bit different, since it if usually made from several NMEA sentences
		/// </summary>
		/// <param name="strGSV">GPGSV sentence</param>
		public void ParseGPGSV(string strGSV)
        {
            //fire the event if last GSV message.
            //log.Debug("ParseGPGSV");
            if (GPGSV.AddSentence(strGSV))
            {
				
				//GPSEventArgs e = new GPSEventArgs();
				//e.TypeOfEvent = GPSEventType.GPGSV;
				//e.Sentence = strGSV;
				//_NewGPSFix(this, e);
			}
			//log.Debug($"ParseGPGSV {GPGSV.Satellites.Count}");
		}

		/// <summary>
		/// Private method for parsing the GPGSV NMEA sentence
		/// GPGSV is a bit different, since it if usually made from several NMEA sentences
		/// </summary>
		/// <param name="strGSV">GPGSV sentence</param>
		public void ParseGAGSV(string strGSV)
        {
            //log.Debug("ParseGAGSV");
            //fire the event if last GSV message.
            if (GAGSV.AddSentence(strGSV))
            {
                //GPSEventArgs e = new GPSEventArgs();
                //e.TypeOfEvent = GPSEventType.GAGSV;
                //e.Sentence = strGSV;
                //_NewGPSFix(this, e);
            }
        }


		/// <summary>
		/// Private method for parsing the GPGSV NMEA sentence
		/// GPGSV is a bit different, since it if usually made from several NMEA sentences
		/// </summary>
		/// <param name="strGSV">GPGSV sentence</param>
		public void ParseGLGSV(string strGSV)
        {
            //fire the event if last GSV message.
            if (GLGSV.AddSentence(strGSV))
            {
				//GPSEventArgs e = new GPSEventArgs();
				//e.TypeOfEvent = GPSEventType.GLGSV;
				//e.Sentence = strGSV;
				//_NewGPSFix(this, e);
			}
        }


		/// <summary>
		/// Private method for parsing the GPGSV NMEA sentence
		/// GPGSV is a bit different, since it if usually made from several NMEA sentences
		/// </summary>
		/// <param name="strGSV">GPGSV sentence</param>
		public void ParseGQGSV(string strGSV)
        {
            //log.Debug("ParseGQGSV");
            //fire the event if last GSV message.
            if (GQGSV.AddSentence(strGSV))
            {
                //GPSEventArgs e = new GPSEventArgs();
                //e.TypeOfEvent = GPSEventType.GQGSV;
                //e.Sentence = strGSV;
                //_NewGPSFix(this, e);
            }
        }


		/// <summary>
		/// Private method for parsing the GPGSV NMEA sentence
		/// GPGSV is a bit different, since it if usually made from several NMEA sentences
		/// </summary>
		/// <param name="strGSV">GPGSV sentence</param>
		public void ParseBDGSV(string strGSV)
        {
            //log.Debug("ParseBDGSV");
            //fire the event if last GSV message.
            if (BDGSV.AddSentence(strGSV))
            {
                //GPSEventArgs e = new GPSEventArgs();
                //e.TypeOfEvent = GPSEventType.BDGSV;
                //e.Sentence = strGSV;
                //_NewGPSFix(this, e);
            }
        }

		/// <summary>
		/// Private method for parsing the GPGSA NMEA sentence
		/// </summary>
		/// <param name="strGSA">GPGSA sentence</param>
		public void ParseGSA(string strGSA)
        {
			//log.Debug("ParseGSA");
			if (DateTime.Now > GSATime.AddMilliseconds(500))
            {
                gsaNum = 0;
				GSATime = DateTime.Now;
			}
            else {
				GSATime= DateTime.Now;
				gsaNum++;
			}
            if (SysConfig.getInstance().SensorBrand.ToUpper().StartsWith("HEMISHPERE"))
            {
                GPGSA.addSequenceHemisphere(strGSA);
            }
            else {
                GPGSA.addSequence(strGSA);
            }
			
        }



		/// <summary>
		/// Private method for parsing the GPGGA NMEA sentence
		/// </summary>
		/// <param name="strGGA">GPGGA sentence</param>
		public void ParseGGA(string strGGA)
        {
            //log.Debug("ParseGGA");
            GPGGA = new GPGGA(strGGA);
            if (GPGGA.Latitude != 0 && GPGGA.Longitude != 0 && GPGGA.Altitude != 0) {
                GPGGABffer.Add(GPGGA);
                if (GPGGABffer.Count > 600) {
                    GPGGABffer.RemoveAt(0);
                }
            }
            //fire the event.
            //GPSEventArgs e = new GPSEventArgs();
            //e.TypeOfEvent = GPSEventType.GPGGA;
            //e.Sentence = strGGA;
            //_NewGPSFix(this, e);
        }

		/// <summary>
		/// Private method for parsing the GPRMC NMEA sentence
		/// </summary>
		/// <param name="strRMC">GPRMC sentence</param>
		public void ParseRMC(string strRMC)
        {
            //log.Debug("ParseRMC");
            GPRMC = new GPRMC(strRMC);

            //fire the event.
            GPSEventArgs e = new GPSEventArgs();
            e.TypeOfEvent = GPSEventType.GPRMC;
            e.Sentence = strRMC;
            //_NewGPSFix(this, e);
        }

		/// <summary>
		/// Private method for parsing the PGRME NMEA sentence
		/// </summary>
		/// <param name="strRME">GPRMC sentence</param>
		public void ParseRME(string strRME)
        {
            //log.Debug("ParseRME");
            PGRME = new GPRME(strRME);
            //fire the event.
            //GPSEventArgs e = new GPSEventArgs();
            //e.TypeOfEvent = GPSEventType.PGRME;
            //e.Sentence = strRME;
            //_NewGPSFix(this, e);
        }

        /// <summary>
        /// Fires a NewGPSFix event
        /// </summary>
        /// <param name="type">Type of GPS event (GPGGA, GPGSA, etx...)</param>
        /// <param name="sentence">NMEA Sentence</param>
        private void FireEvent(GPSEventType type, string sentence)
        {
            GPSEventArgs e = new GPSEventArgs();
            e.TypeOfEvent = type;
            e.Sentence = sentence;
            GPSDataEventHandler(this, e);
        }

        /// <summary>
        /// Extracts a full NMEA string from the data recieved on the serialport, and parses this.
        /// The remaining and unparsed NMEA string is returned.
        /// </summary>
        /// <param name="strNMEA">NMEA ASCII data</param>
        /// <returns>Unparsed NMEA data</returns>
        public string ReadNmeaString(string strNMEA)
        {
            strNMEA = strNMEA.Replace("\n", "").Replace("\r", ""); //Remove linefeeds

            int nStart = strNMEA.IndexOf("$"); //Position of first NMEA data
            if (nStart < 0 || nStart == strNMEA.Length - 2)
                return strNMEA;
            //This will never pass the last NMEA sentence, before the next one arrives
            //The following should instead stop at the end of the line. 
            int nStop = strNMEA.IndexOf("$", nStart + 1); //Position of next NMEA sentence
            //log.Debug($"ReadNmeaString 2 nStop {nStop}");
            if (nStop > -1)
            {
                //log.Debug("ReadNmeaString 22");
                string strData;
                strData = strNMEA.Substring(nStart, nStop - nStart).Trim();

                //log.Debug($"ReadNmeaString 22{strData}");
                if (strData.StartsWith("$"))
                {
                    //log.Debug("ReadNmeaString 3");
                    if (CheckSentence(strData))
                    {
                        //log.Debug("ReadNmeaString 4");
                        FireEvent(String2Eventtype(strData), strData);
                    }
                }
                return strNMEA.Substring(nStop);
            }
            else
            {
                return strNMEA;
            }

        }

        /// <summary>
        /// 读取NMEA
        /// </summary>
        /// <param name="strData"></param>
        public void ParseNmeaString(string strData)
        {
            if (strData.StartsWith("$"))
            {
                if (CheckSentence(strData) || strData.StartsWith("$GNGSA") || strData.StartsWith("$GPGSA"))
                {
                    FireEvent(String2Eventtype(strData), strData);
                }
                else {
                    log.Debug("CheckSentence " + strData);
                }
            }
        }


        /// <summary>
        /// Checks the checksum of a NMEA sentence
        /// </summary>
        /// <remarks>
        /// The optional checksum field consists of a "*" and two hex digits 
        /// representing the exclusive OR of all characters between, but not
        /// including, the "$" and "*".  A checksum is required on some
        /// sentences.
        /// </remarks>
        /// <param name="strSentence">NMEA Sentence</param>
        /// <returns>'true' of checksum is correct</returns>
        private bool CheckSentence(string strSentence)
        {
            int iStart = strSentence.IndexOf('$');
            int iEnd = strSentence.IndexOf('*');
            //If start/stop isn't found it probably doesn't contain a checksum,
            //or there is no checksum after *. In such cases just return true.
            if (iStart >= iEnd || iEnd + 3 > strSentence.Length)
                return true;
            byte result = 0;
            for (int i = iStart + 1; i < iEnd; i++)
                result ^= (byte)strSentence[i];
            return result.ToString("X") == strSentence.Substring(iEnd + 1, 2);
        }

        /// <summary>
        /// Analyzes a NMEA sentence and returns the corresponding NMEA sentence type
        /// </summary>
        /// <param name="strData">NMEA Sentence</param>
        /// <returns>Sentence type</returns>
        internal static GPSEventType String2Eventtype(string strData)
        {
            if (strData.StartsWith("$GPGGA"))
                return GPSEventType.GPGGA;
            if (strData.StartsWith("$GNGGA"))
                return GPSEventType.GPGGA;
            else if (strData.StartsWith("$GPGLL"))
                return GPSEventType.GPGLL;
            else if (strData.StartsWith("$GNGLL"))
                return GPSEventType.GPGLL;
            else if (strData.StartsWith("$GPGSA")) {
                return GPSEventType.GPGSA;
            }
            else if (strData.StartsWith("$GNGSA")) {
                return GPSEventType.GPGSA;
            }
            else if (strData.StartsWith("$GPGSV" ))
                return GPSEventType.GPGSV;
            else if (strData.StartsWith("$BDGSV"))
                return GPSEventType.BDGSV;
            else if (strData.StartsWith("$GPZDA" ))
                return GPSEventType.GPZDA;
            else if (strData.StartsWith("$GNZDA" ))
                return GPSEventType.GNZDA;
            else if (strData.StartsWith("$GAGSV"))
                return GPSEventType.GAGSV;
            else if (strData.StartsWith("$GPGST"))
                return GPSEventType.GPGST;
            else if (strData.StartsWith("$GNGST"))
                return GPSEventType.GPGST;
            else if (strData.StartsWith("$GBGSV"))
                return GPSEventType.BDGSV;
            else if (strData.StartsWith("$GLGSV"))
                return GPSEventType.GLGSV;
            else if (strData.StartsWith("$GQGSV" ))
                return GPSEventType.GQGSV;
            else if (strData.StartsWith("$GPRMC"))
                return GPSEventType.GPRMC;
            else if (strData.StartsWith("$GNRMC" ))
                return GPSEventType.GPRMC;
            else if (strData.StartsWith("$PGRME" ))
                return GPSEventType.PGRME;
            else if (strData.StartsWith("$GPVTG" ))
                return GPSEventType.GPVTG;
            else if (strData.StartsWith("$GNVTG" ))
                return GPSEventType.GPVTG;
            else {
                return GPSEventType.Unknown;
            }
             
        }


        /// <summary>
        /// Converts GPS position in d"dd.ddd' to decimal degrees ddd.ddddd
        /// </summary>
        /// <param name="DM"></param>
        /// <param name="Dir"></param>
        /// <returns></returns>
        public static double GPSToDecimalDegrees(string DM, string Dir)
        {
            try
            {
                if (DM == "" || Dir == "")
                {
                    return 0.0;
                }
                //Get the fractional part of minutes
                //DM = '5512.45',  Dir='N'
                //DM = '12311.12', Dir='E'

                string t = DM.Substring(DM.IndexOf("."));
                double FM = double.Parse(DM.Substring(DM.IndexOf(".")), NumberFormatEnUs);

                //Get the minutes.
                t = DM.Substring(DM.IndexOf(".") - 2, 2);
                double Min = double.Parse(DM.Substring(DM.IndexOf(".") - 2, 2), NumberFormatEnUs);

                //Degrees
                t = DM.Substring(0, DM.IndexOf(".") - 2);
                double Deg = double.Parse(DM.Substring(0, DM.IndexOf(".") - 2), NumberFormatEnUs);

                if (Dir == "S" || Dir == "W")
                    Deg = -(Deg + (Min + FM) / 60);
                else
                    Deg = Deg + (Min + FM) / 60;
                return Deg;
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Internal class for parsing doubles. This replaces the Double.TryParse() method that isn't supported by CF.
        /// </summary>
        /// <remarks>Uses EN-us format for parsing doubles</remarks>
        /// <param name="str">string to parse</param>
        /// <param name="result">Output result. 0 if parse failed</param>
        /// <returns>true if parse was succesfull</returns>
        public static bool dblTryParse(string str, out double result)
        {
            try
            {
                result = double.Parse(str, NumberFormatEnUs);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        public static bool intTryParse(string str, out int result)
        {
            try
            {
                result = int.Parse(str, NumberFormatEnUs);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }
        public static int intTryParse(string str)
        {
            try { return int.Parse(str, NumberFormatEnUs); }
            catch { return 0; }
        }

    }
}
