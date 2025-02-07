namespace NtripShare.GNSS.Device.Config
{
    public class NtripCasterConfig
    {

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int NtripPort { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool NtripEnable { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string MountPoint { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
		public string NtripUserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
		public string NtripPassword { get; set; }
        /// <summary>
        /// 启用密码验证
        /// </summary>
		public bool PasswordEnable { get; set; }
	}
}
