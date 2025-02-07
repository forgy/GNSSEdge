using DotNetty.Transport.Channels;

namespace NtripShare.GNSS.Device.Service
{
    public class BaseService 
    {
		protected IEventLoopGroup bossGroup = null;
		/// <summary>
		/// 服务是否启动
		/// </summary>
		public bool IsServiceStarted { get; set; } = false;
        /// <summary>
        /// 已通过服务器授权
        /// </summary>
        public bool ICY200OK { get; set; } = false;
        /// <summary>
        /// 服务器已连接
        /// </summary>
        public bool IsConnected { get; set; } = false;

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="arr"></param>
        public virtual void SendData(byte[] arr) { }

        /// <summary>
        /// 开始
        /// </summary>
        public virtual void StartService() { }

        /// <summary>
        /// 停止服务
        /// </summary>
        public virtual void StopService() { }
    }
}
