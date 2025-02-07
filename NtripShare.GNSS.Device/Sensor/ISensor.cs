using NtripShare.GNSS.Device.Config;

namespace NtripShare.GNSS.Device.Sensor
{
    public interface ISensor
    {

        /// <summary>
        /// 获取版本
        /// </summary>
        /// <returns></returns>
        public List<byte[]> getVersionCommond(SysConfig sysConfig);

        /// <summary>
        /// 获取卫星追踪命令
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        public List<byte[]> getTrackCommond(TrackConfig trackConfig);
        /// <summary>
        /// 获取工作模式命令
        /// </summary>
        /// <param name="workModeConfig"></param>
        /// <returns></returns>
        public List<byte[]> getWorkModeCommond(WorkModeConfig workModeConfig);
        /// <summary>
        /// 获取天线模式命令
        /// </summary>
        /// <param name="antennaConfig"></param>
        /// <returns></returns>
        public List<byte[]> getAntennaCommond(AntennaConfig antennaConfig);

        /// <summary>
        /// 串口输出数据配置
        /// </summary>
        /// <param name="comConfig"></param>
        /// <returns></returns>
        public List<byte[]> getComOutCommond(ComConfig comConfig,OutConfig outConfig);

		/// <summary>
		/// 串口输出数据配置
		/// </summary>
		/// <param name="comConfig"></param>
		/// <returns></returns>
		public List<byte[]> getOutCommond(SysConfig sysConfig);

		/// <summary>
		/// 获取初始化板卡指令
		/// </summary>
		/// <param name="sysConfig"></param>
		/// <returns></returns>
		public List<byte[]> getInitCommond(SysConfig sysConfig);

        /// <summary>
        /// 解析返回结果
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public ResultType getResultType(string str);

        /// <summary>
        /// 解析版本信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public SensorVersion parseVersion(string str);

        public void dealConfigMessage(byte[] data);
	}

    public enum SensorBrand {
        UNKNOW,
        /// <summary>
        /// 和芯星通
        /// </summary>
        UNICORE,
        /// <summary>
        /// 司南
        /// </summary>
        COMNAV,
        /// <summary>
        /// 天宝
        /// </summary>
        TRIMBLE,
        /// <summary>
        /// 诺瓦泰
        /// </summary>
        NovAtel,
        /// <summary>
        /// 半球
        /// </summary>
        HEMISPHERE
    }
}
