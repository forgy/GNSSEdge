using System.Globalization;

namespace NtripShare.GNSS.Device.NMEA
{
	public class GPZDA
	{
		public GPZDA() { 
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Zda"/> class.
		/// </summary>
		/// <param name="type">The message type</param>
		/// <param name="message">The NMEA message values.</param>
		public GPZDA( string NMEAsentence) 
		{
			try
			{
				string[] message = NMEAsentence.Split(new Char[] { ',' });
				var time = StringToTimeSpan(message[0+1]);
				if (int.TryParse(message[1 + 1], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out int day) &&
					int.TryParse(message[2 + 1], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out int month) &&
					int.TryParse(message[3 + 1], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out int year))
				{
					FixDateTime = new DateTimeOffset(year, month, day, time.Hours, time.Minutes,
						time.Seconds, TimeSpan.Zero);
				}
			}
			catch { }

		}

		internal static TimeSpan StringToTimeSpan(string value)
		{
			if (value != null && value.Length >= 6)
			{
				return new TimeSpan(int.Parse(value.Substring(0, 2), CultureInfo.InvariantCulture),
								   int.Parse(value.Substring(2, 2), CultureInfo.InvariantCulture), 0)
								   .Add(TimeSpan.FromSeconds(double.Parse(value.Substring(4), CultureInfo.InvariantCulture)));
			}
			return TimeSpan.Zero;
		}
		/// <summary>
		/// Gets the time of fix
		/// </summary>
		public DateTimeOffset FixDateTime { get; }

		public String UTC { get; }


	}
}
