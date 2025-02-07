namespace NtripShare.GNSS.Device.Config
{
    public class TrackConfig
    {
        /// <summary>
        /// 卫星截止高度角
        /// </summary>
        public int SatelliteElevation { get; set; }

        /// <summary>
        /// 禁止接收机跟踪 GPS 卫星系统
        /// </summary>
        public bool UseGPS { get; set; }
        /// <summary>
        /// 禁止接收机跟踪 BDS 卫星系统
        /// </summary>
        public bool UseBDS { get; set; }
        /// <summary>
        /// 禁止接收机跟踪 GLO 卫星系统
        /// </summary>
        public bool UseGLO { get; set; }
        /// <summary>
        /// 禁止接收机跟踪 GAL 卫星系统
        /// </summary>
        public bool UseGAL { get; set; }
        /// <summary>
        /// 禁止接收机跟踪 QZSS 卫星系统
        /// </summary>
        public bool UseQZSS { get; set; }

        /// <summary>
        /// 是否平滑
        /// </summary>
        public bool Smooth { get; set; }
		public bool B2BPPP { get; set; }
		
	}
}
