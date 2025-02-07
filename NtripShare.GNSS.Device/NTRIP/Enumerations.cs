
namespace NtripShare.GNSS.Device.NTRIP
{
	/// <summary>
	/// GPSEventType
	/// </summary>
	public enum GPSEventType 
	{
		/// <summary>
		/// Recommended minimum specific GPS/Transit data
		/// </summary>
		GNRMC,
		/// <summary>
		/// Recommended minimum specific GPS/Transit data
		/// </summary>
		GPRMC,
		/// <summary>
		/// Global Positioning System Fix Data
		/// </summary>
		GPGGA,
        /// <summary>
        /// Global Positioning System Fix Data
        /// </summary>
        GNGGA,
        /// <summary>
        /// Satellites in view
        /// </summary>
        GPGSV,
        /// <summary>
        /// Satellites in view
        /// </summary>
        BDGSV,
        /// <summary>
        /// Satellites in view
        /// </summary>
        GBGSV,
        /// <summary>
        /// Satellites in view
        /// </summary>
        GLGSV,
        /// <summary>
        /// Satellites in view
        /// </summary>
        GQGSV,
        /// <summary>
        /// Satellites in view
        /// </summary>
        GAGSV,
		GPGST,
		GNGST,
		/// <summary>
		/// GPS DOP and active satellites
		/// </summary>
		GPGSA,
		/// <summary>
		/// Geographic position, Latitude and Longitude
		/// </summary>
		GNGSA,
		/// <summary>
		/// Geographic position, Latitude and Longitude
		/// </summary>
		GPGLL,
		/// <summary>
		/// Geographic position, Latitude and Longitude
		/// </summary>
		GNGLL,
		/// <summary>
		/// Estimated Position Error - Garmin proprietary sentence(!)
		/// </summary>
		PGRME,
		GPZDA,
		GNZDA,
		GPVTG,
		GNVTG,
		/// <summary>
		/// Data timeout event fired when data haven't been received from GPS device for a while
		/// </summary>
		TimeOut,
		/// <summary>
		/// GPS sentence not recognized, unknown or not implemented
		/// </summary>
		Unknown,
		/// <summary>
		/// Fired when Fix is lost (to be implemented)
		/// </summary>
		FixLost,
		/// <summary>
		/// Fired when valid Fix is acquired (to be implemented)
		/// </summary>
		FixAquired
	}
}
