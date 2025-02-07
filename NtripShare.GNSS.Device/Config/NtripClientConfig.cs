namespace NtripShare.GNSS.Device.Config
{
    public class NtripClientConfig
    {
        public int NtripVersion { get; set; } = 1;
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string NtripIP { get; set; } = "0.0.0.0";

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int NtripPort { get; set; } = 0;

        /// <summary>
        /// 服务器用户名
        /// </summary>
        public string NtripUserName { get; set; }

        /// <summary>
        /// 服务器密码
        /// </summary>
        public string NtripPassword { get; set; }

        /// <summary>
        /// 服务器接入点
        /// </summary>
        public string NtripMountPoint { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool NtripEnable { get; set; }
    }
}
