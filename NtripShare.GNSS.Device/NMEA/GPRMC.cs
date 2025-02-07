using NtripShare.GNSS.Device.NTRIP;

namespace NtripShare.GNSS.Device.NMEA
{
    /// <summary>
    /// Recommended minimum specific GPS/Transit data
    /// </summary>
    public class GPRMC
	{
		/// <summary>
		/// Enum for the Receiver Status information.
		/// </summary>
		public enum StatusEnum
		{
			/// <summary>
			/// Fix warning
			/// </summary>
			Warning,
			/// <summary>
			/// Fix OK
			/// </summary>
			OK,
			/// <summary>
			/// Bad fix
			/// </summary>
			BadFix,
			/// <summary>
			/// GPS fix
			/// </summary>
			GPS,
			/// <summary>
			/// Differential GPS fix
			/// </summary>
			DGPS
		}

		/// <summary>
		/// Initializes the NMEA Recommended minimum specific GPS/Transit data
		/// </summary>
		public GPRMC()
		{
			_position = new Coordinate();
		}

		/// <summary>
		/// Initializes the NMEA Recommended minimum specific GPS/Transit data and parses an NMEA sentence
		/// </summary>
		/// <param name="NMEAsentence"></param>
		public GPRMC(string NMEAsentence)
		{
			try
			{
				//Split into an array of strings.
				string[] split = NMEAsentence.Split(new Char[] { ',' });

				//Extract date/time
				try
				{
					string[] DateTimeFormats = { "ddMMyyHHmmss", "ddMMyy", "ddMMyyHHmmss.FFFFFF" };
					if (split[9].Length >= 6) { //Require at least the date to be present 
						string time = split[9] + split[1]; // +" 0";
						_timeOfFix = DateTime.ParseExact(time, DateTimeFormats, NMEAHelper.NumberFormatEnUs, System.Globalization.DateTimeStyles.AssumeUniversal);
					}
					else
						_timeOfFix = new DateTime();
				}
				catch { _timeOfFix = new DateTime(); }

				if (split[2] == "A")
					_status = StatusEnum.OK;
				else
					_status = StatusEnum.Warning;

				_position = new Coordinate(NMEAHelper.GPSToDecimalDegrees(split[5], split[6]),
											NMEAHelper.GPSToDecimalDegrees(split[3], split[4]));

				NMEAHelper.dblTryParse(split[7], out _speed);
				NMEAHelper.dblTryParse(split[8], out _course);
				NMEAHelper.dblTryParse(split[10], out _magneticVariation);
			}
			catch { }
		}

		#region Properties

		private Coordinate _position;
		private StatusEnum _status;
		private DateTime _timeOfFix;
		private double _speed;
		private double _course;
		private double _magneticVariation;
		//private string _magneticVariationDirection;

		/// <summary>
		/// Indicates the current status of the GPS receiver.
		/// </summary>
		public StatusEnum Status
		{
			get { return _status; }
			//set { _status = value; }
		}

		/// <summary>
		/// Coordinate of recieved position
		/// </summary>
		public Coordinate Position
		{
			get { return _position; }
			//set { _position = value; }
		}
		
		/// <summary>
		/// Groundspeed in knots.
		/// </summary>
		public double Speed
		{
			get { return _speed; }
			//set { _speed = value; }
		}
	
		/// <summary>
		/// Course (true, not magnetic) in decimal degrees.
		/// </summary>
		public double Course
		{
			get { return _course; }
			//set { _course = value; }
		}

		/// <summary>
		/// MagneticVariation in decimal degrees.
		/// </summary>
		public double MagneticVariation
		{
			get { return _magneticVariation; }
			//set { _magneticVariation = value; }
		}
	
		///// <summary>
		///// The direction of the magnetic variation.
		///// </summary>
		//public string MagneticVariationDirection
		//{
		//    get { return _magneticVariationDirection; }
		//    //set { _magneticVariationDirections = value; }
		//}

		/// <summary>
		/// Date and Time of fix - Greenwich mean time.
		/// </summary>
		public DateTime TimeOfFix
		{
			get { return _timeOfFix; }
			//set { _timeOfFix = value; }
		}
#endregion
	}
}
