using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Handlers.Flow;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Helper;

namespace NtripShare.GNSS.Device.Service
{
	public class NtripSource2Service : BaseService
	{
		private static ILog log = LogManager.GetLogger("NtripShare", typeof(NtripSource2Service));

		private IChannel ServerChannel = null;
		IEventLoopGroup bossGroup = null;
		public Bootstrap bootstrap = null;
		public int DataSize { get; set; } = 0;

		private static NtripSource2Service Instance = null;
		public static NtripSource2Service getInstance()
		{
			if (Instance == null)
			{
				Instance = new NtripSource2Service();
			}
			return Instance;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		private NtripSource2Service()
		{

		}


		/// <summary>
		/// 发送log到服务器
		/// </summary>
		/// <param name="logStr"></param>
		public override void SendData(byte[] arr)
		{
			if (!IsServiceStarted || !IsConnected || !ICY200OK)
			{
				return;
			}
			if (ServerChannel == null)
			{
				return;
			}

			try
			{
				IByteBuffer initialMessage = Unpooled.Buffer(arr.Length);
				initialMessage.WriteBytes(arr);
				ServerChannel.WriteAndFlushAsync(initialMessage);
				DataSize += arr.Length;
			}
			catch (Exception e)
			{
				log.Error(e.StackTrace);
				log.Error(e.Message);
			}
		}

		/// <summary>
		/// 停止
		/// </summary>
		public override void StopService()
		{
			IsServiceStarted = false;
			IsConnected = false;
			ICY200OK = false;
			try
			{
				if (ServerChannel != null)
				{
					ServerChannel.CloseAsync().Wait();
				}
			}
			catch (Exception e)
			{

			}
		}

		/// <summary>
		/// 连接服务器
		/// </summary>
		public void tryConnect()
		{
			if (!IsServiceStarted)
			{
				return;
			}
			if (bossGroup == null)
			{
				bossGroup = new MultithreadEventLoopGroup(1);
				Environment.SetEnvironmentVariable("io.netty.allocator.numHeapArenas", "0");
			}
			if (bootstrap == null)
			{
				bootstrap = new Bootstrap();
				bootstrap
					.Group(bossGroup)
					.Channel<TcpSocketChannel>()
					.Option(ChannelOption.TcpNodelay, true)
					.Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
					{
						IChannelPipeline pipeline = channel.Pipeline;
						pipeline.AddLast("idleStateHandle", new IdleStateHandler(20, 60, 0));
						pipeline.AddLast("NtripSource2Service", new NtripSourceServiceHandler());
					}));
			}

