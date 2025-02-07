using NtripShare.GNSS.Device.Sensor;

namespace NtripShare.GNSS.Device.Config
{
    public class WorkModeConfig
    {
        /// <summary>
        /// 基站ID
        /// </summary>
        public int BaseID { get; set; }
        /// <summary>
        /// 0、Rover
        /// 1、Base
        /// 2、Single
        /// 4、AutoBase
        /// </summary>
        public int WorkMode { get; set; }
        /// <summary>
        /// 基站经度
        /// </summary>
        public double BaseLon { get; set; }
        /// <summary>
        /// 东经/西经
        /// </summary>
        public int BaseLonType { get; set; }
        /// <summary>
        /// 基站纬度
        /// </summary>
        public double BaseLat { get; set; }
        /// <summary>
        /// 南纬/北纬
        /// </summary>
        public int BaseLatType { get; set; }
        /// <summary>
        /// 基站高程
        /// </summary>
        public double BaseHeight { get; set; }
	}
}
