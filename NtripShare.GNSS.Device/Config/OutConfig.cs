namespace NtripShare.GNSS.Device.Config
{
	/// <summary>
	/// 配置参数
	/// </summary>
	public class OutConfig
    {
		public float NMEA0183Interval { get; set; } = 1;
		public int NavInterval { get; set; } = 1;
		public int OBSInterval { get; set; } = 1;
		public int AntennaInterval { get; set; } = 1;
		public int BaseCoordInterval { get; set; } = 1;
		public bool GPGGA { get; set; } = false;
		public bool GPGLL { get; set; } = false;
		public bool GPGSA { get; set; } = false;
		public bool GPGST { get; set; } = false;
		public bool GPGSV { get; set; } = false;
		public bool GPHDT { get; set; } = false;
		public bool GPRMC { get; set; } = false;
		public bool GPVTG { get; set; } = false;
		public bool GPZDA { get; set; } = false;

		public bool RTCM1005 { get; set; } = false;
		public bool RTCM1006 { get; set; } = false;
        public bool RTCM1033 { get; set; } = false;

        public bool RTCM1074 { get; set; } = false;
        public bool RTCM1084 { get; set; } = false;
        public bool RTCM1094 { get; set; } = false;
		public bool RTCM1114 { get; set; } = false;
		public bool RTCM1124 { get; set; } = false;

		public bool RTCM1042 { get; set; } = false;
        public bool RTCM1020 { get; set; } = false;
		public bool RTCM1044 { get; set; } = false;
		public bool RTCM1045 { get; set; } = false;
        public bool RTCM1046 { get; set; } = false;
        public bool RTCM1019 { get; set; } = false;
    }
}
