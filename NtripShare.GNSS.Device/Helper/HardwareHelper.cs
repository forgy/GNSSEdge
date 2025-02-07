//using CZGL.SystemInfo;
//using System.Diagnostics;
//using System.Management;

//namespace NtripShare.GNSS.Device.Helper
//{
//    public class HardwareHelper
//    {
//        /// <summary>
//        /// 获取的CPU使用率
//        /// </summary>
//        /// <returns></returns>
//        public static int GetCPUUsage()
//        {
//            CPUTime v1 = CPUHelper.GetCPUTime();
//            Thread.Sleep(1000);
//            var v2 = CPUHelper.GetCPUTime();
//            var value = CZGL.SystemInfo.CPUHelper.CalculateCPULoad(v1, v2);
//            //var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
//            //var cpuUsage = (int)cpuCounter.NextValue();
//            return (int)(value*100);
//        }

//        /// <summary>
//        /// 获取cpu温度
//        /// </summary>
//        /// <returns></returns>
//        public static float GetCPUTemperature()
//        {
//            try
//            {
//                string str = "";
//                ManagementObjectSearcher vManagementObjectSearcher = new ManagementObjectSearcher(@"rootWMI", @"select * from MSAcpi_ThermalZoneTemperature");
//                foreach (ManagementObject managementObject in vManagementObjectSearcher.Get())
//                {
//                    str += managementObject.Properties["CurrentTemperature"].Value.ToString();
//                }

//                //这就是CPU的温度了
//                float temp = (float.Parse(str) - 2732) / 10;
//                return temp;
//            }
//            catch (Exception ex)
//            {
//                return 0;
//            }
//        }
//    }
//}
