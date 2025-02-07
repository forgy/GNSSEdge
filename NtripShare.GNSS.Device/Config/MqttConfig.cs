
namespace NtripShare.GNSS.Device.Config
{
    /// <summary>
    /// 配置参数
    /// </summary>

    public class MqttConfig
    {
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string MqttIP { get; set; } = "0.0.0.0";

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int MqttPort { get; set; } = 0;

        /// <summary>
        /// mqtt客户端ID
        /// </summary>
        public string MqttClientId { get; set; }
        /// <summary>
        /// mqtt数据主题
        /// </summary>
        public string MqttClientTopic { get; set; }

        /// <summary>
        /// 服务器用户名
        /// </summary>
        public string MqttUserName { get; set; }

        /// <summary>
        /// 服务器密码
        /// </summary>
        public string MqttPassword { get; set; }

        /// <summary>
        /// 服务器密码
        /// </summary>
        public string MqttStream { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool MqttEnable { get; set; }
    }
}
