using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Helper;
using NtripShare.GNSS.Device.NTRIP;
using NtripShare.GNSS.Device.Service;
using NtripShare.GNSS.Service;
using System.Net;

namespace NtripShare.GNSS.Edge.Controllers
{
	public class IOController : Controller
	{
		private readonly IStringLocalizer<IOController> _localizer;

		public IOController(IStringLocalizer<IOController> localizer)
		{
			_localizer = localizer;
		}
		/// <summary>
		/// 在执行控制器中的Action方法之前执行该方法  判断当前用户是否登录
		/// </summary>
		/// <param name="context"></param>
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (HttpContext.Session.GetString("User") == null)
			{
				context.Result = Redirect("Login");
			}
		}

		public IActionResult Index()
		{
			var model = new
			{
				ComName = SysConfig.getInstance().ComConfig.SerialName,
				ComSerialStream = SysConfig.getInstance().ComConfig.SerialStream,

				CasterPort = SysConfig.getInstance().NtripCasterConfig.NtripPort,
				CasterSerialStream = "RTCM3",
				CasterStart = NtripCasterService.getInstance().IsServiceStarted,
				CasterConnected = NtripCasterService.getInstance().IsConnected,
				CasterAuth = NtripCasterService.getInstance().ICY200OK,

				TCPServer = SysConfig.getInstance().TcpConfig.TcpIP + ":" + SysConfig.getInstance().TcpConfig.TcpPort,
				TcpSerialStream = SysConfig.getInstance().TcpConfig.TcpStream,
				TcpStart = TcpService.getInstance().IsServiceStarted,
				TcpConnected = TcpService.getInstance().IsConnected,
				TcpAuth = TcpService.getInstance().ICY200OK,

				TCPServer2 = SysConfig.getInstance().TcpConfig2.TcpIP + ":" + SysConfig.getInstance().TcpConfig2.TcpPort,
				TcpSerialStream2 = SysConfig.getInstance().TcpConfig2.TcpStream,
				TcpStart2 = Tcp2Service.getInstance().IsServiceStarted,
				TcpConnected2 = Tcp2Service.getInstance().IsConnected,
				TcpAuth2 = Tcp2Service.getInstance().ICY200OK,
				TCPServer3 = SysConfig.getInstance().TcpConfig3.TcpIP + ":" + SysConfig.getInstance().TcpConfig3.TcpPort,
				TcpSerialStream3 = SysConfig.getInstance().TcpConfig3.TcpStream,
				TcpStart3 = Tcp3Service.getInstance().IsServiceStarted,
				TcpConnected3 = Tcp3Service.getInstance().IsConnected,
				TcpAuth3 = Tcp3Service.getInstance().ICY200OK,

				NtripClientServer = SysConfig.getInstance().NtripClientConfig.NtripIP + ":" + SysConfig.getInstance().NtripClientConfig.NtripPort,
				NtripClientStart = NtripClientService.getInstance().IsServiceStarted,
				NtripClientConnected = NtripClientService.getInstance().IsConnected,
				NtripClientAuth = NtripClientService.getInstance().ICY200OK,

				//NtripClientServer2 = SysConfig.getInstance().NtripClientConfig2.NtripIP + ":" + SysConfig.getInstance().NtripClientConfig2.NtripPort,
				//NtripClientStart2 = NtripClient2Service.getInstance().IsServiceStarted,
				//NtripClientConnected2 = NtripClient2Service.getInstance().IsConnected,
				//NtripClientAuth2 = NtripClient2Service.getInstance().ICY200OK,

				//NtripClientServer3 = SysConfig.getInstance().NtripClientConfig3.NtripIP + ":" + SysConfig.getInstance().NtripClientConfig3.NtripPort,
				//NtripClientStart3 = NtripClient3Service.getInstance().IsServiceStarted,
				//NtripClientConnected3 = NtripClient3Service.getInstance().IsConnected,
				//NtripClientAuth3 = NtripClient3Service.getInstance().ICY200OK,


				NtripServerServer = SysConfig.getInstance().NtripServerConfig.NtripIP + ":" + SysConfig.getInstance().NtripServerConfig.NtripPort,
				NtripSeverStart = NtripSourceService.getInstance().IsServiceStarted,
				NtripSeverConnected = NtripSourceService.getInstance().IsConnected,
				NtripSeverAuth = NtripSourceService.getInstance().ICY200OK,

				NtripServerServer2 = SysConfig.getInstance().NtripServerConfig2.NtripIP + ":" + SysConfig.getInstance().NtripServerConfig2.NtripPort,
				NtripSeverStart2 = NtripSource2Service.getInstance().IsServiceStarted,
				NtripSeverConnected2 = NtripSource2Service.getInstance().IsConnected,
				NtripSeverAuth2 = NtripSource2Service.getInstance().ICY200OK,

				NtripServerServer3 = SysConfig.getInstance().NtripServerConfig3.NtripIP + ":" + SysConfig.getInstance().NtripServerConfig3.NtripPort,
				NtripSeverStart3 = NtripSource3Service.getInstance().IsServiceStarted,
				NtripSeverConnected3 = NtripSource3Service.getInstance().IsConnected,
				NtripSeverAuth3 = NtripSource3Service.getInstance().ICY200OK,


				MQTTServer = SysConfig.getInstance().MqttConfig.MqttIP + ":" + SysConfig.getInstance().MqttConfig.MqttPort,
				MQTTSerialStream = SysConfig.getInstance().MqttConfig.MqttStream,
				MQTTStart = MqttService.getInstance().IsServiceStarted,
				MQTTConnected = MqttService.getInstance().IsConnected,
				MQTTAuth = MqttService.getInstance().ICY200OK
			};
			return View(model);
		}

