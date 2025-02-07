using System.IO.Ports;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
//using log4net;

namespace NtripShare.GNSS.Device.Helper
{
    public class StringHelper
    {
        #region Fields

        //private static readonly ILog _objLogger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        #endregion

        /// <summary>
        /// Converts an ASCII string to it's string representation as hex bytes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToHexString(string s)
        {
            return ToHexString( Encoding.ASCII.GetBytes(s) );
        }

        public static string ToHexString(byte[] arrBytes)
        {
            return ToHexString(arrBytes, 0, arrBytes.Length);
        }

        /// <summary>
        /// Converts an array of bytes to a string of hex bytes
        /// </summary>
        /// <param name="arrBytes"></param>
        /// <param name="iOffset"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        public static string ToHexString(byte[] arrBytes, int iOffset, int iLength)
        {
            if (arrBytes.Length == 0)
                return "";

            var sb = new StringBuilder("");

            var i = 0;
            for (; i < iLength - 1; i++)
            {
                sb.AppendFormat("{0:X2} ", arrBytes[i]);
            }
            sb.AppendFormat("{0:X2}", arrBytes[i]);

            return sb.ToString();

        }

        public static string ToHexString(byte data)
        {
            return string.Format("{0:X2}", data);
        }

        public static StopBits strToStopBits(string s)
        {
            if (s == "1")
            {
                return StopBits.One;
            }
            if (s == "2")
            {
                return StopBits.Two;
            }
            if (s == "1.5")
            {
                return StopBits.OnePointFive;
            }
            return StopBits.None;
        }

		public static bool IPCheck(string IP)
		{
			return Regex.IsMatch(IP, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
		}

		/// <summary>
		/// 传入域名返回对应的IP 
		/// </summary>
		/// <param name="domainName">域名</param>
		/// <returns></returns>
		public static string GetIp(string domainName)
		{
			domainName = domainName.Replace("http://", "").Replace("https://", "");
			IPHostEntry hostEntry = Dns.GetHostEntry(domainName);
			IPEndPoint ipEndPoint = new IPEndPoint(hostEntry.AddressList[0], 0);
			return ipEndPoint.Address.ToString();
		}
    }
}
