﻿using System;
using System.Globalization;

namespace NtripShare.GNSS.Device.NMEA
{
    /// <summary>
    /// Recommended Minimum specific GNSS data
    /// </summary>
    /// <remarks>
    /// <para>Time, date, position, course and speed data provided by a GNSS navigation receiver. This sentence is
    /// transmitted at intervals not exceeding 2-seconds and is always accompanied by <see cref="Rmb"/> when a destination waypoint
    /// is active.</para>
    /// <para><see cref="Rmc"/> and <see cref="Rmb"/> are the recommended minimum data to be provided by a GNSS receiver.</para>
    /// </remarks>
    public class Rmc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rmc"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public Rmc(string type, string[] message)
        {
            if (message == null || message.Length < 11)
                throw new ArgumentException("Invalid GPRMC", "message"); 
            
            if (message[8].Length == 6 && message[0].Length >= 6)
            {
                FixTime = new DateTimeOffset(int.Parse(message[8].Substring(4, 2), CultureInfo.InvariantCulture) + 2000,
                                       int.Parse(message[8].Substring(2, 2), CultureInfo.InvariantCulture),
                                       int.Parse(message[8].Substring(0, 2), CultureInfo.InvariantCulture),
                                       int.Parse(message[0].Substring(0, 2), CultureInfo.InvariantCulture),
                                       int.Parse(message[0].Substring(2, 2), CultureInfo.InvariantCulture),
                                       0, TimeSpan.Zero).AddSeconds(double.Parse(message[0].Substring(4), CultureInfo.InvariantCulture));
            }
            Active = (message[1] == "A");
            Latitude = NmeaMessage.StringToLatitude(message[2], message[3]);
            Longitude = NmeaMessage.StringToLongitude(message[4], message[5]);
            Speed = NmeaMessage.StringToDouble(message[6]);
            Course = NmeaMessage.StringToDouble(message[7]);
            MagneticVariation = NmeaMessage.StringToDouble(message[9]);            
            if (!double.IsNaN(MagneticVariation) && message[10] == "W")
                MagneticVariation *= -1;
        }

        /// <summary>
        /// Fix Time
        /// </summary>
        public DateTimeOffset FixTime { get; }

        /// <summary>
        /// Gets a value whether the device is active
        /// </summary>
        public bool Active { get; }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Speed over the ground in knots
        /// </summary>
        public double Speed { get; }

        /// <summary>
        /// Track angle in degrees True
        /// </summary>
        public double Course { get; }

        /// <summary>
        /// Magnetic Variation
        /// </summary>
        public double MagneticVariation { get; }
    }
}