			Thread retryThread = new Thread(new ThreadStart(delegate
			{
				try
				{
					while (IsServiceStarted && !IsConnected)
					{
						SysConfig mosConfig = SysConfig.getInstance();
						if (StringHelper.IPCheck(mosConfig.NtripServerConfig2.NtripIP))
						{
							bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(mosConfig.NtripServerConfig2.NtripIP), mosConfig.NtripServerConfig2.NtripPort));
						}
						else
						{
							bootstrap.ConnectAsync(mosConfig.NtripServerConfig2.NtripIP, mosConfig.NtripServerConfig2.NtripPort);
						}

						log.Info($"NtripSource2Service.ConnectAsync-{IsConnected}--{mosConfig.NtripServerConfig2.NtripIP}:{mosConfig.NtripServerConfig2.NtripPort}");
						Thread.Sleep(5000);
					}
				}
				catch (Exception ex)
				{
				}
			}));

			retryThread.Start();
		}

		/// <summary>
		/// 启动服务
		/// </summary>
		public override void StartService()
		{
			IsServiceStarted = true;
			IsConnected = false;
			ICY200OK = false;
			//ConnectToProxyServer();
			tryConnect();
		}

		private class NtripSourceServiceHandler : FlowControlHandler
		{
			/// <summary>
			/// 服务器连接
			/// </summary>
			/// <param name="context"></param>
			public override void ChannelActive(IChannelHandlerContext context)
			{
				IChannel channel = NtripSource2Service.getInstance().ServerChannel;
				NtripSource2Service.getInstance().IsConnected = true;
				NtripSource2Service.getInstance().ICY200OK = false;
				NtripSource2Service.getInstance().ServerChannel = context.Channel;
				if (!NtripSource2Service.getInstance().IsServiceStarted)
				{
					context.Channel.CloseAsync().Wait();
					return;
				}

				if (channel != null && context.Channel.Id != channel.Id)
				{
					channel.CloseAsync().Wait();
					log.Info("NtripSourceService2.getInstance().ServerChannel != null");
				}

				SysConfig mosConfig = SysConfig.getInstance();
				byte[] checkServer = Encoding.ASCII.GetBytes(this.getMountPointString());
				IByteBuffer initialMessage = Unpooled.Buffer(checkServer.Length);
				initialMessage.WriteBytes(checkServer);
				log.Info($"验证Ntrip Source2用户名密码---{mosConfig.NtripServerConfig2.NtripUserName}:{mosConfig.NtripServerConfig2.NtripPassword}");
				context.Channel.WriteAndFlushAsync(initialMessage);
				//NtripSource2Service.getInstance().ICY200OK = true;
			}

			/// <summary>
			/// 服务器断开连接
			/// </summary>
			/// <param name="context"></param>
			public override void ChannelInactive(IChannelHandlerContext context)
			{
				//log.Info($"验证Ntrip Source2 ChannelInactive");
				if (NtripSource2Service.getInstance().ServerChannel != null && context.Channel.Id != NtripSource2Service.getInstance().ServerChannel.Id)
				{
					return;
				}
				NtripSource2Service.getInstance().IsConnected = false;
				NtripSource2Service.getInstance().ICY200OK = false;
				NtripSource2Service.getInstance().ServerChannel = null;
				base.ChannelInactive(context);
				if (!NtripSource2Service.getInstance().IsServiceStarted)
				{
					return;
				}
				System.Timers.Timer timer = new System.Timers.Timer(5000);
				timer.Elapsed += new System.Timers.ElapsedEventHandler((s, x) =>
				{
					timer.Stop();
					SysConfig mosConfig = SysConfig.getInstance();
					if (mosConfig.NtripServerConfig2.NtripEnable)
					{
						NtripSource2Service.getInstance().tryConnect();
						//if (StringHelper.IPCheck(mosConfig.NtripServerConfig2.NtripIP))
						//{
						//	NtripSource2Service.getInstance().bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(mosConfig.NtripServerConfig2.NtripIP), mosConfig.NtripServerConfig2.NtripPort));
						//}
						//else
						//{
						//	NtripSource2Service.getInstance().bootstrap.ConnectAsync(mosConfig.NtripServerConfig2.NtripIP, mosConfig.NtripServerConfig2.NtripPort);
						//}
						//NtripSource2Service.getInstance().bootstrap.ConnectAsync(mosConfig.NtripServerConfig2.NtripIP, mosConfig.NtripServerConfig2.NtripPort);
						log.Info($"NtripSourceService2.ConnectAsync-{mosConfig.NtripServerConfig2.NtripIP}:{mosConfig.NtripServerConfig2.NtripPort}");
					}
				});
				timer.Enabled = true;
				timer.Start();
			}

			public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
			{
				// Close the connection when an exception is raised.
				Console.WriteLine($"{exception}");
				context.CloseAsync();
			}

			/// <summary>
			/// 接收到服务器数据
			/// </summary>
			/// <param name="context"></param>
			/// <param name="msg"></param>
			public override void ChannelRead(IChannelHandlerContext context, object msg)
			{
				try
				{
					var buffer = msg as IByteBuffer;
					byte[] data = new byte[buffer.ReadableBytes];
					buffer.ReadBytes(data);

					if (!NtripSource2Service.getInstance().ICY200OK)
					{
						string re = Encoding.ASCII.GetString(data, 0, data.Length);
						if (re.Contains("ICY 200 OK") || re.Contains("HTTP/1.1 200 OK") || re.Contains("200 OK"))
						{
							NtripSource2Service.getInstance().ICY200OK = true;
							log.Info($"验证Ntrip Source用户名密码成功！");
						}
						else if (re.Contains("401 Unauthorized") || re.Contains("ERROR - Bad Password"))
						{
							//Log.i("handleMessage", "Invalid Username or Password.");
							log.Info("NTRIP Server: Bad username or password.");
							context.CloseAsync();
						}
						else if (re.Length > 1024)
						{ // We've received 1KB of data but no start command. WTF?
							log.Info("NTRIP Server: Unrecognized server response:");
							//context.CloseAsync();
						}
						else
						{
							log.Info($"NTRIP Server:{re}");
						}
					}
					else
					{
						if (ConfigService.getInstance() != null)
						{
							ConfigService.getInstance().SendData(data);
						}
					}
				}
				catch (Exception e)
				{
					log.Error(e.StackTrace);
					log.Error(e.Message);
				}
			}


			/// <summary>
			/// 获取Caster验证数据
			/// </summary>
			/// <returns></returns>
			private string getMountPointString()
			{
				SysConfig mosConfig = SysConfig.getInstance();
				if (mosConfig.NtripServerConfig2.NtripVersion == 1)
				{
					string mountPointString =
					$"SOURCE {mosConfig.NtripServerConfig2.NtripPassword} /{mosConfig.NtripServerConfig2.NtripMountPoint}\r\n" +
					"Source-Agent: NtripShareEdge/1.0\r\n" +
					$"STR:\r\n\r\n";
					return mountPointString;
				}
				else
				{
					string mountPointString = $"POST /{mosConfig.NtripServerConfig2.NtripMountPoint} HTTP/1.1\r\n" +
					$"Host: {mosConfig.NtripServerConfig2.NtripIP}\r\n" +
					$"Ntrip-Version: Ntrip/2.0\r\n" +
					$"User-Agent: NtripShareEdge/1.0\r\n" +
					$"Authorization: Basic {Convert.ToBase64String(Encoding.Default.GetBytes(mosConfig.NtripServerConfig2.NtripUserName + ":" + mosConfig.NtripServerConfig2.NtripPassword))}\r\n" +
					$"Ntrip-STR: \r\n" +
					$"Connection: close\r\n" +
					$"Transfer-Encoding: chunked\r\n\r\n";
					return mountPointString;
				}
			}

		}
	}
}
