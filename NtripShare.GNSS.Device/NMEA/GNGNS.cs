using System.Globalization;

namespace NtripShare.GNSS.Device.NMEA
{
    public class GNGNS
    {
        /*
         * Example of GNS messages:
         * $GNGNS,014035.00,4332.69262,S,17235.48549,E,RR,13,0.9,25.63,11.24,,*70   //GLONASS
         * $GPGNS,014035.00,,,,,,8,,,,1.0,23*76                                     //GPS
         * $GLGNS,014035.00,,,,,,5,,,,1.0,23*67                                     //GALILEO
         */

        /// <summary>
        /// GNS Mode Indicator
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// No fix. Satellite system not used in position fix, or fix not valid
            /// </summary>
            NoFix,
            /// <summary>
            /// Autonomous. Satellite system used in non-differential mode in position fix
            /// </summary>
            Autonomous,
            /// <summary>
            /// Differential (including all OmniSTAR services). Satellite system used in differential mode in position fix
            /// </summary>
            Differential,
            /// <summary>
            /// Precise. Satellite system used in precision mode. Precision mode is defined as no deliberate degradation (such as Selective Availability) and higher resolution code (P-code) is used to compute position fix.
            /// </summary>
            Precise,
            /// <summary>
            ///  Real Time Kinematic. Satellite system used in RTK mode with fixed integers
            /// </summary>
            RealTimeKinematic,
            /// <summary>
            /// Float RTK. Satellite system used in real time kinematic mode with floating integers
            /// </summary>
            FloatRtk,
            /// <summary>
            /// Estimated (dead reckoning) mode
            /// </summary>
            Estimated,
            /// <summary>
            /// Manual input mode
            /// </summary>
            Manual,
            /// <summary>
            /// Simulator mode
            /// </summary>
            Simulator
        }

        /// <summary>
        /// Navigational status
        /// </summary>
        public enum NavigationalStatus
        {
            /// <summary>
            /// Navigational status not valid, equipment is not providing navigational status indication.
            /// </summary>
            NotValid = 0,
            /// <summary>
            /// Safe: When the estimated positioning accuracy (95% confidence) is within the selected accuracy level corresponding
            /// to the actual navigation mode, and integrity is available and within the requirements for the actual navigation mode,
            /// and a new valid position has been calculated within 1s for a conventional craft, and 0.5s for a high speed craft.
            /// </summary>
            Safe = 3,
            /// <summary>
            /// Caution: When integrity is not available
            /// </summary>
            Caution = 2,
            /// <summary>
            /// Unsafe When the estimated positioning accuracy (95% confidence) is less than the selected accuracy level corresponding
            /// to the actual navigation mode, and integrity is available and within the requirements for the actual navigation mode,
            /// and/or a new valid position has not been calculated within 1s for a conventional craft, and 0.5s for a high speed craft.
            /// </summary>
            Unsafe = 1
        }

        private static Mode ParseModeIndicator(char c)
        {
            switch (c)
            {
                case 'A': return Mode.Autonomous;
                case 'D': return Mode.Differential;
                case 'P': return Mode.Precise;
                case 'R': return Mode.RealTimeKinematic;
                case 'F': return Mode.FloatRtk;
                case 'E': return Mode.Estimated;
                case 'M': return Mode.Manual;
                case 'S': return Mode.Simulator;
                case 'N':
                default: return Mode.NoFix;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GNGNS"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public GNGNS(string NMEAsentence) 
        {
			string[] message = NMEAsentence.Split(new Char[] { ',' });
            FixTime = NmeaMessage.StringToTimeSpan(message[0+1]);
            Latitude = NmeaMessage.StringToLatitude(message[1 + 1], message[2 + 1]);
            Longitude = NmeaMessage.StringToLongitude(message[3 + 1], message[4 + 1]);
            ModeIndicators = message[5 + 1].Select(t => ParseModeIndicator(t)).ToArray();
            NumberOfSatellites = int.Parse(message[6 + 1], CultureInfo.InvariantCulture);
            Hdop = NmeaMessage.StringToDouble(message[7 + 1]);
            OrhometricHeight = NmeaMessage.StringToDouble(message[8 + 1]);
            GeoidalSeparation = NmeaMessage.StringToDouble(message[9 + 1]);
            var timeInSeconds = NmeaMessage.StringToDouble(message[10 + 1]);
            if (!double.IsNaN(timeInSeconds))
                TimeSinceLastDgpsUpdate = TimeSpan.FromSeconds(timeInSeconds);
            else
                TimeSinceLastDgpsUpdate = TimeSpan.MaxValue;
            if (message[11].Length > 0)
                DgpsStationId = message[11 + 1];

            if (message.Length > 12)
            {
                switch (message[12 + 1])
                {
                    case "S": Status = NavigationalStatus.Safe; break;
                    case "C": Status = NavigationalStatus.Caution; break;
                    case "U": Status = NavigationalStatus.Unsafe; break;
                    case "V":
                    default: Status = NavigationalStatus.NotValid; break;
                }
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
        /// Mode indicator for GPS
        /// </summary>
        public Mode GpsModeIndicator => ModeIndicators.Length > 0 ? ModeIndicators[0] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for GLONASS
        /// </summary>
        public Mode GlonassModeIndicator => ModeIndicators.Length > 1 ? ModeIndicators[1] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for Galileo
        /// </summary>
        public Mode GalileoModeIndicator => ModeIndicators.Length > 2 ? ModeIndicators[2] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for Beidou (BDS)
        /// </summary>
        public Mode BDSModeIndicator => ModeIndicators.Length > 3 ? ModeIndicators[3] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for QZSS
        /// </summary>
        public Mode QZSSModeIndicator => ModeIndicators.Length > 4 ? ModeIndicators[4] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for NavIC (IRNSS)
        /// </summary>
        public Mode NavICModeIndicator => ModeIndicators.Length > 5 ? ModeIndicators[5] : Mode.NoFix;

        /// <summary>
        /// Mode indicator for future constallations
        /// </summary>
        public Mode[] ModeIndicators { get; }

        /// <summary>
        /// Number of satellites (SVs) in use
        /// </summary>
        public int NumberOfSatellites { get; }

        /// <summary>
        /// Horizontal Dilution of Precision (HDOP), calculated using all the satellites (GPS, GLONASS, and any future satellites) used in computing the solution reported in each GNS sentence.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hdop")]
        public double Hdop { get; }

        /// <summary>
        /// Orthometric height in meters (MSL reference)
        /// </summary>
        public double OrhometricHeight { get; }

        /// <summary>
        /// Geoidal separation in meters - the difference between the earth ellipsoid surface and mean-sea-level (geoid) surface defined by the reference datum used in the position solution<br/>
        /// '-' = mean-sea-level surface below ellipsoid.
        /// </summary>
        public double GeoidalSeparation { get; }

        /// <summary>
        ///  Age of differential data - <see cref="TimeSpan.MaxValue"/> if talker ID is GN, additional GNS messages follow with GP and/or GL Age of differential data
        /// </summary>
        public TimeSpan TimeSinceLastDgpsUpdate { get; }

        /// <summary>
        /// eference station ID1, range 0000-4095 - Null if talker ID is GN, additional GNS messages follow with GP and/or GL Reference station ID
        /// </summary>
        public string? DgpsStationId { get; }

        /// <summary>
        /// Navigational status
        /// </summary>
        public NavigationalStatus Status { get; }
    }
}
