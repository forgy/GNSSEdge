using System.Globalization;

namespace NtripShare.GNSS.Device.NMEA
{

    public class GPVTG 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GPVTG"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public GPVTG( string NMEAsentence) 
        {
            try {
				string[] message = NMEAsentence.Split(new Char[] { ',' });
				CourseTrue = StringToDouble(message[0+1]);
				CourseMagnetic = StringToDouble(message[2 + 1]);
				SpeedKnots = StringToDouble(message[4 + 1]);
				SpeedKph = StringToDouble(message[6 + 1]);
			} catch { }
        }

		internal static double StringToDouble(string value)
		{
			if (value != null && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
			{
				return result;
			}
			return double.NaN;
		}

		/// <summary>
		///  Course over ground relative to true north
		/// </summary>
		public double CourseTrue { get; }

        /// <summary>
        ///  Course over ground relative to magnetic north
        /// </summary>
        public double CourseMagnetic { get; }

        /// <summary>
        /// Speed over ground in knots
        /// </summary>
        public double SpeedKnots { get; }

        /// <summary>
        /// Speed over ground in kilometers/hour
        /// </summary>
        public double SpeedKph { get; }
    }
}