using Microsoft.Win32;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NtripShare.GNSS.Device.Helper
{
    public class DataChecker
    {
        /// <summary>
        /// </summary>
        public string VersionNo
        {
            get
            {
                return "e5bbfd";
            }
        }

        /// <summary>
        /// 机器识别码
        /// </summary>
        /// <returns></returns>
        public string GetMachineCode()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetLinuxMachineCode();
            }
            try
            {
                string subkey = "PCGUID";
                RegistryKey key = Registry.CurrentUser;//注册表，本地计算机的配置数据
                RegistryKey software = key.OpenSubKey("software\\ntripshare", true);//这里AppBindingPC是程序名，可以随意取
                if (software == null)
                {
                    software = key.CreateSubKey("software\\ntripshare");//创建目录
                }
                if (software.GetValue(subkey) == null)
                {
                    string strGuid = Guid.NewGuid().ToString("N");
                    SetRegistryKey("PCGUID", strGuid);
                }
                return MD5Util.GetMD5HashString(software.GetValue(subkey).ToString());//读取键值

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
                String strHardDiskID = null;//存储磁盘序列号
                foreach (ManagementObject mo in searcher.Get())
                {
                    strHardDiskID = mo["SerialNumber"].ToString().Trim();//记录获得的磁盘序列号
                    break;
                }

                return MD5Util.GetMD5HashString(strHardDiskID);

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// 机器识别码
        /// </summary>
        /// <returns></returns>
        private string GetLinuxMachineCode()
        {
            try
            {
                ProcessStartInfo proc_start_info = new ProcessStartInfo();
                proc_start_info.FileName = "bash";
                proc_start_info.Arguments = "-c dmidecode -s system-uuid";

                proc_start_info.RedirectStandardOutput = true;
                proc_start_info.UseShellExecute = false;
                proc_start_info.CreateNoWindow = true;

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = proc_start_info;
                proc.Start();
                string result = proc.StandardOutput.ReadToEnd();

                return MD5Util.GetMD5HashString(result);

            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// 设置机器码
        /// </summary>
        /// <param name="subkey"></param>
        /// <param name="data"></param>
        private void SetRegistryKey(string subkey, string data)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser;//注册表，本地计算机的配置数据
                RegistryKey software = key.OpenSubKey("software\\ntripshare", true);//这里AppBindingPC是程序名，可以随意取
                if (software == null)
                {
                    software = key.CreateSubKey("software\\ntripshare");//创建目录
                }
                software.SetValue(subkey, data);//创建键值
            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// 校验授权文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool Check(string filePath, out string message)
        {
            bool result = false;
            message = string.Empty;

            string content = System.IO.File.ReadAllText(filePath);
            string[] items = content.Split(new string[] { "+++++=====+++++" }, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length != 3)
            {
                message = "授权文件格式无法识别";
                return false;
            }

            string json = items[0];
            string sign = items[1];
            string publicKeyXml = items[2];

            var fileVersionNo = MD5Util.GetMD5HashString(publicKeyXml).Substring(0, 6);
            //if (fileVersionNo.Equals(this.VersionNo) == false)
            //{
            //    message = "版本识别号有误";
            //    return false;
            //}

            var isVerify = Verify(json, sign, publicKeyXml);

            if (isVerify == false)
            {
                message = "授权文件格式校验未通过";
                return false;
            }

            AFile.DefaultInstance = AFile.FromJson(json);
            string code = "";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                code = this.GetLinuxMachineCode();
            }
            else
            {
                code = this.GetMachineCode();
            }

            if (AFile.DefaultInstance.Check(code) == false)
            {
                message = "应用授权失败，当前授权类型为：" + AFile.DefaultInstance.AuthorizeType;
                System.IO.File.Delete(filePath);
                return false;
            }
            else
            {
                if (AFile.DefaultInstance.getEndTime() < DateTime.Now)
                {
                    message = "应用授权失败，授权已过期" + AFile.DefaultInstance.AuthorizeType;
                    System.IO.File.Delete(filePath);
                    return false;
                }

                result = true;
            }
            return result;
        }

        bool Verify(byte[] data, byte[] Signature, string PublicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //导入公钥，准备验证签名
            rsa.FromXmlString(PublicKey);
            //返回数据验证结果
            return rsa.VerifyData(data, "MD5", Signature);
        }

        public bool Verify(String Text, string SignatureBase64, string PublicKey)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(Text);
            byte[] Signature = Convert.FromBase64String(SignatureBase64);

            return Verify(data, Signature, PublicKey);

        }


    }

    /// <summary>
    /// 授权文件数据格式
    /// </summary>
    public class AFile
    {

        public static AFile DefaultInstance;
        public string Version { get; set; }

        public string AuthorizeType { get; set; }

        public string AuthorizeContent { get; set; }

        public DateTime SignDate { get; set; }

        public DateTime getEndTime()
        {
            if (AFile.DefaultInstance.Version.Contains("年"))
            {
                return SignDate.AddYears(1);
            }
            if (AFile.DefaultInstance.Version.Contains("年"))
            {
                return SignDate.AddMonths(1);
            }
            return SignDate.AddYears(50);
        }


        public bool Check(string machineCode)
        {
            string[] splits = new string[] { ",", "\r\n" };
            string[] items = this.AuthorizeContent.Split(splits, StringSplitOptions.RemoveEmptyEntries);


            foreach (string item in items)
            {
                if (item.Equals(machineCode))
                    return true;
            }

            return false;
        }

        public string ToJson()
        {
            this.SignDate = DateTime.Now;

            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static AFile FromJson(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AFile>(json);
        }
    }
}
