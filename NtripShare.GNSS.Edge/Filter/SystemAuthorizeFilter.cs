using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// 自定义行为过滤器，实现登录及权限的验证
/// </summary>
public class SystemAuthorizeFilter : IActionFilter
{
	public void OnActionExecuted(ActionExecutedContext context)
	{
		//throw new NotImplementedException();
	}

	/// <summary>
	/// 在执行控制器中的Action方法之前执行该方法  判断当前用户是否登录
	/// </summary>
	/// <param name="context"></param>
	public void OnActionExecuting(ActionExecutingContext context)
	{        //排除可以匿名访问的  未登录时
		if (HasAllow(context) == false && context.HttpContext.Session.GetString("User") == null)
		{
			bool isAjax = IsAjax(context.HttpContext.Request);

			//如果是Ajax请求自定义返回json
			if (isAjax)
			{
				context.Result = new JsonResult(new { Code = 401, Msg = "登录已失效，请重新登录2！" })
				{
					StatusCode = StatusCodes.Status401Unauthorized
				};
			}
			else
			{
				ContentResult Content = new ContentResult();
				Content.Content = "<script type='text/javascript'>alert('登录已失效，请重新登录！'); top.location.href='/Login';</script>";
				Content.ContentType = "text/html;charset=utf-8";
				context.Result = Content;
			}
		}
	}
	/// <summary>
	/// 排除掉控制器不需要鉴权  即加[AllowAnonymous]特性的无需鉴权
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public static bool HasAllow(ActionExecutingContext context)
	{
		var filters = context.Filters;
		if (filters.OfType<IAllowAnonymousFilter>().Any())
		{
			return true;
		}
		var endpoint = context.HttpContext.GetEndpoint();
		return endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;
	}

	/// <summary>
	/// 判断是否是Ajax请求
	/// </summary>
	/// <param name="req"></param>
	/// <returns></returns>
	public static bool IsAjax(HttpRequest req)
	{
		bool result = false;
		var xreq = req.Headers.ContainsKey("x-requested-with");
		if (xreq)
		{
			result = req.Headers["x-requested-with"] == "XMLHttpRequest";
		}
		return result;
	}
}