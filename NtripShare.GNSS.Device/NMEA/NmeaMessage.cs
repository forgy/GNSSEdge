using System.Globalization;
using System.Reflection;

namespace NtripShare.GNSS.Device.NMEA
{
	/// <summary>
	/// Nmea message attribute type used on concrete <see cref="NmeaMessage"/> implementations.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class NmeaMessageTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NmeaMessageTypeAttribute"/> class.
        /// </summary>
        /// <param name="nmeaType">The type.</param>
        public NmeaMessageTypeAttribute(string nmeaType)
        {
            NmeaType = nmeaType;
        }
        /// <summary>
        /// Gets or sets the NMEA message type.
        /// </summary>
        public string NmeaType { get; private set; }
    }

    /// <summary>
    /// NMEA Message base class.
    /// </summary>
    public abstract class NmeaMessage
    {
        private readonly static Dictionary<string, ConstructorInfo> messageTypes;

        /// <summary>
        /// Initializes an instance of the NMEA message
        /// </summary>
        /// <param name="messageType">Type</param>
        /// <param name="messageParts">Message values</param>
        protected NmeaMessage(string messageType, string[] messageParts)
        {
            MessageType = messageType;
            MessageParts = messageParts;
        }

        static NmeaMessage()
        {
            messageTypes = new Dictionary<string, ConstructorInfo>();
            var typeinfo = typeof(NmeaMessage).GetTypeInfo();
            foreach (var subclass in typeinfo.Assembly.DefinedTypes.Where(t => t.IsSubclassOf(typeof(NmeaMessage))))
            {
                var attr = subclass.GetCustomAttribute<NmeaMessageTypeAttribute>(false);
                if (attr != null)
                {
                    if (!subclass.IsAbstract)
                    {
                        foreach (var c in subclass.DeclaredConstructors)
                        {
                            var pinfo = c.GetParameters();
                            if (pinfo.Length == 2 && pinfo[0].ParameterType == typeof(string) && pinfo[1].ParameterType == typeof(string[]))
                            {
                                messageTypes.Add(attr.NmeaType, c);
                                break;
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Gets the NMEA message parts.
        /// </summary>
        protected IReadOnlyList<string> MessageParts { get; }

        /// <summary>
        /// Gets the NMEA type id for the message.
        /// </summary>
        public string MessageType { get; }


        /// <summary>
        /// Gets a value indicating whether this message type is proprietary
        /// </summary>
        public bool IsProprietary => MessageType[0] == 'P'; //Appendix B

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "${0},{1}*{2:X2}", MessageType, string.Join(",", MessageParts), Checksum);
        }

        /// <summary>
        /// Gets the checksum value of the message.
        /// </summary>
        public byte Checksum => GetChecksum(MessageType, MessageParts);

		public static byte GetChecksum(string messageType, IReadOnlyList<string> messageParts)
        {
            int checksumTest = 0;
            for (int j = -1; j < messageParts.Count; j++)
            {
                string message = j < 0 ? messageType : messageParts[j];
                if (j >= 0)
                    checksumTest ^= 0x2C; //Comma separator
                for (int i = 0; i < message.Length; i++)
                {
                    checksumTest ^= Convert.ToByte(message[i]);
                }
            }
            return Convert.ToByte(checksumTest);
        }

		public static double StringToLatitude(string value, string ns)
        {
            if (value == null || value.Length < 3)
                return double.NaN;
            double latitude = int.Parse(value.Substring(0, 2), CultureInfo.InvariantCulture) + double.Parse(value.Substring(2), CultureInfo.InvariantCulture) / 60;
            if (ns == "S")
                latitude *= -1;
            return latitude;
        }

		public static double StringToLongitude(string value, string ew)
        {
            if (value == null || value.Length < 4)
                return double.NaN;
            double longitude = int.Parse(value.Substring(0, 3), CultureInfo.InvariantCulture) + double.Parse(value.Substring(3), CultureInfo.InvariantCulture) / 60;
            if (ew == "W")
                longitude *= -1;
            return longitude;
        }

		public static double StringToDouble(string value)
        {
            if(value != null && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return double.NaN;
        }
        public static TimeSpan StringToTimeSpan(string value)
        {
            if (value != null && value.Length >= 6)
            {
                return new TimeSpan(int.Parse(value.Substring(0, 2), CultureInfo.InvariantCulture),
                                   int.Parse(value.Substring(2, 2), CultureInfo.InvariantCulture), 0)
                                   .Add(TimeSpan.FromSeconds(double.Parse(value.Substring(4), CultureInfo.InvariantCulture)));
            }
            return TimeSpan.Zero;
        }
    }
}
