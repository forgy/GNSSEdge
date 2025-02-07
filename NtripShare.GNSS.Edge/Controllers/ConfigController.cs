using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;

namespace NtripShare.GNSS.Edge.Controllers
{
	public class ConfigController : Controller
    {
        static ILog log = LogManager.GetLogger("NtripShare", typeof(ConfigController));
        private readonly IStringLocalizer<ConfigController> _localizer;

        public ConfigController(IStringLocalizer<ConfigController> localizer)
        {
            _localizer = localizer;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult OutSetting()
        {
            return View(SysConfig.getInstance().OutConfig);
        }



		public IActionResult Restart()
		{
			return View();
		}
		
		/// <summary>
		/// 更新卫星跟踪配置
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
        public JsonResult setOutSetting(OutConfig outConfig)
        {
            SysConfig.getInstance().OutConfig = outConfig;
            SysConfig.getInstance().saveConfig();
            ConfigService.getInstance().setOutConfig();
            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = "",//直接根据键名进行后端返回
            });
        }

		public IActionResult SatelliteTrack()
        {
            return View(SysConfig.getInstance().TrackConfig);
        }

        public IActionResult Antenna()
        {
            return View(SysConfig.getInstance().AntennaConfig);
        }

        /// <summary>
        /// 更新卫星跟踪配置
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public JsonResult setAntenna(AntennaConfig antennaConfig)
        {
            SysConfig.getInstance().AntennaConfig = antennaConfig;
            SysConfig.getInstance().saveConfig();

            ConfigService.getInstance().setAntennaConfig();
            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = antennaConfig,//直接根据键名进行后端返回
            });
        }


        /// <summary>
        /// 工作模式 页面
        /// </summary>
        /// <returns></returns>
        public IActionResult WorkMode()
        {
            WorkModeConfig workModeConfig = SysConfig.getInstance().WorkModeConfig;
            if (workModeConfig == null) {
				SysConfig.getInstance().WorkModeConfig = new WorkModeConfig();
			}
            ViewData["BaseID"] = workModeConfig.BaseID;
            ViewData["WorkMode"] = workModeConfig.WorkMode;
            ViewData["BaseLat"] = workModeConfig.BaseLat;
            ViewData["BaseLon"] = workModeConfig.BaseLon;
            ViewData["BaseHeight"] = workModeConfig.BaseHeight;
            return View(workModeConfig);
        }

        /// <summary>
        /// 更新卫星跟踪配置
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public JsonResult setWorkMode(WorkModeConfig workModeConfig)
        {
            SysConfig.getInstance().WorkModeConfig = workModeConfig;
            SysConfig.getInstance().saveConfig();

            ConfigService.getInstance().setWorkModeConfig();
            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = workModeConfig,//直接根据键名进行后端返回
            });
        }


        public IActionResult Pass()
        {
            return View();
        }

        /// <summary>
        /// 更新卫星跟踪配置
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public JsonResult setPass(string password)
        {
            SysConfig.getInstance().Password = password;
            SysConfig.getInstance().saveConfig();

            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = "",//直接根据键名进行后端返回
            });
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

        /// <summary>
        /// 更新卫星跟踪配置
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public JsonResult setSatelliteTrack(TrackConfig trackConfig)
        {
            SysConfig.getInstance().TrackConfig = trackConfig;
            SysConfig.getInstance().saveConfig();

            ConfigService.getInstance().setTracConfig();
            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = trackConfig,//直接根据键名进行后端返回
            });
        }


        /// <summary>
        /// 更新卫星跟踪配置
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public JsonResult getCoordinate()
        {
            if (ConfigService.getInstance().NMEAHelper.GPGGA.Longitude == double.NaN)
            {
                return Json(new
                {//所返回的形式要符合layui的table接收形式
                    code = 0,
                    msg = "",
                    data = new
                    {
                        X = 0,
                        Y = 0,
                        Z = 0
                    }
                });
            }
            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = new
                {
                    X = Math.Round(ConfigService.getInstance().NMEAHelper.GPGGA.Longitude, 8),
                    Y = Math.Round(ConfigService.getInstance().NMEAHelper.GPGGA.Latitude, 8),
                    Z = Math.Round(ConfigService.getInstance().NMEAHelper.GPGGA.Altitude+ ConfigService.getInstance().NMEAHelper.GPGGA.GeoidalSeparation, 4)
				}
            });
        }

        /// <summary>
        /// 更新卫星跟踪配置
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public JsonResult getAvgCoordinate()
        {
            if (ConfigService.getInstance().NMEAHelper.GPGGABffer.Count == 0)
            {
                return Json(new
                {//所返回的形式要符合layui的table接收形式
                    code = 0,
                    msg = "",
                    data = new
                    {
                        X = 0,
                        Y = 0,
                        Z = 0
                    }
                });
            }

            double x = ConfigService.getInstance().NMEAHelper.GPGGABffer.Average(t => t.Longitude);
			double y = ConfigService.getInstance().NMEAHelper.GPGGABffer.Average(t => t.Latitude);
			double z = ConfigService.getInstance().NMEAHelper.GPGGABffer.Average(t => t.Altitude) + ConfigService.getInstance().NMEAHelper.GPGGABffer.Average(t => t.GeoidalSeparation);

            if (double.IsNaN(x)) 
            {
                x = 0;
            }
			if (double.IsNaN(y))
			{
				y = 0;
			}
			if (double.IsNaN(z))
			{
				z = 0;
			}
			return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = new
                {
                    X = Math.Round(x,8),
                    Y = Math.Round(y, 8),
                    Z = Math.Round(z, 4)
				}
            });
        }

        /// <summary>
        /// 更新卫星跟踪配置
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public JsonResult clearAvgCoordinate()
        {
            ConfigService.getInstance().NMEAHelper.GPGGABffer.Clear();
            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = new
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                }
            });

        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        public JsonResult GetSerialPort()
        {
            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = System.IO.Ports.SerialPort.GetPortNames(),//直接根据键名进行后端返回
            });
        }

    }
}
