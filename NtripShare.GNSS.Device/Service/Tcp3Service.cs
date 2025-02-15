﻿using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Handlers.Flow;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Helper;

namespace NtripShare.GNSS.Device.Service
{
	public class Tcp3Service : BaseService
	{
		private static ILog log = LogManager.GetLogger("NtripShare", typeof(TcpService));

		private IChannel ServerChannel = null;
		IEventLoopGroup bossGroup = null;
		public Bootstrap bootstrap = null;
		public int DataSize { get; set; } = 0;

		private static Tcp3Service Instance = null;
		public static Tcp3Service getInstance()
		{
			if (Instance == null)
			{
				Instance = new Tcp3Service();
			}
			return Instance;
		}

		/// <summary>
		/// 初始化
		/// </summary>
		private Tcp3Service()
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
		/// 发送log到服务器
		/// </summary>
		/// <param name="logStr"></param>
		public void SendData(string arr)
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
				initialMessage.WriteString(arr, Encoding.Default);
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
			if (ServerChannel != null)
			{
				ServerChannel.CloseAsync().Wait();
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
			Thread retryThread = new Thread(new ThreadStart(delegate
			{
				try
				{
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
								pipeline.AddLast("TcpServiceHandler3", new TcpServiceHandler());
							}));
					}
					while (IsServiceStarted && !IsConnected)
					{
						TcpConfig tcpConfig = SysConfig.getInstance().TcpConfig3;
						if (StringHelper.IPCheck(tcpConfig.TcpIP))
						{
							bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(tcpConfig.TcpIP), tcpConfig.TcpPort));
						}
						else
						{
							bootstrap.ConnectAsync(tcpConfig.TcpIP, tcpConfig.TcpPort);
						}
						//bootstrap.ConnectAsync(tcpConfig.TcpIP, tcpConfig.TcpPort);
						log.Info($"TcpServiceHandler3.ConnectAsync-{IsConnected}--{tcpConfig.TcpIP}:{tcpConfig.TcpPort}");
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
			tryConnect();
		}

		private class TcpServiceHandler : FlowControlHandler
		{
			public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
			{
				// Close the connection when an exception is raised.
				Console.WriteLine($"{exception}");
				context.CloseAsync();
			}
			/// <summary>
			/// 服务器连接
			/// </summary>
			/// <param name="context"></param>
			public override void ChannelActive(IChannelHandlerContext context)
			{
				IChannel channel = Tcp3Service.getInstance().ServerChannel;
				Tcp3Service.getInstance().IsConnected = true;
				Tcp3Service.getInstance().ServerChannel = context.Channel;
				if (!Tcp3Service.getInstance().IsServiceStarted)
				{
					context.Channel.CloseAsync().Wait();
					return;
				}

				if (channel != null && context.Channel.Id != channel.Id)
				{
					channel.CloseAsync().Wait();
					log.Info("NtripClient2Service.getInstance().ServerChannel != null");
				}
				Tcp3Service.getInstance().ICY200OK = true;
				log.Info("Tcp3Service平台已连接");
			}

			/// <summary>
			/// 服务器断开连接
			/// </summary>
			/// <param name="context"></param>
			public override void ChannelInactive(IChannelHandlerContext context)
			{
				if (Tcp3Service.getInstance().ServerChannel != null && context.Channel.Id != Tcp3Service.getInstance().ServerChannel.Id)
				{
					return;
				}
				Tcp3Service.getInstance().IsConnected = false;
				Tcp3Service.getInstance().ICY200OK = false;
				Tcp3Service.getInstance().ServerChannel = null;
				base.ChannelInactive(context);
				log.Info("Tcp3Service平台已duan连接");
				SysConfig mosConfig = SysConfig.getInstance();
				if (!mosConfig.TcpConfig3.TcpEnable)
				{
					return;
				}
				System.Timers.Timer timer = new System.Timers.Timer(5000);
				timer.Elapsed += new System.Timers.ElapsedEventHandler((s, x) =>
				{
					timer.Stop();
					if (mosConfig.TcpConfig3.TcpEnable)
					{
						TcpConfig tcpConfig = SysConfig.getInstance().TcpConfig3;
						if (StringHelper.IPCheck(tcpConfig.TcpIP))
						{
							Tcp3Service.getInstance().bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(tcpConfig.TcpIP), tcpConfig.TcpPort));
						}
						else
						{
							Tcp3Service.getInstance().bootstrap.ConnectAsync(tcpConfig.TcpIP, tcpConfig.TcpPort);
						}
						//Tcp3Service.getInstance().bootstrap.ConnectAsync(tcpConfig.TcpIP, tcpConfig.TcpPort);
					}
				});
				timer.Enabled = true;
				timer.Start();
			
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
					DataService.getInstance().SendData(data);
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
