
using System.Globalization;

namespace NtripShare.GNSS.Device.NMEA
{
    /// <summary>
    /// Bearing - Origin to Destination
    /// </summary>
    /// <remarks>
    /// Bearing angle of the line, calculated at the origin waypoint, extending to the destination waypoint from 
    /// the origin waypoint for the active navigation leg of the journey
    /// </remarks>
    public class GPBOD 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bod"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public GPBOD(string NMEAsentence)
		{
			string[] message = NMEAsentence.Split(new Char[] { ',' });
			if (message[0+1].Length > 0)
                TrueBearing = double.Parse(message[0 + 1], CultureInfo.InvariantCulture);
            else
                TrueBearing = double.NaN;
            if (message[2 + 1].Length > 0)
                MagneticBearing = double.Parse(message[2 + 1], CultureInfo.InvariantCulture);
            else
                MagneticBearing = double.NaN;
            if (message.Length > 4 && !string.IsNullOrEmpty(message[4 + 1]))
                DestinationId = message[4 + 1];
            if (message.Length > 5 && !string.IsNullOrEmpty(message[5 + 1]))
                OriginId = message[5 + 1];
        }
        /// <summary>
        /// True Bearing in degrees from start to destination
        /// </summary>
        public double TrueBearing { get; }

        /// <summary>
        /// Magnetic Bearing in degrees from start to destination
        /// </summary>
        public double MagneticBearing { get; }

        /// <summary>
        /// Name of origin waypoint ID
        /// </summary>
        public string? OriginId { get; }

        /// <summary>
        /// Name of destination waypoint ID
        /// </summary>
        public string? DestinationId { get; }
    }
}