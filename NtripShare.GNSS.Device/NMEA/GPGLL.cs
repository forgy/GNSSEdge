
using NtripShare.GNSS.Device.NTRIP;

namespace NtripShare.GNSS.Device.NMEA
{
	/// <summary>
	/// Geographic position, Latitude and Longitude
	/// </summary>
	public class GPGLL
	{
		/// <summary>
		/// Initializes the NMEA Geographic position, Latitude and Longitude and parses an NMEA sentence
		/// </summary>
		/// <param name="NMEAsentence"></param>
		public GPGLL(string NMEAsentence)
		{
			try
			{
				//Split into an array of strings.
				string[] message = NMEAsentence.Split(new Char[] { ',' });

				Latitude = NmeaMessage.StringToLatitude(message[0 + 1], message[1 + 1]);
				Longitude = NmeaMessage.StringToLongitude(message[2 + 1], message[3 + 1]);
				if (message.Length >= 5) //Some older GPS doesn't broadcast fix time
				{
					FixTime = NmeaMessage.StringToTimeSpan(message[4 + 1]);
				}
				DataActive = (message.Length < 6 || message[5 + 1] == "A");
				ModeIndicator = DataActive ? Mode.Autonomous : Mode.DataNotValid;
				if (message.Length > 6)
				{
					switch (message[6 + 1])
					{
						case "A": ModeIndicator = Mode.Autonomous; break;
						case "D": ModeIndicator = Mode.DataNotValid; break;
						case "E": ModeIndicator = Mode.EstimatedDeadReckoning; break;
						case "M": ModeIndicator = Mode.Manual; break;
						case "S": ModeIndicator = Mode.Simulator; break;
						case "N": ModeIndicator = Mode.DataNotValid; break;
					}
				}
			}
			catch { }
		}

		#region Properties

		/// <summary>
		/// Latitude
		/// </summary>
		public double Latitude { get; }

		/// <summary>
		/// Longitude
		/// </summary>
		public double Longitude { get; }

		/// <summary>
		/// Time since last DGPS update
		/// </summary>
		public TimeSpan FixTime { get; }

		/// <summary>
		/// Gets a value indicating whether data is active.
		/// </summary>
		/// <value>
		///   <c>true</c> if data is active; otherwise, <c>false</c>.
		/// </value>
		public bool DataActive { get; }

		/// <summary>
		/// Positioning system Mode Indicator
		/// </summary>
		public Mode ModeIndicator { get; }

		/// <summary>
		/// Positioning system Mode Indicator
		/// </summary>
		/// <seealso cref="Gll.ModeIndicator"/>
		public enum Mode
		{
			/// <summary>
			/// Autonomous mode
			/// </summary>
			Autonomous,
			/// <summary>
			///  Differential mode
			/// </summary>
			Differential,
			/// <summary>
			///  Estimated (dead reckoning) mode
			/// </summary>
			EstimatedDeadReckoning,
			/// <summary>
			/// Manual input mode
			/// </summary>
			Manual,
			/// <summary>
			/// Simulator mode
			/// </summary>
			Simulator,
			/// <summary>
			/// Data not valid
			/// </summary>
			DataNotValid
		}
		#endregion
	}
}