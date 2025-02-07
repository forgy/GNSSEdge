//using System.Net;
//using System.Text;
//using DotNetty.Buffers;
//using DotNetty.Handlers.Flow;
//using DotNetty.Handlers.Timeout;
//using DotNetty.Transport.Bootstrapping;
//using DotNetty.Transport.Channels;
//using DotNetty.Transport.Channels.Sockets;
//using log4net;
//using NtripShare.GNSS.Device.Config;
//using NtripShare.GNSS.Device.Helper;

//namespace NtripShare.GNSS.Device.Service
//{
//	public class NtripClient2Service : BaseService
//	{
//		private static ILog log = LogManager.GetLogger("NtripShare", typeof(NtripClient2Service));

//		private IChannel ServerChannel = null;
//		IEventLoopGroup bossGroup = null;
//		public Bootstrap bootstrap = null;
//		public int DataSize { get; set; } = 0;

//		private static NtripClient2Service Instance = null;
//		public static NtripClient2Service getInstance()
//		{
//			if (Instance == null)
//			{
//				Instance = new NtripClient2Service();
//			}
//			return Instance;
//		}

//		private NtripClient2Service()
//		{

//		}


//		/// <summary>
//		/// 发送log到服务器
//		/// </summary>
//		/// <param name="logStr"></param>
//		public override void SendData(byte[] arr)
//		{
//			if (!IsServiceStarted || !IsConnected || !ICY200OK)
//			{
//				return;
//			}
//			if (ServerChannel == null)
//			{
//				return;
//			}
//			try
//			{
//				IByteBuffer initialMessage = Unpooled.Buffer(arr.Length);
//				initialMessage.WriteBytes(arr);
//				ServerChannel.WriteAndFlushAsync(initialMessage);
//			}
//			catch (Exception e)
//			{
//				log.Error(e.StackTrace);
//				log.Error(e.Message);
//			}
//		}

//		/// <summary>
//		/// 发送log到服务器
//		/// </summary>
//		/// <param name="logStr"></param>
//		public void SendData(string data)
//		{

//			if (!IsServiceStarted || !IsConnected || !ICY200OK)
//			{
//				return;
//			}
//			if (ServerChannel == null)
//			{
//				return;
//			}
//			try
//			{
//				log.Debug($"SendData {data}");
//				IByteBuffer initialMessage = Unpooled.Buffer(data.Length);
//				initialMessage.WriteString(data, Encoding.Default);
//				ServerChannel.WriteAndFlushAsync(initialMessage);
//			}
//			catch (Exception e)
//			{
//				log.Error(e.StackTrace);
//				log.Error(e.Message);
//			}
//		}


//		/// <summary>
//		/// 停止
//		/// </summary>
//		public override void StopService()
//		{
//			IsServiceStarted = false;
//			IsConnected = false;
//			ICY200OK = false;
//			try
//			{
//				if (ServerChannel != null)
//				{
//					ServerChannel.CloseAsync().Wait();
//				}
//			}
//			catch (Exception e)
//			{

//			}
//		}

//		/// <summary>
//		/// 连接服务器
//		/// </summary>
//		public void tryConnect()
//		{
//			if (!IsServiceStarted)
//			{
//				return;
//			}
//			Thread retryThread = new Thread(new ThreadStart(delegate
//			{
//				try
//				{
//					if (bossGroup == null)
//					{
//						bossGroup = new MultithreadEventLoopGroup(1);
//						Environment.SetEnvironmentVariable("io.netty.allocator.numHeapArenas", "0");
//					}
//					if (bootstrap == null)
//					{
//						bootstrap = new Bootstrap();
//						bootstrap
//							.Group(bossGroup)
//							.Channel<TcpSocketChannel>()
//							.Option(ChannelOption.TcpNodelay, true)
//							.Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
//							{
//								IChannelPipeline pipeline = channel.Pipeline;
//								pipeline.AddLast("idleStateHandle2", new IdleStateHandler(20, 60, 0));
//								pipeline.AddLast("NtripClientService2", new NtripClientServiceHandler());
//							}));
//					}
//					while (IsServiceStarted && !IsConnected)
//					{
//						SysConfig mosConfig = SysConfig.getInstance();

