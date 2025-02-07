using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace NtripShare.GNSS.Edge.Controllers
{
    public class AboutController : Controller
    {
        private readonly IStringLocalizer<AboutController> _localizer;

        public AboutController(IStringLocalizer<AboutController> localizer)
        {
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            return View();
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
