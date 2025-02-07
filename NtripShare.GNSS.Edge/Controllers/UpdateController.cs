using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NtripShare.GNSS.Edge.Models;
using System.IO.Compression;
using System.Text;

namespace NtripShare.GNSS.Edge.Controllers
{
	public class UpdateController : Controller
	{
		static ILog log = LogManager.GetLogger("NtripShare", typeof(SysConfigController));

		[DisableRequestSizeLimit]
		public async Task<IActionResult> FileSave()
		{
		
			var date = Request;
			var files = Request.Form.Files;
			long size = files.Sum(f => f.Length);
			string filename = "";
			var filePath = Environment.CurrentDirectory + "/Update/update.zip";
			foreach (var formFile in files)
			{
				if (formFile.Length > 0)
				{
					filename = formFile.FileName;
					long fileSize = formFile.Length; //获得文件大小，以字节为单位

					if (!Directory.Exists(Environment.CurrentDirectory + "/Update/"))
					{
						Directory.CreateDirectory(Environment.CurrentDirectory + "/Update/");
					}
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await formFile.CopyToAsync(stream);
					}
				}
			}
			if (!Directory.Exists(Environment.CurrentDirectory + "/Update/tmp"))
			{
				Directory.CreateDirectory(Environment.CurrentDirectory + "/Update/tmp");
			}
			else
			{
				Directory.Delete(Environment.CurrentDirectory + "/Update/tmp", true);
				Directory.CreateDirectory(Environment.CurrentDirectory + "/Update/tmp");
			}

			try
			{
				ZipFile.ExtractToDirectory(filePath, Environment.CurrentDirectory + "/Update/tmp", Encoding.UTF8, true);
				if (!System.IO.File.Exists(Environment.CurrentDirectory + "/Update/tmp/Version"))
				{
					return Ok(new { version = "!System.IO.File.Exists", publicTime = "", success = false, msg = "非法固件" });
				}

				StreamReader streamReader = new StreamReader(Environment.CurrentDirectory + "/Update/tmp/Version", Encoding.Default);
				string jsonRoot = streamReader.ReadToEnd();  //读全部json
				UpdateVersion sysConfig = JsonConvert.DeserializeObject<UpdateVersion>(jsonRoot);
				streamReader.Close();
				if (sysConfig != null)
				{
					return Ok(new { version = sysConfig.Version, publicTime = sysConfig.PublicTime, success = true, msg = sysConfig.Description });
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
				return Ok(new { version = "--", publicTime = "", success = false, msg = "非法固件" });
			}
			return Ok(new { version = "--", publicTime = "", success = false, msg = "" });

		}

		/// <summary>
		/// 获取列表
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[Microsoft.AspNetCore.Mvc.HttpPost]
		public JsonResult updateFirmware()
		{
			try
			{
				var filePath = Environment.CurrentDirectory + "/Update/update.zip";
				ZipFile.ExtractToDirectory(filePath, Environment.CurrentDirectory, Encoding.UTF8, true);
			}
			catch (Exception e)
			{
				log.Error(e);
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
