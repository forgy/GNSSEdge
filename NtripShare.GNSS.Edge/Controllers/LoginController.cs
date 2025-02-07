using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NtripShare.GNSS.Device.Config;
using Microsoft.Extensions.Localization;
using System.Reflection;
using NtripShare.Mos.CoreV2.Service;
using Microsoft.AspNetCore.Authorization;

namespace NtripShare.GNSS.Edge.Controllers
{
	public class LoginController : Controller
    {
		// 用于提供 HomeController 的区域性资源
		private readonly IStringLocalizer<LoginController> _localizer;

		public IActionResult Index()
        {
			//获取cookie中的数据
			if (Request.Cookies.ContainsKey("UserName") && Request.Cookies.ContainsKey("UserPwd"))//存在Cookie信息
			{
				//把保存的用户名和密码赋值给对应的文本框
				//用户名
				var name = Request.Cookies["UserName"].ToString();
				ViewBag.UserName = name;
				//密码
				var pwd = Request.Cookies["UserPwd"].ToString();
				ViewBag.UserPwd = pwd;
			}
			ViewData["Version"] = Assembly.GetEntryAssembly().GetName().Version;
			return View();
        }

		public LoginController(IStringLocalizer<LoginController> localizer)
		{
			_localizer = localizer;
		}


		[AllowAnonymous]
        public IActionResult Login()
        {
			ViewData["Version"] = Assembly.GetEntryAssembly().GetName().Version;
			return View();
        }

        [AllowAnonymous]
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public JsonResult userLogin(string account,string password, bool check)
        {
			//判断是否记住密码
			if (check)
			{
				if (!Request.Cookies.ContainsKey("UserName"))
				{
					HttpContext.Response.Cookies.Append("UserName", account);
				}
				else
				{
					HttpContext.Response.Cookies.Delete("UserName");
					HttpContext.Response.Cookies.Append("UserName", account);
				}
				//将Cookie放在响应报文中发送给浏览器
				if (!Request.Cookies.ContainsKey("UserPwd"))
				{
					HttpContext.Response.Cookies.Append("UserPwd", password);
				}
				else
				{
					HttpContext.Response.Cookies.Delete("UserPwd");
					HttpContext.Response.Cookies.Append("UserPwd", password);
				}
				CookieOptions options = new CookieOptions();
				// 设置过期时间
				options.Expires = DateTime.Now.AddDays(1);
				HttpContext.Response.Cookies.Append("setCookieExpires", "CookieValueExpires", options);
			}
			else
			{
				if (Request.Cookies.ContainsKey("UserName"))
				{
					HttpContext.Response.Cookies.Delete("UserName");
				}
				//将Cookie放在响应报文中发送给浏览器
				if (Request.Cookies.ContainsKey("UserPwd"))
				{
					HttpContext.Response.Cookies.Delete("UserPwd");
				}
				//设置过期时间
				CookieOptions options = new CookieOptions();
				// 设置过期时间
				options.Expires = DateTime.Now.AddDays(-1);
				//保存到客户端
				HttpContext.Response.Cookies.Append("setCookieExpires", "CookieValueExpires", options);
			}

			SysConfig sysConfig = SysConfig.getInstance();
            if (sysConfig.UserName.ToLower() == account.ToLower() && sysConfig.Password.ToLower() == password.ToLower())
            {
				HttpContext.Session.SetString("User", account);
				return Json(new
				{//所返回的形式要符合layui的table接收形式
					code = 200
				});
			}
			return Json(new
			{//所返回的形式要符合layui的table接收形式
                code = -1,
                message = "用户名密码错误",
			});
		}

        [Microsoft.AspNetCore.Mvc.Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToAction("");
        }

		[AllowAnonymous]
		[Microsoft.AspNetCore.Mvc.HttpGet]
		public JsonResult getNet()
		{
			SysConfig mosConfig = SysConfig.getInstance();
			try
			{
				return Json(new
				{//所返回的形式要符合layui的table接收形式
					device = mosConfig.DeviceKey,
					data = NetService.getInstance().RoundtripTime//直接根据键名进行后端返回
				});
			}
			catch (Exception e)
			{
				return Json(new
				{//所返回的形式要符合layui的table接收形式
					device = mosConfig.DeviceKey,
					data = -1//直接根据键名进行后端返回

				});
			}
		}
	}
}