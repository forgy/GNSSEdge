
namespace NtripShare.GNSS.Device.Config
{
    /// <summary>
    /// 配置参数
    /// </summary>
    public class TcpConfig
    {

        public string TcpIP { get; set; } = "0.0.0.0";

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int TcpPort { get; set; } = 0;

        /// <summary>
        /// 数据流
        /// </summary>
        public string TcpStream { get; set; } = "rtcm3";

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool TcpEnable { get; set; }
    }
}
