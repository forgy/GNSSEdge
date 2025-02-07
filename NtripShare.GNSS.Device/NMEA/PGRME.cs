namespace NtripShare.GNSS.Device.NMEA
{
    /// <summary>
    /// Estimated Position Error
    /// The following are Garmin proprietary sentences.  "P" denotes
    /// proprietary, "GRM" is Garmin's manufacturer code.
    /// </summary>
    public class GPRME
	{
		/// <summary>
		/// Initializes the NMEA Estimated Position Error
		/// </summary>
		public GPRME()
		{
		}

		/// <summary>
		/// Initializes the NMEA Estimated Position Error and parses an NMEA sentence
		/// </summary>
		/// <param name="NMEAsentence"></param>
		public GPRME(string NMEAsentence)
		{
			try
			{
				//Split into an array of strings.
				string[] split = NMEAsentence.Split(new Char[] { ',' });
                NMEAHelper.dblTryParse(split[1], out _estHorisontalError);
                NMEAHelper.dblTryParse(split[3], out _estVerticalError);
                NMEAHelper.dblTryParse(split[5], out _estSphericalError);
			}
			catch { }
		}

		#region Properties

		private double _estHorisontalError;
		private double _estVerticalError;
		private double _estSphericalError;

		/// <summary>
		/// Estimated horizontal position error in metres (HPE)
		/// </summary>
		public double EstHorisontalError
		{
			get { return _estHorisontalError; }
			//set { _estHorisontalError = value; }
		}

		/// <summary>
		/// Estimated vertical error (VPE) in metres
		/// </summary>
		public double EstVerticalError
		{
			get { return _estVerticalError; }
			//set { _estVerticalError = value; }
		}

		/// <summary>
		/// Overall spherical equivalent position error
		/// </summary>
		public double EstSphericalError
		{
			get { return _estSphericalError; }
			//set { _estSphericalError = value; }
		}
		#endregion
	}
}