using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace NtripShare.GNSS.Edge.Controllers
{
    public class IndexController : Controller
    {
        private readonly ILogger<IndexController> _logger;
		// 用于提供 HomeController 的区域性资源
		private readonly IStringLocalizer<IndexController> _localizer;
		public IndexController(ILogger<IndexController> logger, IStringLocalizer<IndexController> localizer)
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
			string ss = _localizer["SysName"];
            ViewData["User"] = HttpContext.Session.GetString("User");
            return View();
        }


	}
}