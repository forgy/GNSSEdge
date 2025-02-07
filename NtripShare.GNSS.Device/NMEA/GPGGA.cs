using System.Globalization;

namespace NtripShare.GNSS.Device.NMEA
{
	/// <summary>
	/// Global Positioning System Fix Data
	/// </summary>
	public class GPGGA
	{
		public GPGGA() { 
		}
		/// <summary>
		/// Initializes the NMEA Global Positioning System Fix Data and parses an NMEA sentence
		/// </summary>
		/// <param name="NMEAsentence"></param>
		public GPGGA(string NMEAsentence)
		{
			try
			{
				if (NMEAsentence.IndexOf('*') > 0)
					NMEAsentence = NMEAsentence.Substring(0, NMEAsentence.IndexOf('*'));
				//Split into an array of strings.
				string[] split = NMEAsentence.Split(new Char[] { ',' });
				if (split[1].Length >= 6)
				{
					FixTime = NmeaMessage.StringToTimeSpan(split[0 + 1]);
				}
				
				Latitude = NmeaMessage.StringToLatitude(split[1 + 1], split[2 + 1]);
				Longitude = NmeaMessage.StringToLongitude(split[3 + 1], split[4 + 1]);
				Quality = (GPGGA.FixQuality)int.Parse(split[5 + 1], CultureInfo.InvariantCulture);
				NumberOfSatellites = int.Parse(split[6 + 1], CultureInfo.InvariantCulture);
				Hdop = NmeaMessage.StringToDouble(split[7 + 1]);
				Altitude = NmeaMessage.StringToDouble(split[8 + 1]);
				AltitudeUnits = split[9 + 1];
				GeoidalSeparation = NmeaMessage.StringToDouble(split[10 + 1]);
				HeightOfGeoidUnits = split[11 + 1];
				var timeInSeconds = NmeaMessage.StringToDouble(split[12 + 1]);
				if (!double.IsNaN(timeInSeconds))
					TimeSinceLastDgpsUpdate = TimeSpan.FromSeconds(timeInSeconds);
				else
					TimeSinceLastDgpsUpdate = TimeSpan.FromSeconds(0);
                if (split[13 + 1].Length > 0)
					DgpsStationId = int.Parse(split[13 + 1], CultureInfo.InvariantCulture);
				else
					DgpsStationId = -1;
			}
			catch { 
			}
		}

		/// <summary>
		/// Time of day fix was taken
		/// </summary>
		public TimeSpan FixTime { get; }

		/// <summary>
		/// Latitude
		/// </summary>
		public double Latitude { get; }

		/// <summary>
		/// Longitude
		/// </summary>
		public double Longitude { get; }

		/// <summary>
		/// Fix Quality
		/// </summary>
		public FixQuality Quality { get; }

		/// <summary>
		/// Number of satellites being tracked
		/// </summary>
		public int NumberOfSatellites { get; }

		/// <summary>
		/// Horizontal Dilution of Precision
		/// </summary>
		public double Hdop { get; }

		/// <summary>
		/// Altitude
		/// </summary>
		public double Altitude { get; }

		/// <summary>
		/// Altitude units ('M' for Meters)
		/// </summary>
		public string AltitudeUnits { get; }

		/// <summary>
		/// Geoidal separation: the difference between the WGS-84 earth ellipsoid surface and mean-sea-level (geoid) surface.
		/// </summary>
		/// <remarks>
		/// A negative value means mean-sea-level surface is below the WGS-84 ellipsoid surface.
		/// </remarks>
		public double GeoidalSeparation { get; }

		/// <summary>
		/// Altitude units ('M' for Meters)
		/// </summary>
		public string HeightOfGeoidUnits { get; }

		/// <summary>
		/// Time since last DGPS update (ie age of the differential GPS data)
		/// </summary>
		public TimeSpan TimeSinceLastDgpsUpdate { get; }

		/// <summary>
		/// Differential Reference Station ID
		/// </summary>
		public int DgpsStationId { get; }

		/// <summary>
		/// Fix quality indicater
		/// </summary>
		public enum FixQuality : int
		{
			/// <summary>Fix not available or invalid</summary>
			Invalid = 0,
			/// <summary>GPS SPS Mode, fix valid</summary>
			Single = 1,
			/// <summary>Differential GPS, SPS Mode, or Satellite Based Augmentation System (SBAS), fix valid</summary>
			DGPS = 2,
			/// <summary>GPS PPS (Precise Positioning Service) mode, fix valid</summary>
			PPS = 3,
			/// <summary>Real Time Kinematic (Fixed). System used in RTK mode with fixed integers</summary>
			Rtk = 4,
			/// <summary>Real Time Kinematic (Floating). Satellite system used in RTK mode, floating integers</summary>
			Float = 5,
			/// <summary>Estimated (dead reckoning) mode</summary>
			Estimated = 6,
			/// <summary>Manual input mode</summary>
			ManualInput = 7,
			/// <summary>Simulator mode</summary>
			Simulation = 8
		}
	}
}