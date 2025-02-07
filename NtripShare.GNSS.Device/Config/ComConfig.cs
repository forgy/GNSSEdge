namespace NtripShare.GNSS.Device.Config
{
    /// <summary>
    /// 配置参数
    /// </summary>
    public class ComConfig
	{
        /// <summary>
        /// 0、NONE
        /// 1、TCP
        /// 2、Mqtt
        /// </summary>
        public string SerialName { get; set; } = "COM1";

        /// <summary>
        /// 0、NMEA
        /// 1、RTCM3
        /// </summary>
        public string SerialStream { get; set; } = "rtcm";

		public int SerialBaudRate { get; set; } = 115200;
		//public bool RTCM1005 { get; set; } = false;
		//public bool RTCM1006 { get; set; } = false;
		//      public bool RTCM1033 { get; set; } = false;

		//      public bool RTCM1074 { get; set; } = false;
		//      public bool RTCM1084 { get; set; } = false;
		//      public bool RTCM1094 { get; set; } = false;
		//public bool RTCM1114 { get; set; } = false;
		//public bool RTCM1124 { get; set; } = false;

		//public bool RTCM1042 { get; set; } = false;
		//      public bool RTCM1020 { get; set; } = false;
		//public bool RTCM1044 { get; set; } = false;
		//public bool RTCM1045 { get; set; } = false;
		//      public bool RTCM1046 { get; set; } = false;
		//      public bool RTCM1019 { get; set; } = false;
	}
}
