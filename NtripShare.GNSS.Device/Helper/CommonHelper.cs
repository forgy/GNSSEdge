namespace NtripShare.GNSS.Device.Helper
{
    public class CommonHelper
    {
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>

        public static double dmsToRad(string s)
        {
            string[] ss = s.Split(new char[3] { '°', '′', '″' }, StringSplitOptions.RemoveEmptyEntries);//分割字符串，删除空字符
            double[] d = new double[ss.Length];//新建一个双精度的数值数组
            for (int i = 0; i < d.Length; i++)
                d[i] = Convert.ToDouble(ss[i]);//将度分秒存入双精度的数值数组中
            double sign = d[0] >= 0.0 ? 1.0 : -1.0;//判断正负
            double rad = 0;
            if (d.Length == 1)//根据数组长度进行判断计算
                rad = (Math.Abs(d[0])) * Math.PI / 180;//将度取绝对值，并转换成弧度
            else if (d.Length == 2)
                rad = ((Math.Abs(d[0])) + d[1] / 60) * Math.PI / 180;
            else
                rad = ((Math.Abs(d[0])) + d[1] / 60 + d[2] / 60 / 60) * Math.PI / 180;
            rad = sign * rad;//弧度前边添加正负号
            return rad;//返回弧度值
        }


        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static string radToDms(double rad)
        {
            double sign = rad >= 0.0 ? 1.0 : -1.0;//判断正负
            rad = Math.Abs(rad) * 180 / Math.PI;//将弧度取绝对值并转化为度
            double[] d = new double[3];//新建一个长度为3的数组
            d[0] = (int)rad;//取整获取度
            d[1] = (int)((rad - d[0]) * 60);//取整获取分
            d[2] = (rad - d[0] - d[1] / 60) * 60 * 60;//获取秒不取整
            d[2] = Math.Round(d[2], 2);//将秒保留两位小数
            if (d[2] == 60)
            {
                d[1] += 1;
                d[2] -= 60;
                if (d[1] == 60)
                {
                    d[0] += 1;
                    d[1] -= 60;
                }
            }
            d[0] = sign * d[0];//度前添加正负号
            string s = Convert.ToString(d[0]) + "°" + Convert.ToString(d[1]) + "′" + Convert.ToString(d[2]) + "″";
            //将度分秒赋值给文本框，并添加°′″
            return s;
        }

        /// <summary>
        /// 季度转度分秒
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static string dToDms(double rad)
        {
            double sign = rad >= 0.0 ? 1.0 : -1.0;//判断正负
            //rad = Math.Abs(rad) * 180 / Math.PI;//将弧度取绝对值并转化为度
            double[] d = new double[3];//新建一个长度为3的数组
            d[0] = (int)rad;//取整获取度
            d[1] = (int)((rad - d[0]) * 60);//取整获取分
            d[2] = (rad - d[0] - d[1] / 60) * 60 * 60;//获取秒不取整
            d[2] = Math.Round(d[2], 2);//将秒保留两位小数
            if (d[2] == 60)
            {
                d[1] += 1;
                d[2] -= 60;
                if (d[1] == 60)
                {
                    d[0] += 1;
                    d[1] -= 60;
                }
            }
            d[0] = sign * d[0];//度前添加正负号
            string s = Convert.ToString(d[0]) + "°" + Convert.ToString(d[1]) + "′" + Convert.ToString(d[2]) + "″";
            //将度分秒赋值给文本框，并添加°′″
            return s;
        }

    }
}
