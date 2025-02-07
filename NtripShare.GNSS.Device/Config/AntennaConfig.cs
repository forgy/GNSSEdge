using NtripShare.GNSS.Device.Sensor;

namespace NtripShare.GNSS.Device.Config
{
    public class AntennaConfig
    {
        /// <summary>
        /// 垂直距离（天线高）
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 东向偏差
        /// </summary>
        public double East { get; set; }
        /// <summary>
        /// 北向偏差
        /// </summary>
        public double North { get; set; }
    }
}