		public IActionResult Mqtt()
		{
			return View(SysConfig.getInstance().MqttConfig);
		}

		/// <summary>
		/// 更新Tcp
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setMqtt(MqttConfig mqttConfig)
		{
			SysConfig.getInstance().MqttConfig = mqttConfig;
			SysConfig.getInstance().saveConfig();
			if (mqttConfig.MqttEnable)
			{
				MqttService.getInstance().StopService();
				MqttService.getInstance().StartService();
			}
			else
			{
				MqttService.getInstance().StopService();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}

		public IActionResult NtripCaster()
		{
			return View(SysConfig.getInstance().NtripCasterConfig);
		}

		/// <summary>
		/// 更新Tcp
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setNtripCaster(NtripCasterConfig ntripCasterConfig)
		{
			if (ntripCasterConfig.NtripPort == 80 || ntripCasterConfig.NtripPort == 22)
			{
				return Json(new
				{//所返回的形式要符合layui的table接收形式
					code = -1,
					msg = "非法端口",
					data = "",//直接根据键名进行后端返回
				});
			}
			SysConfig.getInstance().NtripCasterConfig = ntripCasterConfig;
			SysConfig.getInstance().saveConfig();
			if (ntripCasterConfig.NtripEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					NtripCasterService.getInstance().StopService();
					NtripCasterService.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					NtripCasterService.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}


		public IActionResult Tcp(int channel)
		{
			var model = new
			{
				channel = channel,
				Channel1 = SysConfig.getInstance().TcpConfig,
				Channel2 = SysConfig.getInstance().TcpConfig2,
				Channel3 = SysConfig.getInstance().TcpConfig3,
			};
			return View(model);
		}

		/// <summary>
		/// 更新Tcp
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setTcp(TcpConfig tcpConfig)
		{
			SysConfig.getInstance().TcpConfig = tcpConfig;
			SysConfig.getInstance().saveConfig();
			if (tcpConfig.TcpEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					TcpService.getInstance().StopService();
					TcpService.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					TcpService.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}


		/// <summary>
		/// 更新Tcp
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setTcp2(TcpConfig tcpConfig)
		{
			SysConfig.getInstance().TcpConfig2 = tcpConfig;
			SysConfig.getInstance().saveConfig();
			if (tcpConfig.TcpEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					Tcp2Service.getInstance().StopService();
					Tcp2Service.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					Tcp2Service.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}

		/// <summary>
		/// 更新Tcp
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setTcp3(TcpConfig tcpConfig)
		{
			SysConfig.getInstance().TcpConfig3 = tcpConfig;
			SysConfig.getInstance().saveConfig();
			if (tcpConfig.TcpEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					Tcp3Service.getInstance().StopService();
					Tcp3Service.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					Tcp3Service.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}


		public IActionResult Com()
		{
			return View(SysConfig.getInstance().ComConfig);
		}

		/// <summary>
		/// 更新卫星跟踪配置
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setCOM(ComConfig comConfig)
		{
			SysConfig.getInstance().ComConfig = comConfig;
			SysConfig.getInstance().saveConfig();
			ConfigService.getInstance().setComConfig();
			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}

		public IActionResult NtripClient(int channel)
		{
			var model = new
			{
				channel = channel,
				Channel1 = SysConfig.getInstance().NtripClientConfig,
				//Channel2 = SysConfig.getInstance().NtripClientConfig2,
				//Channel3 = SysConfig.getInstance().NtripClientConfig3,
			};

			return View(model);
		}

		/// <summary>
		/// 更新NtripClient
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setNtripClient(NtripClientConfig ntripClientConfig)
		{
			SysConfig.getInstance().NtripClientConfig = ntripClientConfig;
			SysConfig.getInstance().saveConfig();
			if (ntripClientConfig.NtripEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					NtripClientService.getInstance().StopService();
					NtripClientService.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					NtripClientService.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}

		/// <summary>
		/// 更新NtripClient
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setNtripClient2(NtripClientConfig ntripClientConfig)
		{
			//SysConfig.getInstance().NtripClientConfig2 = ntripClientConfig;
			SysConfig.getInstance().saveConfig();
			if (ntripClientConfig.NtripEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					//NtripClient2Service.getInstance().StopService();
					//NtripClient2Service.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					//NtripClient2Service.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}

		/// <summary>
		/// 更新NtripClient
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setNtripClient3(NtripClientConfig ntripClientConfig)
		{
			//SysConfig.getInstance().NtripClientConfig3 = ntripClientConfig;
			SysConfig.getInstance().saveConfig();
			if (ntripClientConfig.NtripEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					//NtripClient3Service.getInstance().StopService();
					//NtripClient3Service.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					//NtripClient3Service.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}


		public IActionResult NtripServer(int channel)
		{
			var model = new
			{
				channel = channel,
				Channel1 = SysConfig.getInstance().NtripServerConfig,
				Channel2 = SysConfig.getInstance().NtripServerConfig2,
				Channel3 = SysConfig.getInstance().NtripServerConfig3,
			};

			return View(model);
		}

		/// <summary>
		/// 更新NtripClient
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setNtripServer(NtripServerConfig ntripServerConfig)
		{
			SysConfig.getInstance().NtripServerConfig = ntripServerConfig;
			SysConfig.getInstance().saveConfig();
			if (ntripServerConfig.NtripEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					NtripSourceService.getInstance().StopService();
					NtripSourceService.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					NtripSourceService.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}

		/// <summary>
		/// 更新NtripClient
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setNtripServer2(NtripServerConfig ntripServerConfig)
		{
			SysConfig.getInstance().NtripServerConfig2 = ntripServerConfig;
			SysConfig.getInstance().saveConfig();
			if (ntripServerConfig.NtripEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					NtripSource2Service.getInstance().StopService();
					NtripSource2Service.getInstance().StartService();
				}).Unwrap();

			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					NtripSource2Service.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}

		/// <summary>
		/// 更新NtripClient
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult setNtripServer3(NtripServerConfig ntripServerConfig)
		{
			SysConfig.getInstance().NtripServerConfig3 = ntripServerConfig;
			SysConfig.getInstance().saveConfig();
			if (ntripServerConfig.NtripEnable)
			{
				Task.Factory.StartNew(async delegate
				{
					NtripSource3Service.getInstance().StopService();
					NtripSource3Service.getInstance().StartService();
				}).Unwrap();
			}
			else
			{
				Task.Factory.StartNew(async delegate
				{
					NtripSource3Service.getInstance().StopService();
				}).Unwrap();
			}

			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				data = "",//直接根据键名进行后端返回
			});
		}


		/// <summary>
		/// 更新卫星跟踪配置
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpGet]
		public JsonResult getMountPoint(string ip, int port)
		{
			if (StringHelper.IPCheck(ip))
			{

				NTRIPClient nTRIPClient = new NTRIPClient(new System.Net.IPEndPoint(IPAddress.Parse(ip), port), null);
				SourceTable sourceTables = nTRIPClient.GetSourceTable();
				return Json(new
				{//所返回的形式要符合layui的table接收形式
					code = 0,
					msg = "",
					data = sourceTables.DataStreams,//直接根据键名进行后端返回
				});
			}
			else {
				NTRIPClient nTRIPClient = new NTRIPClient(new System.Net.IPEndPoint(IPAddress.Parse(StringHelper.GetIp( ip)), port), null);
				SourceTable sourceTables = nTRIPClient.GetSourceTable();
				return Json(new
				{//所返回的形式要符合layui的table接收形式
					code = 0,
					msg = "",
					data = sourceTables.DataStreams,//直接根据键名进行后端返回
				});
			}
			
		}


	}
}
