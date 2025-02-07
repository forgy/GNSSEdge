using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Sensor;
using NtripShare.GNSS.Device.Service;
using System.Reflection;
using System.Text;

namespace NtripShare.GNSS.Edge.Controllers
{
	public class SysConfigController : Controller
    {
        static ILog log = LogManager.GetLogger("NtripShare", typeof(SysConfigController));
        private readonly IStringLocalizer<SysConfigController> _localizer;

        public SysConfigController(IStringLocalizer<SysConfigController> localizer)
        {
            _localizer = localizer;
        }

        public IActionResult Pass()
        {
            return View(SysConfig.getInstance());
        }


        /// <summary>
        /// 更新UPNP
        /// </summary>
        /// <param name="trackConfig"></param>
        /// <returns></returns>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public JsonResult setWifi(bool SIM, bool WIFI,int WIFICloseTime, String WIFIPassword)
        {
            SysConfig.getInstance().SIM = SIM;
			SysConfig.getInstance().WIFI = WIFI;
            SysConfig.getInstance().WIFIPassword = WIFIPassword;
            SysConfig.getInstance().WIFICloseTime = WIFICloseTime;
            SysConfig.getInstance().saveConfig();
			WifiService.getInstance().CloseTime = WIFICloseTime;
			if (!WIFI)
            {
                WifiService.getInstance().StopService();
            }
            else {
				WifiService.getInstance().StartService();
			}
			if (SIM)
			{
				SimService.getInstance().StartService();
			}
			else
			{
				SimService.getInstance().StopService();
			}
		
            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = "",//直接根据键名进行后端返回
            });
        }

        public IActionResult Wifi()
        {
            return View(SysConfig.getInstance());
        }
		public IActionResult Restart()
		{
			return View();
		}
		

		/// <summary>
		/// 更新UPNP
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
        public JsonResult setUPNP(bool UPNP,int UPNPCloseTime)
        {
            SysConfig.getInstance().UPNP = UPNP;
            SysConfig.getInstance().UPNPCloseTime = UPNPCloseTime;
            SysConfig.getInstance().saveConfig();

            return Json(new
            {//所返回的形式要符合layui的table接收形式
                code = 0,
                msg = "",
                data = "",//直接根据键名进行后端返回
            });
        }

        public IActionResult Upnp()
        {
            return View(SysConfig.getInstance());
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

        public IActionResult SoftVersion()
        {
            SensorVersion sensorVersion = ConfigService.getInstance().SensorVersion;
            SysConfig sysConfig = SysConfig.getInstance();
            ViewData["Version"] = Assembly.GetEntryAssembly().GetName().Version.ToString();
            ViewData["PublishTime"] = "2024.01.01";
            if (sensorVersion != null){
                ViewData["EfuseID"] = LicenceCheck.GetLinuxMachineCode(sensorVersion.EfuseID);
            }
			ViewData["SoftKey"] = sysConfig.SoftKey;
			return View();
        }


		

		/// <summary>
		/// 更新卫星跟踪配置
		/// </summary>
		/// <param name="trackConfig"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpGet]
		public JsonResult setKey(string key)
		{
			SensorVersion sensorVersion = ConfigService.getInstance().SensorVersion;
            string msg = "";
			bool isCheck = LicenceCheck.Check(key, sensorVersion.EfuseID, out msg);
			

			if (isCheck)
            {
				SysConfig sysConfig = SysConfig.getInstance();
                sysConfig.SoftKey = key;
				sysConfig.saveConfig();
				return Json(new
                {//所返回的形式要符合layui的table接收形式
                    code = 200,
                    msg = "",
                    data = "",//直接根据键名进行后端返回
                });
            }
            else {
				return Json(new
				{//所返回的形式要符合layui的table接收形式
					code = -1,
					msg = msg,
					data = "",//直接根据键名进行后端返回
				});
			}
		
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
	}
}
