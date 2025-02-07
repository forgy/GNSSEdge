namespace NtripShare.GNSS.Device.NMEA
{
    /// <summary>
    /// Space Vehicle (SV/Satellite) info structure
    /// </summary>
    public class Satellite
    {
        /// <summary>
        /// 卫星系统
        /// </summary>
        public string Sys { get; set; }
        /// <summary>
        /// Pseudo-Random Number ID
        /// </summary>
        public string PRN { get; set; }
        /// <summary>
        /// Elevation above horizon in degrees (0-90)
        /// </summary>
        public byte Elevation { get; set; }
        /// <summary>
        /// Azimuth	in degrees (0-359)
        /// </summary>
        public short Azimuth { get; set; }
        /// <summary>
        /// Signal-to-noise ratio in dBHZ (0-99)
        /// </summary>
        public byte SNR { get; set; }
        /// <summary>
        /// 获取时间
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
