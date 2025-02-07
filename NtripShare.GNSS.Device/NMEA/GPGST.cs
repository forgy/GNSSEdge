namespace NtripShare.GNSS.Device.NMEA
{
    public class GPGST 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GPGST"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public GPGST( string NMEAsentence) 
        {
            try
            {
				string[] message = NMEAsentence.Split(new Char[] { ',' });
				FixTime = NmeaMessage.StringToTimeSpan(message[0+1]);
                Rms = NmeaMessage.StringToDouble(message[1 + 1]);
                SemiMajorError = NmeaMessage.StringToDouble(message[2 + 1]);
                SemiMinorError = NmeaMessage.StringToDouble(message[3 + 1]);
                ErrorOrientation = NmeaMessage.StringToDouble(message[4 + 1]);
                SigmaLatitudeError = NmeaMessage.StringToDouble(message[5 + 1]);
                SigmaLongitudeError = NmeaMessage.StringToDouble(message[6 + 1]);
                SigmaHeightError = NmeaMessage.StringToDouble(message[7 + 1]);
            }
            catch { }          
        }

        /// <summary>
        /// UTC of position fix
        /// </summary>
        public TimeSpan FixTime { get; }

        /// <summary>
        /// RMS value of the standard deviation of the range inputs in the navigation process. Range inputs include pseudoranges and DGNSS corrections.
        /// </summary>
        public double Rms { get; }

        /// <summary>
        /// Standard deviation of semi-major axis of error ellipse in meters.
        /// </summary>
        public double SemiMajorError { get; }

        /// <summary>
        /// Standard deviation of semi-minor axis of error ellipse in meters.
        /// </summary>
        public double SemiMinorError { get; }

        /// <summary>
        /// Orientation of semi-major axis of error ellipse (degrees from true north).
        /// </summary>
        public double ErrorOrientation { get; }

        /// <summary>
        /// Standard deviation of latitude error in meters.
        /// </summary>
        public double SigmaLatitudeError { get; }

        /// <summary>
        /// Standard deviation of longitude error in meters.
        /// </summary>
        public double SigmaLongitudeError { get; }

        /// <summary>
        /// Standard deviation of altitude error in meters.
        /// </summary>
        public double SigmaHeightError { get; }
    }
}
