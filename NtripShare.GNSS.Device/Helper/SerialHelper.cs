namespace NtripShare.GNSS.Device.Helper
{
    public class SerialHelper

    {
        /// <summary>
        /// 检查串口是否存在
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        public static bool checkPort(string portName)
        {
            string[] gCOM = System.IO.Ports.SerialPort.GetPortNames(); // 获取设备的所有可用串口
            int j = gCOM.Length; // 得到所有可用串口数目
            for (int i = 0; i < j; i++)
            {
                if (gCOM[i] == portName)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
