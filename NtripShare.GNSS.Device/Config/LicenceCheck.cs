using log4net;

namespace NtripShare.GNSS.Device.Config
{
	public class LicenceCheck
	{
		private static string MD5Key = "QwErTyzU";
		static ILog log = LogManager.GetLogger("NtripShare", typeof(LicenceCheck));

		/// <summary>
		/// 机器识别码
		/// </summary>
		/// <returns></returns>
		public static string GetLinuxMachineCode(string add)
		{
			try
			{
				string key = GetId();
				return MD5Help.GetMD5HashString(add + key).ToUpper();
			}
			catch (Exception ex)
			{
				return "";
			}
		}

		/// <summary>
		/// 获取唯一码
		/// </summary>
		/// <returns></returns>
		private static string GetId()
		{
			try
			{
				string[] tmp = File.ReadAllLines("/proc/cpuinfo", System.Text.Encoding.UTF8);
				for (int i = 0; i < tmp.Length; i++)
				{
					if (tmp[i].StartsWith("Serial"))
					{
						return tmp[i].Substring(tmp[i].IndexOf(':') + 2);
					}
				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				return "";
			}
		}
		/// <summary>
		/// 校验授权文件
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static bool Check(string code, string add, out string message)
		{
			try
			{
				if (string.IsNullOrEmpty(code))
				{
					message = "注册码不正确";
					return false;
				}
				string decCode = MD5Help.MD5Decrypt(code, MD5Key);//解密
				if (string.IsNullOrEmpty(decCode))
				{
					message = "注册码不正确!";
					return false;
				}
				if (decCode == GetLinuxMachineCode(add))//唯一ID正确
				{
					message = "注册成功";
					return true;
				}
				else
				{
					message = "注册码不匹配!";
					return false;
				}
			}
			catch
			{
				message = "注册异常";
				return false;
			}
		}
	}
}
