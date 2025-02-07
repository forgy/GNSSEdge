using System.Text;
using System.Security.Cryptography;

namespace NtripShare.GNSS.Device.Config
{
    public class MD5Help
    {
		private static MD5 md5 = MD5.Create();

		//使用utf8编码将字符串散列
		public static string GetMD5HashString(string sourceStr)
		{
			return GetMD5HashString(Encoding.UTF8, sourceStr);
		}
		//使用utf8编码将字符串散列
		public static string GetMD5HashString(string sourceStr, int length)
		{
			string code = GetMD5HashString(Encoding.UTF8, sourceStr);

			if (code.Length > length)
				code = code.Substring(0, length);

			code = code.Replace("o", "a");
			code = code.Replace("O", "a");
			code = code.Replace("0", "a");

			return code;
		}
		//使用指定编码将字符串散列
		public static string GetMD5HashString(Encoding encode, string sourceStr)
		{
			StringBuilder sb = new StringBuilder();

			byte[] source = md5.ComputeHash(encode.GetBytes(sourceStr));
			for (int i = 0; i < source.Length; i++)
			{
				sb.Append(source[i].ToString("x2"));
			}

			return sb.ToString();
		}
		/// <summary>
		/// MD5加密
		/// </summary>
		/// <param name="pToEncrypt"></param>
		/// <param name="sKey"></param>
		/// <returns></returns>
		public static string MD5Encrypt(string pToEncrypt, string sKey)
        {
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                StringBuilder ret = new StringBuilder();
                foreach (byte b in ms.ToArray())
                {
                    ret.AppendFormat("{0:X2}", b);
                }
                ret.ToString();
                return ret.ToString();
            }
            catch
            {
                return "";
            }


        }

        /// <summary>
        /// MD5解密
        /// </summary>
        /// <param name="pToDecrypt"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string MD5Decrypt(string pToDecrypt, string sKey)
        {
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();

                byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
                for (int x = 0; x < pToDecrypt.Length / 2; x++)
                {
                    int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                    inputByteArray[x] = (byte)i;
                }

                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();

                StringBuilder ret = new StringBuilder();

                return System.Text.Encoding.Default.GetString(ms.ToArray());
            }
            catch
            {
                return "";
            }

        }
    }
}
