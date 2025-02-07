using System.Text;
using DotNetty.Buffers;
using DotNetty.Handlers.Flow;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using log4net;
using Microsoft.VisualBasic;
using NtripShare.GNSS.Device.Config;

namespace NtripShare.GNSS.Device.Service
{
	public class NtripCasterService : BaseService
	{
		private static ILog log = LogManager.GetLogger("NtripShare", typeof(NtripCasterService));

		private List<IChannel> ServerChannel = new List<IChannel>();
		IEventLoopGroup bossGroup;
		IEventLoopGroup workerGroup;
		ServerBootstrap bootstrap;
		private static NtripCasterService Instance = null;

		IChannel SChannel = null;
		public static NtripCasterService getInstance()
		{
			if (Instance == null)
			{
				Instance = new NtripCasterService();
			}
			return Instance;
		}

		private NtripCasterService()
		{
			bossGroup = new MultithreadEventLoopGroup();
			workerGroup = new MultithreadEventLoopGroup();
			bootstrap = new ServerBootstrap();
			bootstrap.Group(bossGroup, workerGroup);
			bootstrap.ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
			{
				IChannelPipeline pipeline = channel.Pipeline;
				pipeline.AddLast("idleStateHandle", new IdleStateHandler(20, 60, 0));
				pipeline.AddLast("NtripCasterServiceHandler", new NtripCasterServiceHandler());
			}));
			bootstrap.Channel<TcpServerSocketChannel>()
			.Option(ChannelOption.SoBacklog, 128);
		}


		/// <summary>
		/// 发送log到服务器
		/// </summary>
		/// <param name="logStr"></param>
		public override void SendData(byte[] arr)
		{
			if (!IsServiceStarted )
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
				foreach (IChannel channel in ServerChannel)
				{
					channel.WriteAndFlushAsync(initialMessage);
				}
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
				SChannel.CloseAsync().Wait();
				workerGroup.ShutdownGracefullyAsync().Wait();
				bossGroup.ShutdownGracefullyAsync().Wait();
			}
			catch (Exception ex)
			{
				log.Error("[Error] TcpServer_Close,errmsg=" + ex.Message);
			}
		}

		async void tryConnect(object obj)
		{
			Environment.SetEnvironmentVariable("io.netty.allocator.numHeapArenas", "0");
			try
			{
				SysConfig mosConfig = SysConfig.getInstance();
				SChannel = await bootstrap.BindAsync(mosConfig.NtripCasterConfig.NtripPort);
				IsConnected = true;
				ICY200OK = true;
				log.Info("NtripCasterServiceHandler=" + mosConfig.NtripCasterConfig.NtripPort);
			}
			catch (Exception ex)
			{
				log.Error("[Error] TcpServer_RunThread,errmsg=" + ex.Message);
			}
		}

		/// <summary>
		/// 启动服务
		/// </summary>
		public override void StartService()
		{
			IsServiceStarted = true;
			IsConnected = false;
			ICY200OK = false;
			ThreadPool.QueueUserWorkItem(new WaitCallback(tryConnect));
		}



		private class NtripCasterServiceHandler : FlowControlHandler
		{
			/// <summary>
			/// 服务器连接
			/// </summary>
			/// <param name="context"></param>
			public override void ChannelActive(IChannelHandlerContext context)
			{
				log.Info("ChannelActive");
				if (!NtripCasterService.getInstance().IsServiceStarted)
				{
					context.Channel.CloseAsync().Wait();
				}
			}

			/// <summary>
			/// 服务器断开连接
			/// </summary>
			/// <param name="context"></param>
			public override void ChannelInactive(IChannelHandlerContext context)
			{
				log.Info("ChannelInactive");
				try
				{
					NtripCasterService.getInstance().ServerChannel.Remove(context.Channel);
				}
				catch (Exception ex) { }
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

					string str = Encoding.ASCII.GetString(data).Replace("\n", "");

					string[] lines = Strings.Split(str, System.Convert.ToString('\r'));
					for (var i = 0; i <= lines.Length - 1; i++)
					{
						if (lines[i].StartsWith("GET "))
						{
							SysConfig sysConfig = SysConfig.getInstance();
							string point = "AUTO";
							if (sysConfig.NtripCasterConfig.MountPoint != null && sysConfig.NtripCasterConfig.MountPoint != "")
							{
								point = sysConfig.NtripCasterConfig.MountPoint;
							}
							String header = "SOURCETABLE 200 OK\r\n" +
						   "Content-Type: text/html\r\n" +
						   "Connection: close\r\n";
							String body = "";
							body += $"STR;{point};{point};RTCM 3.2;1074(1),1084(1),1124(1),1005(5),1007(5),1033(5);2;GNSS;NtripShare;CHN;0;0;1;1;NSCaster1.0;none;Basic;N;19200;\r\n";
							body += "ENDSOURCETABLE\r\n";
							header += "Content-Length: " + Encoding.ASCII.GetBytes(body).Length + "\r\n";
							IByteBuffer initialMessage = Unpooled.Buffer(data.Length);
							initialMessage.WriteString(header + body, Encoding.ASCII);
							context.Channel.WriteAndFlushAsync(initialMessage);
						}
						else if (lines[i].StartsWith("Authorization: Basic "))
						{
							byte[] originalbytes = Convert.FromBase64String(lines[i].Replace("Authorization: Basic ", ""));
							string originaltext = System.Text.Encoding.ASCII.GetString(originalbytes);
							if (originaltext.IndexOf(":") >= 0)
							{
								//Check info
								int colinlocation = originaltext.IndexOf(":");
								if (originaltext.Length >= colinlocation + 1)
								{
									string username = originaltext.Substring(0, colinlocation);
									string password = originaltext.Substring(colinlocation + 1);

									SysConfig sysConfig = SysConfig.getInstance();
									if (sysConfig.NtripCasterConfig.PasswordEnable &&
										(!sysConfig.NtripCasterConfig.NtripUserName.Equals(username) || !sysConfig.NtripCasterConfig.NtripPassword.Equals(password)))
									{
										log.Info("ChannelRead--" + username + "----" + password);
										IByteBuffer initialMessage = Unpooled.Buffer("HTTP/1.1 401 Unauthorized\r\n".Length);
										initialMessage.WriteString("HTTP/1.1 401 Unauthorized\r\n", Encoding.Default);
										context.Channel.WriteAndFlushAsync(initialMessage);
										log.Debug($"HTTP/1.1 401 Unauthorize\r\n");
									}
									else
									{
										if (str.Contains("Ntrip/2.0"))
										{
											IByteBuffer initialMessage = Unpooled.Buffer("HTTP/1.1 200 OK\r\nNtrip-Version: Ntrip/2.0\r\nServer: NtripShareCloud/1.0.0\r\n".Length);
											initialMessage.WriteString("HTTP/1.1 200 OK\r\nNtrip-Version: Ntrip/2.0\r\nServer: NtripShareCloud/1.0.0\r\n", Encoding.Default);
											context.Channel.WriteAndFlushAsync(initialMessage);

										}
										else
										{
											IByteBuffer initialMessage = Unpooled.Buffer("ICY 200 OK\r\n".Length);
											initialMessage.WriteString("ICY 200 OK\r\n", Encoding.Default);
											context.Channel.WriteAndFlushAsync(initialMessage);
										}
										NtripCasterService.getInstance().ServerChannel.Add(context.Channel);
										log.Debug($"ICY 200 OK\r\n");
									}
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					log.Error(e.StackTrace);
					log.Error(e.Message);
				}
			}

		}
	}
}
