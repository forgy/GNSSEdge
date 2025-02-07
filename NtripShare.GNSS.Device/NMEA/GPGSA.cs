using log4net;
using System.Globalization;

namespace NtripShare.GNSS.Device.NMEA
{
    /// <summary>
    /// GPS DOP and active satellites
    /// </summary>
    public class GPGSA
	{
		private static ILog log = LogManager.GetLogger("NtripShare", typeof(GPGSA));

		///// <summary>
		///// Initializes the NMEA GPS DOP and active satellites
		///// </summary>
		public GPGSA()
		{
			//_pRNInSolution = new List<string>();
		}

        public void addSequenceHemisphere(string NMEAsentence)
        {
            //_pRNInSolution = new List<string>(); 
            try
            {
                if (updateTime.AddSeconds(10) < updateTime)
                {
                    GPSSatelliteIDs.Clear();
                    GLOSatelliteIDs.Clear();
                    BDSSatelliteIDs.Clear();
                    QZSSSatelliteIDs.Clear();
                    GALSatelliteIDs.Clear();
                    updateTime = DateTime.Now;
                }
                if (NMEAsentence.IndexOf('*') > 0)
                {
                    NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
                }

                //Split into an array of strings.
                string[] message = NMEAsentence.Split(new Char[] { ',' });
                int num = int.Parse(message[message.Length - 1]);
                Mode = message[0 + 1] == "A" ? GPGSA.ModeSelection.Auto : GPGSA.ModeSelection.Manual;
                Fix = (GPGSA.FixType)int.Parse(message[1 + 1], CultureInfo.InvariantCulture);

                if (num == 1)
                {
                    for (int i = 2; i < 14 + 1; i++)
                    {
                        int id = -1;
                        if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
                        {
                            if (!GPSSatelliteIDs.Contains(id.ToString()))
                            {
                                GPSSatelliteIDs.Add(id.ToString());
                            }
                        }
                    }
                    //log.Info($"GPSSatelliteIDs {GPSSatelliteIDs.Count}");
                }
                if (num == 2)
                {
                    for (int i = 2; i < 14 + 1; i++)
                    {
                        int id = -1;
                        if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
                        {
                            if (!GLOSatelliteIDs.Contains(id.ToString()))
                            {
                                GLOSatelliteIDs.Add(id.ToString());
                            }
                        }

                    }
                    //log.Info($"GLOSatelliteIDs {GLOSatelliteIDs.Count}");
                }

                if (num == 3)
                {
                    for (int i = 2; i < 14 + 1; i++)
                    {
                        int id = -1;
                        if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
                        {
                            if (!GALSatelliteIDs.Contains(id.ToString()))
                            {
                                GALSatelliteIDs.Add(id.ToString());
                            }
                        }
                    }
                    //log.Info($"GALSatelliteIDs {GALSatelliteIDs.Count}");
                }

                if (num == 5)
                {
                    for (int i = 2; i < 14 + 1; i++)
                    {
                        int id = -1;
                        if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
                        {
                            if (!BDSSatelliteIDs.Contains(id.ToString()))
                            {
                                BDSSatelliteIDs.Add(id.ToString());
                            }
                        }
                    }
                    //log.Info($"BDSSatelliteIDs {BDSSatelliteIDs.Count}");
                }

                if (num == 4)
                {
                    for (int i = 2; i < 14 + 1; i++)
                    {
                        int id = -1;
                        if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
                        {
                            if (!QZSSSatelliteIDs.Contains(id.ToString()))
                            {
                                QZSSSatelliteIDs.Add(id.ToString());
                            }
                        }
                    }
                    //log.Info($"QZSSSatelliteIDs {QZSSSatelliteIDs.Count}");
                }


                double tmp;
                if (double.TryParse(message[14 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                    Pdop = tmp;
                else
                    Pdop = double.NaN;

                if (double.TryParse(message[15 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                    Hdop = tmp;
                else
                    Hdop = double.NaN;

                if (double.TryParse(message[16 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
                    Vdop = tmp;
                else
                    Vdop = double.NaN;
                if (NMEAsentence.IndexOf('*') > 0)
                {
                    NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
                }
            }
            catch
            {

            }
        }

        public void addSequence(string NMEAsentence)
		{
			try
			{
                if (updateTime.AddSeconds(300) < updateTime)
                {
                    GPSSatelliteIDs.Clear();
                    GLOSatelliteIDs.Clear();
                    BDSSatelliteIDs.Clear();
                    QZSSSatelliteIDs.Clear();
                    GALSatelliteIDs.Clear();
                    updateTime = DateTime.Now;
                }
                if (NMEAsentence.IndexOf('*') > 0)
				{
					NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
				}
				
				string[] message = NMEAsentence.Split(',' );
				int num = int.Parse(message[message.Length-1]);
				Mode = message[0 + 1] == "A" ? GPGSA.ModeSelection.Auto : GPGSA.ModeSelection.Manual;
				Fix = (GPGSA.FixType)int.Parse(message[1 + 1], CultureInfo.InvariantCulture);

				if (num == 1)
				{
					for (int i = 2 ; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
                            if (!GPSSatelliteIDs.Contains(id.ToString()))
                            {
                                GPSSatelliteIDs.Add(id.ToString());
                            }
                        }
					}
				}
				if (num == 2)
				{
					for (int i = 2; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
                            if (!GLOSatelliteIDs.Contains(id.ToString()))
                            {
                                GLOSatelliteIDs.Add(id.ToString());
                            }
                        }

					}
				}

				if (num == 3)
				{
					for (int i = 2; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
                            if (!GALSatelliteIDs.Contains(id.ToString()))
                            {
                                GALSatelliteIDs.Add(id.ToString());
                            }
                        }
					}
				}

				if (num == 4)
				{
					for (int i = 2; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
                            if (!BDSSatelliteIDs.Contains(id.ToString()))
                            {
                                BDSSatelliteIDs.Add(id.ToString());
                            }
                        }
					}
				}

				if (num == 5)
				{
					for (int i = 2 ; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
                            if (!QZSSSatelliteIDs.Contains(id.ToString()))
                            {
                                QZSSSatelliteIDs.Add(id.ToString());
                            }
                        }
					}
				}

				double tmp;
				if (double.TryParse(message[14 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
					Pdop = tmp;
				else
					Pdop = double.NaN;

				if (double.TryParse(message[15 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
					Hdop = tmp;
				else
					Hdop = double.NaN;

				if (double.TryParse(message[16 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
					Vdop = tmp;
				else
					Vdop = double.NaN;
				if (NMEAsentence.IndexOf('*') > 0)
				{
					NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
				}
			}
			catch
			{

			}
		}

		/// <summary>
		/// Ìí¼ÓÌõÄ¿
		/// </summary>
		/// <param name="NMEAsentence"></param>
		/// <param name="num"></param>
		//public void addSequence(string NMEAsentence,int num)
		//{
		//	if (updateTime.AddSeconds(10) < updateTime) {
		//		GPSSatelliteIDs.Clear();
		//		GLOSatelliteIDs.Clear();
		//		BDSSatelliteIDs.Clear();
  //              QZSSSatelliteIDs.Clear();
		//		GALSatelliteIDs.Clear();
		//		updateTime = DateTime.Now;
  //          }
		//	try
		//	{
		//		if (NMEAsentence.IndexOf('*') > 0) {
		//			NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
		//		}
					
		//		string[] message = NMEAsentence.Split(',' );
		//		Mode = message[0 + 1] == "A" ? GPGSA.ModeSelection.Auto : GPGSA.ModeSelection.Manual;
		//		Fix = (GPGSA.FixType)int.Parse(message[1 + 1], CultureInfo.InvariantCulture);

		//		if (num == 1)
		//		{
		//			for (int i = 2 + 1; i < 14 + 1; i++)
		//			{
		//				int id = -1;
		//				if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
		//				{
		//					if (!GPSSatelliteIDs.Contains(id.ToString()))
		//					{
		//						GPSSatelliteIDs.Add(id.ToString());
  //                          }
  //                      }
		//			}
		//			//log.Info($"GPSSatelliteIDs {GPSSatelliteIDs.Count}");
		//		}
		//		if (num == 2)
		//		{
		//			for (int i = 2 + 1; i < 14 + 1; i++)
		//			{
		//				int id = -1;
		//				if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
		//				{
  //                          if (!GLOSatelliteIDs.Contains(id.ToString()))
  //                          {
  //                              GLOSatelliteIDs.Add(id.ToString());
  //                          }
  //                      }
		//			}
		//			//log.Info($"GLOSatelliteIDs {GLOSatelliteIDs.Count}");
		//		}
			
		//		if (num == 3)
		//		{
		//			for (int i = 2 + 1; i < 14 + 1; i++)
		//			{
		//				int id = -1;
		//				if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
		//				{
  //                          if (!GALSatelliteIDs.Contains(id.ToString()))
  //                          {
  //                              GALSatelliteIDs.Add(id.ToString());
  //                          }
  //                      }
		//			}
		//			//log.Info($"GALSatelliteIDs {GALSatelliteIDs.Count}");
		//		}
				
		//		if (num == 4)
		//		{
		//			for (int i = 2 + 1; i < 14 + 1; i++)
		//			{
		//				int id = -1;
		//				if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
		//				{
  //                          if (!BDSSatelliteIDs.Contains(id.ToString()))
  //                          {
  //                              BDSSatelliteIDs.Add(id.ToString());
  //                          }
  //                      }
		//			}
		//			//log.Info($"BDSSatelliteIDs {BDSSatelliteIDs.Count}");
		//		}

		//		if (num == 5)
		//		{
		//			for (int i = 2 + 1; i < 14 + 1; i++)
		//			{
		//				int id = -1;
		//				if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
		//				{
  //                          if (!QZSSSatelliteIDs.Contains(id.ToString()))
  //                          {
  //                              QZSSSatelliteIDs.Add(id.ToString());
  //                          }
  //                      }
		//			}
		//			//log.Info($"QZSSSatelliteIDs {QZSSSatelliteIDs.Count}");
		//		}
				
		//		//SatelliteIDs = svs.ToArray();

		//		double tmp;
		//		if (double.TryParse(message[14 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
		//			Pdop = tmp;
		//		else
		//			Pdop = double.NaN;

		//		if (double.TryParse(message[15 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
		//			Hdop = tmp;
		//		else
		//			Hdop = double.NaN;

		//		if (double.TryParse(message[16 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
		//			Vdop = tmp;
		//		else
		//			Vdop = double.NaN;
		//		if (NMEAsentence.IndexOf('*') > 0)
		//		{
		//			NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
		//		}
		//	}
		//	catch {
			
		//	}
		//}

		/// <summary>
		///  GPS DOP and active satellites and parses an NMEA sentence
		/// </summary>
		/// <param name="NMEAsentence"></param>
		public GPGSA(string NMEAsentence)
		{
			//_pRNInSolution = new List<string>(); 
			try
			{
				if (NMEAsentence.IndexOf('*') > 0)
					NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
				//Split into an array of strings.
				string[] message = NMEAsentence.Split(new Char[] { ',' });
				Mode = message[0+1] == "A" ? GPGSA.ModeSelection.Auto : GPGSA.ModeSelection.Manual;
				Fix = (GPGSA.FixType)int.Parse(message[1 + 1], CultureInfo.InvariantCulture);

				List<int> svs = new List<int>();

                List<string> gps = new List<string>();
                List<string> bds = new List<string>();
                List<string> glo = new List<string>();
                List<string> gal = new List<string>();
                List<string> qzss = new List<string>();
                for (int i = 2 + 1; i < 14 + 1; i++)
				{
					int id = -1;
					if (message[i].Length > 0 && int.TryParse(message[i + 1], out id)) {
                        svs.Add(id);
                    }
					if (id >= 1 && id <= 32) {
						gps.Add(id.ToString());
                    }
                    if (id >= 38 && id <= 61)
                    {
                        glo.Add(id.ToString());
                    }
                    if (id >= 71 && id <= 106)
                    {
                        gal.Add(id.ToString());
                    }
                    if (id >= 141 && id <= 203)
                    {
                        bds.Add(id.ToString());
                    }
                    if (id >= 131 && id <= 140)
                    {
                        qzss.Add(id.ToString());
                    }
                }
				GPSSatelliteIDs = gps;
				BDSSatelliteIDs = bds;
				GALSatelliteIDs = gal;
				GLOSatelliteIDs = glo;
				QZSSSatelliteIDs = qzss;
				//SatelliteIDs = svs.ToArray();

				double tmp;
				if (double.TryParse(message[14 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
					Pdop = tmp;
				else
					Pdop = double.NaN;

				if (double.TryParse(message[15 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
					Hdop = tmp;
				else
					Hdop = double.NaN;

				if (double.TryParse(message[16 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
					Vdop = tmp;
				else
					Vdop = double.NaN;
				if (NMEAsentence.IndexOf('*') > 0) {
                    NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
                }
			}
			catch { }
		}

		/// <summary>
		///  GPS DOP and active satellites and parses an NMEA sentence
		/// </summary>
		/// <param name="NMEAsentence"></param>
		public GPGSA(List<string> NMEAsentences)
		{
			//_pRNInSolution = new List<string>(); 
			try
			{
				string NMEAsentence = NMEAsentences[0];
				if (NMEAsentence.IndexOf('*') > 0) {
					NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
				}
					
				//Split into an array of strings.
				string[] message = NMEAsentence.Split(new Char[] { ',' });
				Mode = message[0 + 1] == "A" ? GPGSA.ModeSelection.Auto : GPGSA.ModeSelection.Manual;
				Fix = (GPGSA.FixType)int.Parse(message[1 + 1], CultureInfo.InvariantCulture);

				List<int> svs = new List<int>();
				List<string> gps = new List<string>();

				List<string> bds = new List<string>();
				List<string> glo = new List<string>();
				List<string> gal = new List<string>();
				List<string> qzss = new List<string>();
				for (int i = 2 + 1; i < 14 + 1; i++)
				{
					int id = -1;
					if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
					{
						svs.Add(id);
					}
					if (id >= 1 && id <= 32)
					{
						gps.Add(id.ToString());
					}
				}
				GPSSatelliteIDs = gps;
				if (NMEAsentences.Count > 1) {
					NMEAsentence = NMEAsentences[1];
					message = NMEAsentence.Split(new Char[] { ',' });
					for (int i = 2 + 1; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
							svs.Add(id);
						}
						glo.Add(id.ToString());
					}
				}
				GLOSatelliteIDs = glo;
				if (NMEAsentences.Count > 2)
				{
					NMEAsentence = NMEAsentences[2];
					message = NMEAsentence.Split(new Char[] { ',' });
					for (int i = 2 + 1; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
							svs.Add(id);
						}
						gal.Add(id.ToString());
					}
				}
				GALSatelliteIDs = gal;

				if (NMEAsentences.Count > 3)
				{
					NMEAsentence = NMEAsentences[3];
					message = NMEAsentence.Split(new Char[] { ',' });
					for (int i = 2 + 1; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
							svs.Add(id);
						}
						bds.Add(id.ToString());
					}
				}

				BDSSatelliteIDs = bds;

				if (NMEAsentences.Count > 4)
				{
					NMEAsentence = NMEAsentences[4];
					message = NMEAsentence.Split(new Char[] { ',' });
					for (int i = 2 + 1; i < 14 + 1; i++)
					{
						int id = -1;
						if (message[i].Length > 0 && int.TryParse(message[i + 1], out id))
						{
							svs.Add(id);
						}
						qzss.Add(id.ToString());
					}
				}
				QZSSSatelliteIDs = qzss;

				//SatelliteIDs = svs.ToArray();

				double tmp;
				if (double.TryParse(message[14 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				{
					Pdop = tmp;
				}
				else {
					Pdop = double.NaN;
				}
				if (double.TryParse(message[15 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				{
					Hdop = tmp;
				}
				else {
					Hdop = double.NaN;
				}
				if (double.TryParse(message[16 + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out tmp))
				{
					Vdop = tmp;
				}
				else {
					Vdop = double.NaN;
				}
				if (NMEAsentence.IndexOf('*') > 0)
				{
					NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
				}
			}
			catch { }
		}


		#region Properties

		/// <summary>
		/// Mode
		/// </summary>
		public ModeSelection Mode { get; set; }

		/// <summary>
		/// Mode
		/// </summary>
		public FixType Fix { get; set; }

		/// <summary>
		/// ID numbers of satellite vehicles used in the solution.
		/// </summary>
		/// <remarks>
		/// - GPS satellites are identified by their PRN numbers, which range from 1 to 32.
		/// - The numbers 33-64 are reserved for SBAS satellites. The SBAS system PRN numbers are 120-138. The offset from NMEA SBAS SB ID to SBAS PRN number is 87.
		/// A SBAS PRN number of 120 minus 87 yields the SV ID of 33. The addition of87 to the SVID yields the SBAS PRN number.
		/// - The numbers 65-96 are reserved for GLONASS satellites. GLONASS satellites are identified by 64+satellite slot number.
		/// </remarks>
		//public int[] SatelliteIDs { get; set; }

		DateTime updateTime = DateTime.Now;

        public List<string> GPSSatelliteIDs { get; set; }=new List<string>();
		public List<string> BDSSatelliteIDs { get; set; } = new List<string>();
		public List<string> GLOSatelliteIDs { get; set; } = new List<string>();
		public List<string> GALSatelliteIDs { get; set; } = new List<string>();
		public List<string> QZSSSatelliteIDs { get; set; } = new List<string>();

		/// <summary>
		/// Dilution of precision
		/// </summary>
		public double Pdop { get; set; }

		/// <summary>
		/// Horizontal dilution of precision
		/// </summary>
		public double Hdop { get; set; }

		/// <summary>
		/// Vertical dilution of precision
		/// </summary>
		public double Vdop { get; set; }

		/// <summary>
		/// Mode selection
		/// </summary>
		public enum ModeSelection
		{
			/// <summary>
			/// Automatic, allowed to automatically switch 2D/3D
			/// </summary>
			Auto,
			/// <summary>
			/// Manual mode
			/// </summary>
			Manual,
		}

		/// <summary>
		/// Fix Mode
		/// </summary>
		public enum FixType : int
		{
			/// <summary>
			/// Not available
			/// </summary>
			NotAvailable = 1,
			/// <summary>
			/// 2D Fix
			/// </summary>
			Fix2D = 2,
			/// <summary>
			/// 3D Fix
			/// </summary>
			Fix3D = 3
		}
		#endregion
	}
}
