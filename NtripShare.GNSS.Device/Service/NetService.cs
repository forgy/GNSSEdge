using log4net;
using System.Net.NetworkInformation;
using System.Text;

namespace NtripShare.Mos.CoreV2.Service
{
	public class NetService
	{
		private static ILog log = LogManager.GetLogger("NtripShare", typeof(NetService));
		public long RoundtripTime { get; set; }
		private static NetService Instance = null;
		public static NetService getInstance()
		{
			if (Instance == null)
			{
				Instance = new NetService();
			}
			return Instance;
		}

		private NetService()
		{

		}

		/// <summary>
		/// 启动服务
		/// </summary>
		public void StartService()
		{

			Task.Run(() =>
			{
				while (true)
				{
					//远程服务器IP
					string ipStr = "www.baidu.com";
					//构造Ping实例
					Ping pingSender = new Ping();
					//Ping 选项设置
					PingOptions options = new PingOptions();
					options.DontFragment = true;
					//测试数据
					string data = "test";
					byte[] buffer = Encoding.ASCII.GetBytes(data);
					//设置超时时间
					int timeout = 2 * 1000;
					//调用同步 send 方法发送数据,将返回结果保存至PingReply实例
					

					try
					{
						PingReply reply = pingSender.Send(ipStr, timeout, buffer, options);
						if (reply != null && reply.Status == IPStatus.Success)
						{
							this.RoundtripTime = reply.RoundtripTime;
						}
						else
						{
                            ipStr = "8.8.8.8";
                            reply = pingSender.Send(ipStr, timeout, buffer, options);
							if (reply != null && reply.Status == IPStatus.Success)
							{
								this.RoundtripTime = reply.RoundtripTime;
							}
							else {
                                this.RoundtripTime = -1;
                            }
						}
					}
					catch (Exception ex)
					{
						this.RoundtripTime = -1;
						log.Error(ex);
					}
					Thread.Sleep(5000);
				}
			});
		}
	}
}
