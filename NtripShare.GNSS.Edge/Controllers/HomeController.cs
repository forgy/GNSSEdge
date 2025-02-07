using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NtripShare.GNSS.Device.Sensor;
using NtripShare.GNSS.Device.Service;
using NtripShare.GNSS.Device.Config;
using System.Reflection;
using NtripShare.GNSS.Device.NMEA;
using Microsoft.Extensions.Localization;
using NtripShare.Mos.Cal;

namespace NtripShare.GNSS.Edge.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		// 用于提供 HomeController 的区域性资源
		private readonly IStringLocalizer<HomeController> _localizer;
		public HomeController(ILogger<HomeController> logger, IStringLocalizer<HomeController> localizer)
		{
			_logger = logger;
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
			ViewData["User"] = HttpContext.Session.GetString("User");
			return View();
		}

		public IActionResult Version()
		{
			SensorVersion sensorVersion = ConfigService.getInstance().SensorVersion;
			SysConfig sysConfig = SysConfig.getInstance();
			if (sensorVersion != null)
			{
				ViewData["EfuseID"] = sensorVersion.EfuseID;
				ViewData["SwVersion"] = sensorVersion.SwVersion;
				string msg = "";
				bool isCheck = LicenceCheck.Check(SysConfig.getInstance().SoftKey, sensorVersion.EfuseID, out msg);
				if (isCheck)
				{
					ViewData["SoftKey"] = sysConfig.SoftKey;
				}
				else
				{
					ViewData["SoftKey"] = sysConfig.SoftKey + "(无效注册码)";
				}
			}

			ViewData["WebVersion"] = Assembly.GetEntryAssembly().GetName().Version.ToString();
			ViewData["HardwareVersion"] = sysConfig.HardwareVersion;
			ViewData["Model"] = sysConfig.Model + "-" + sysConfig.DeviceKey;

			return View();
		}


		public IActionResult Satellite()
		{
			return View(ConfigService.getInstance().NMEAHelper.GetSatellites());
		}

		public IActionResult SkyView()
		{
			return View();
		}

		public IActionResult Location()
		{
			GPGGA gPGGA = ConfigService.getInstance().NMEAHelper.GPGGA;
			ViewData["Lon"] = AngelUtils.ConvertDigitalToDegrees(gPGGA.Longitude);
			ViewData["Lat"] = AngelUtils.ConvertDigitalToDegrees(gPGGA.Latitude);
			ViewData["Altitude"] = gPGGA.Altitude + gPGGA.GeoidalSeparation;
			ViewData["FixQuality"] = gPGGA.Quality.ToString();
			ViewData["DGPSUpdate"] = gPGGA.TimeSinceLastDgpsUpdate.Seconds;
			ViewData["PDOP"] = ConfigService.getInstance().NMEAHelper.GPGSA.Pdop;
			ViewData["HDOP"] = ConfigService.getInstance().NMEAHelper.GPGSA.Hdop;
			ViewData["VDOP"] = ConfigService.getInstance().NMEAHelper.GPGSA.Vdop;
			ViewData["SatellitesTracked"] = gPGGA.NumberOfSatellites;
			ViewData["SatellitesInView"] =
				ConfigService.getInstance().NMEAHelper.GPGSV.SatellitesPRN.Count +
				ConfigService.getInstance().NMEAHelper.GLGSV.SatellitesPRN.Count +
				ConfigService.getInstance().NMEAHelper.GAGSV.SatellitesPRN.Count +
				ConfigService.getInstance().NMEAHelper.BDGSV.SatellitesPRN.Count +
				ConfigService.getInstance().NMEAHelper.GQGSV.SatellitesPRN.Count;

			if (ConfigService.getInstance().NMEAHelper.GPGST != null)
			{
				ViewData["ErrorOrientation"] = ConfigService.getInstance().NMEAHelper.GPGST.ErrorOrientation;
				ViewData["SigmaLatitudeError"] = ConfigService.getInstance().NMEAHelper.GPGST.SigmaLatitudeError;
				ViewData["SigmaLongitudeError"] = ConfigService.getInstance().NMEAHelper.GPGST.SigmaLongitudeError;
				ViewData["SigmaHeightError"] = ConfigService.getInstance().NMEAHelper.GPGST.SigmaHeightError;
				ViewData["SemiMajorError"] = ConfigService.getInstance().NMEAHelper.GPGST.SemiMajorError;
				ViewData["SemiMinorError"] = ConfigService.getInstance().NMEAHelper.GPGST.SemiMinorError;
			}


			ViewData["GPSInView"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GPGSV.SatellitesPRN);
			ViewData["GlonassInView"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GLGSV.SatellitesPRN);
			ViewData["GalileoInView"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GAGSV.SatellitesPRN);
			ViewData["BDSInView"] = string.Join(",", ConfigService.getInstance().NMEAHelper.BDGSV.SatellitesPRN);
			ViewData["QZSSInView"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GQGSV.SatellitesPRN);

			if (ConfigService.getInstance().NMEAHelper.GPGSA != null)
			{
				ViewData["GPSTracked"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GPGSA.GPSSatelliteIDs);
				ViewData["GlonassTracked"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GPGSA.GLOSatelliteIDs);
				ViewData["GalileoTracked"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GPGSA.GALSatelliteIDs);
				ViewData["BDSTracked"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GPGSA.BDSSatelliteIDs);
				ViewData["QZSSTracked"] = string.Join(",", ConfigService.getInstance().NMEAHelper.GPGSA.QZSSSatelliteIDs);
			}
			return View();
		}



		/// <summary>
		/// 获取列表
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult Satellites()
		{
			List<Satellite> satellites = ConfigService.getInstance().NMEAHelper.GetSatellites();
			//satellites.Add(new Device.NMEA.Satellite() { Sys = "GPS",Azimuth=300,Elevation=65,PRN = "02",SNR = 45});
			//satellites.Add(new Device.NMEA.Satellite() { Sys = "BDS", Azimuth = 200, Elevation = 35, PRN = "02", SNR = 65 });
			//satellites.Add(new Device.NMEA.Satellite() { Sys = "BDS", Azimuth = 100, Elevation = 25, PRN = "02", SNR = 35 });
			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				count = satellites.Count(),
				data = satellites,//直接根据键名进行后端返回
			});
		}

		/// <summary>
		/// 获取列表
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult RestartSys()
		{
			try
			{
				System.Diagnostics.Process p = new System.Diagnostics.Process();
				p.StartInfo.FileName = "reboot";
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.Start();
			}
			catch (Exception e)
			{

			}
			return Json(new
			{//所返回的形式要符合layui的table接收形式
				code = 0,
				msg = "",
				count = 0,
				data = "",//直接根据键名进行后端返回
			});
		}
	}
}