//						if (StringHelper.IPCheck(mosConfig.NtripClientConfig2.NtripIP))
//						{
//							bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(mosConfig.NtripClientConfig2.NtripIP), mosConfig.NtripClientConfig2.NtripPort));
//						}
//						else
//						{
//							bootstrap.ConnectAsync(mosConfig.NtripClientConfig2.NtripIP, mosConfig.NtripClientConfig2.NtripPort);
//						}

//						//bootstrap.ConnectAsync(mosConfig.NtripClientConfig2.NtripIP, mosConfig.NtripClientConfig2.NtripPort);
//						log.Info($"NtripClientService2.ConnectAsync-{IsConnected}--{mosConfig.NtripClientConfig2.NtripIP}:{mosConfig.NtripClientConfig2.NtripPort}");
//						Thread.Sleep(5000);
//					}

//				}
//				catch (Exception ex)
//				{
//				}
//			}));

//			retryThread.Start();
//		}

//		/// <summary>
//		/// 启动服务
//		/// </summary>
//		public override void StartService()
//		{
//			IsServiceStarted = true;
//			IsConnected = false;
//			ICY200OK = false;
//			//ConnectToProxyServer();
//			tryConnect();
//		}



//		private class NtripClientServiceHandler : FlowControlHandler
//		{


//			/// <summary>
//			/// 服务器连接
//			/// </summary>
//			/// <param name="context"></param>
//			public override void ChannelActive(IChannelHandlerContext context)
//			{
//				IChannel channel = NtripClient2Service.getInstance().ServerChannel;
//				NtripClient2Service.getInstance().IsConnected = true;
//				NtripClient2Service.getInstance().ServerChannel = context.Channel;
//                if (!NtripClient2Service.getInstance().IsServiceStarted)
//                {
//                    context.Channel.CloseAsync().Wait();
//                    return;
//                }

//                if (channel != null && context.Channel.Id != channel.Id)
//				{
//					channel.CloseAsync().Wait();
//					log.Info("NtripClient2Service.getInstance().ServerChannel != null");
//				}

//				SysConfig mosConfig = SysConfig.getInstance();

//				byte[] checkServer = Encoding.ASCII.GetBytes(this.getMountPointString());

//				IByteBuffer initialMessage = Unpooled.Buffer(checkServer.Length);
//				initialMessage.WriteBytes(checkServer);
//				log.Info($"验证Ntrip Client用户名密码---{mosConfig.NtripClientConfig2.NtripUserName}:{mosConfig.NtripClientConfig2.NtripPassword}");
//				context.Channel.WriteAndFlushAsync(initialMessage);
//                //NtripClient2Service.getInstance().ICY200OK = true;
//            }

//			/// <summary>
//			/// 服务器断开连接
//			/// </summary>
//			/// <param name="context"></param>
//			public override void ChannelInactive(IChannelHandlerContext context)
//			{
//				if (NtripClient2Service.getInstance().ServerChannel != null && context.Channel.Id != NtripClient2Service.getInstance().ServerChannel.Id)
//				{
//					log.Info("NtripClient2Service.getInstance().ServerChannel != ChannelInactive");
//					return;
//				}
//				NtripClient2Service.getInstance().IsConnected = false;
//				NtripClient2Service.getInstance().ICY200OK = false;
//				NtripClient2Service.getInstance().ServerChannel = null;

