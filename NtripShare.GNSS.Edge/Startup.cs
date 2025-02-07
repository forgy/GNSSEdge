using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;

namespace NtripShare.GNSS.Edge
{
	public class Startup
	{
		public IConfiguration Configuration { get; }
		public IWebHostEnvironment WebHostEnvironment { get; set; }

		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			Configuration = configuration;
			WebHostEnvironment = env;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			//services.Configure<CookiePolicyOptions>(options =>
			//{
			//    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
			//    options.CheckConsentNeeded = context => true;
			//    options.MinimumSameSitePolicy = SameSiteMode.None;
			//});
			//services.AddAuthorization();
			//services.AddRazorPages(options => {
			//	//设置访问Auth文件夹下的页面都需要经过验证。
			//	options.Conventions.AuthorizeFolder("");
			//	//因为 Signin.cshtml 是登录页，所以可以匿名访问。
			//	options.Conventions.AllowAnonymousToPage("/Home/Login");

			//	//上面两行设置可以用下面的这行替换（链式调用）
			//	//options.Conventions.AuthorizeFolder("/Auth").AllowAnonymousToPage("/Auth/Signin");
			//});
			//services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			//				.AddCookie(options =>
			//				{
			//					options.LoginPath = "/Home/Login";
			//					options.LogoutPath = "/Home/Logout";
			//				});
			//services.AddRazorPages();

			// Add base authorization services
			//services.AddAuthorization();
			////身份验证,设置身份验证方式为 Cookie 验证（网页应用程序当然只适合用 cookie）
			//services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
			//	options => { options.LoginPath = new PathString("/Home/Login"); } //设置登录页面为/Auth/Signin
			//	);
			// Add Razor Pages services



			//services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
			//         services.AddMemoryCache();
			//         services.AddSession();
			//services.AddHttpContextAccessor();

			services.Configure<FormOptions>(x =>
			{
				x.ValueLengthLimit = int.MaxValue;
				x.MultipartBodyLengthLimit = int.MaxValue;
				x.MultipartHeadersLengthLimit = int.MaxValue;
			});

			services.AddLocalization(options => options.ResourcesPath = "Resources");
			services.AddSession(option =>
			{
				option.IdleTimeout = TimeSpan.FromMinutes(30);
			});
			////services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
			//services.AddOptions();
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
			{
				//登录路径：这是当用户试图访问资源但未经过身份验证时，程序将会将请求重定向到这个相对路径
				o.LoginPath = new PathString("/Login");
				o.LogoutPath = "/Logout";
				//禁止访问路径：当用户试图访问资源时，但未通过该资源的任何授权策略，请求将被重定向到这个相对路径。
				o.AccessDeniedPath = new PathString("/Index");
			});
			services.AddMvc(options => { options.EnableEndpointRouting = false; });
			//services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
			//配置系统过滤器
			//services.AddControllersWithViews().AddMvcOptions(options =>
			//{
			//	//自定义行为过滤器的方式，验证是否登录及用户权限
			//	options.Filters.Add<SystemAuthorizeFilter>();
			//});
			//.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>

			//{
			//	//登录路径：这是当用户试图访问资源但未经过身份验证时，程序将会将请求重定向到这个相对路径

			//	o.LoginPath = new PathString("/Home/Login");

			//	//禁止访问路径：当用户试图访问资源时，但未通过该资源的任何授权策略，请求将被重定向到这个相对路径。

			//	o.AccessDeniedPath = new PathString("/Home");

			//services.AddMvc();
			//services.Configure<List<Student>>(Configuration.GetSection("UserList"));

		}

		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{


			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			}

			//app.UseHttpsRedirection();
			app.UseStaticFiles(new StaticFileOptions
			{
				//资源所在的绝对路径。
				FileProvider = new PhysicalFileProvider(System.Threading.Thread.GetDomain().BaseDirectory + "/wwwroot"),
			});
			// 启用中间件
			app.UseRequestLocalization(options =>
			{
				var cultures = new[] { "zh-CN", "en-US" };
				options.AddSupportedCultures(cultures);
				options.AddSupportedUICultures(cultures);
				options.SetDefaultCulture(cultures[0]);

				// 当Http响应时，将 当前区域信息 设置到 Response Header：Content-Language 中
				options.ApplyCurrentCultureToResponseHeaders = true;
			});
			//app.UseCookiePolicy();
			app.UseSession();
			app.UseRouting();
			app.UseAuthentication();
			//app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute("default", "{controller=Index}/{action=Index}/{id?}");
			});
			app.UseMvc();

		}
	}
}