//				base.ChannelInactive(context);
//                if (!NtripClient2Service.getInstance().IsServiceStarted)
//                {
//                    return;
//                }
//                log.Info("Caster平台已断开连接");
//				System.Timers.Timer timer = new System.Timers.Timer(5000);
//				timer.Elapsed += new System.Timers.ElapsedEventHandler((s, x) =>
//				{
//					timer.Stop();
//					if (!context.Channel.Active && NtripClient2Service.getInstance().IsServiceStarted)
//					{
//						SysConfig mosConfig = SysConfig.getInstance();
//						if (StringHelper.IPCheck(mosConfig.NtripClientConfig2.NtripIP))
//						{
//							NtripClient2Service.getInstance().bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(mosConfig.NtripClientConfig2.NtripIP), mosConfig.NtripClientConfig2.NtripPort));
//						}
//						else
//						{
//							NtripClient2Service.getInstance().bootstrap.ConnectAsync(mosConfig.NtripClientConfig2.NtripIP, mosConfig.NtripClientConfig2.NtripPort);
//						}
//						//NtripClient2Service.getInstance().bootstrap.ConnectAsync(mosConfig.NtripClientConfig2.NtripIP, mosConfig.NtripClientConfig2.NtripPort);
//						log.Info($"NtripClientService2.ConnectAsync-{mosConfig.NtripClientConfig2.NtripIP}:{mosConfig.NtripClientConfig2.NtripPort}");
//					}
//				});
//				timer.Enabled = true;
//				timer.Start();
//			}


//			/// <summary>
//			/// 接收到服务器数据
//			/// </summary>
//			/// <param name="context"></param>
//			/// <param name="msg"></param>
//			public override void ChannelRead(IChannelHandlerContext context, object msg)
//			{
//				try
//				{
//					var buffer = msg as IByteBuffer;
//					byte[] data = new byte[buffer.ReadableBytes];
//					buffer.ReadBytes(data);

//					if (!NtripClient2Service.getInstance().ICY200OK)
//					{
//						string re = Encoding.ASCII.GetString(data, 0, data.Length);
//						if (re.Contains("ICY 200 OK") || re.Contains("HTTP/1.1 200 OK") || re.Contains("200 OK"))
//						{
//							NtripClient2Service.getInstance().ICY200OK = true;
//							log.Info($"验证Ntrip Client用户名密码成功！");
//						}
//						else if (re.Contains("401 Unauthorized") || re.Contains("ERROR - Bad Password"))
//						{
//							//Log.i("handleMessage", "Invalid Username or Password.");
//							log.Info("NTRIP Client: Bad username or password.");
//							context.CloseAsync().Wait();
//						}
//						else if (re.Length > 1024)
//						{ // We've received 1KB of data but no start command. WTF?
//							log.Info("NTRIP Client: Unrecognized server response:");
//							//context.CloseAsync();
//						}
//					}
//					else
//					{
//						//log.Info($"NTRIP Client: SendData {new BigInteger(1, data).toString(16)}");
//						ConfigService.getInstance().SendData(data);
//					}
//				}
//				catch (Exception e)
//				{
//					log.Error(e.StackTrace);
//					log.Error(e.Message);
//				}
//			}

//			public override void UserEventTriggered(IChannelHandlerContext context, object evt)
//			{
//				base.UserEventTriggered(context, evt);
//				if (!(evt is IdleStateEvent))
//				{
//					return;
//				}

//				IdleStateEvent e = evt as IdleStateEvent;
//				if (e.State == IdleState.ReaderIdle)
//				{
//					log.Info($"Client {context}  ReaderIdle!");
//					context.Channel.CloseAsync().Wait();
//				}
//			}


//			/// <summary>
//			/// 获取Caster验证数据
//			/// </summary>
//			/// <returns></returns>
//			private string getMountPointString()
//			{
//				SysConfig mosConfig = SysConfig.getInstance();
//				string user = $"{mosConfig.NtripClientConfig2.NtripUserName}:{mosConfig.NtripClientConfig2.NtripPassword}";
//				string mountPointString = $"GET /{mosConfig.NtripClientConfig2.NtripMountPoint} HTTP/1.1\r\n" +
//					$"User-Agent: NtripShareEdge/1.0\r\n" +
//					$"Accept: */*\r\n" +
//					$"Connection: close\r\n" +
//					$"Authorization: Basic {Convert.ToBase64String(Encoding.Default.GetBytes(user))}\r\n";

//				if (mosConfig.NtripClientConfig2.NtripVersion == 2)
//				{
//					mountPointString += "Ntrip-Version: Ntrip/2.0\r\n";
//				}
//				mountPointString += "\r\n\r\n";
//				return mountPointString;
//			}

//		}
//	}


//}
