namespace NtripShare.Mos.Cal
{
    public class AngelUtils
    {
        /// <summary>
        /// 数字经纬度和度分秒经纬度转换 (Digital degree of latitude and longitude and vehicle to latitude and longitude conversion)
        /// </summary>
        /// <param name="digitalLati_Longi">数字经纬度</param>
        /// <return>度分秒经纬度</return>
        static public string ConvertDigitalToDegrees(string digitalLati_Longi)
        {
            double digitalDegree = Convert.ToDouble(digitalLati_Longi);
            return ConvertDigitalToDegrees(digitalDegree);
        }

        /// <summary>
        /// 数字经纬度和度分秒经纬度转换 (Digital degree of latitude and longitude and vehicle to latitude and longitude conversion)
        /// </summary>
        /// <param name="digitalDegree">数字经纬度</param>
        /// <return>度分秒经纬度</return>
        static public string ConvertDigitalToDegrees(double digitalDegree)
        {
            const double num = 60;
            int degree = (int)digitalDegree;
            double tmp = (digitalDegree - degree) * num;
            int minute = (int)tmp;
            double second = (tmp - minute) * num;
            string degrees = "" + degree + "°" + minute + "′" + second + "″";
            return degrees;
        }


        /// <summary>
        /// 度分秒经纬度(必须含有'°')和数字经纬度转换
        /// </summary>
        /// <param name="digitalDegree">度分秒经纬度</param>
        /// <return>数字经纬度</return>
        static public double ConvertDegreesToDigital(string degrees)
        {
            const double num = 60;
            double digitalDegree = 0.0;
            int d = degrees.IndexOf('°');           //度的符号对应的 Unicode 代码为：00B0[1]（六十进制），显示为°。
            if (d < 0)
            {
                return digitalDegree;
            }
            string degree = degrees.Substring(0, d);
            digitalDegree += Convert.ToDouble(degree);

            int m = degrees.IndexOf('′');           //分的符号对应的 Unicode 代码为：2032[1]（六十进制），显示为′。
            if (m < 0)
            {
                return digitalDegree;
            }
            string minute = degrees.Substring(d + 1, m - d - 1);
            digitalDegree += ((Convert.ToDouble(minute)) / num);

            int s = degrees.IndexOf('″');           //秒的符号对应的 Unicode 代码为：2033[1]（六十进制），显示为″。
            if (s < 0)
            {
                return digitalDegree;
            }
            string second = degrees.Substring(m + 1, s - m - 1);
            digitalDegree += (Convert.ToDouble(second) / (num * num));

            return digitalDegree;
        }


        /// <summary>
        /// 度分秒经纬度(必须含有'/')和数字经纬度转换
        /// </summary>
        /// <param name="digitalDegree">度分秒经纬度</param>
        /// <param name="cflag">分隔符</param>
        /// <return>数字经纬度</return>
        static public double ConvertDegreesToDigital_default(string degrees)
        {
            char ch = '/';
            return ConvertDegreesToDigital(degrees, ch);
        }

        /// <summary>
        /// 度分秒经纬度和数字经纬度转换
        /// </summary>
        /// <param name="digitalDegree">度分秒经纬度</param>
        /// <param name="cflag">分隔符</param>
        /// <return>数字经纬度</return>
        static public double ConvertDegreesToDigital(string degrees, char cflag)
        {
            const double num = 60;
            double digitalDegree = 0.0;
            int d = degrees.IndexOf(cflag);
            if (d < 0)
            {
                return digitalDegree;
            }
            string degree = degrees.Substring(0, d);
            digitalDegree += Convert.ToDouble(degree);

            int m = degrees.IndexOf(cflag, d + 1);
            if (m < 0)
            {
                return digitalDegree;
            }
            string minute = degrees.Substring(d + 1, m - d - 1);
            digitalDegree += ((Convert.ToDouble(minute)) / num);

            int s = degrees.Length;
            if (s < 0)
            {
                return digitalDegree;
            }
            string second = degrees.Substring(m + 1, s - m - 1);
            digitalDegree += (Convert.ToDouble(second) / (num * num));

            return digitalDegree;
        }



        public static double TranDMS(double Rad) => Rad = AngelUtils.GetDMSOf(Rad);

        public static double TranDMStoD(double DMS) => DMS = AngelUtils.GetDOf(DMS);

        public static double TranRad(double DMS) => DMS = AngelUtils.GetRADOf(DMS);

        public static double TranSecToRad(double Sec)
        {
            Sec = Sec / 3600.0 / 180.0 * Math.PI;
            return Sec;
        }

        /// <summary>
        /// 计算方向
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static double Direction(double x1, double y1, double x2, double y2)
        {
            double num = 0.0;
            if (x2 > x1 && y2 > y1)
                num = Math.Atan((y2 - y1) / (x2 - x1));
            if (x2 < x1 && y2 < y1)
                num = Math.Atan((y2 - y1) / (x2 - x1)) + Math.PI;
            if (x2 > x1 && y2 < y1)
                num = Math.Atan((y2 - y1) / (x2 - x1)) + 2.0 * Math.PI;
            if (x2 < x1 && y2 > y1)
                num = Math.Atan((y2 - y1) / (x2 - x1)) + Math.PI;
            if (x2 == x1)
            {
                if (y2 > y1)
                    num = Math.PI / 2.0;
                if (y2 < y1)
                    num = 3.0 * Math.PI / 2.0;
            }
            if (y2 == y1)
            {
                if (x2 > x1)
                    num = 0.0;
                if (x2 < x1)
                    num = Math.PI;
            }
            if (num >= 2.0 * Math.PI)
                num -= 2.0 * Math.PI;
            return num;
        }

        /// <summary>
        /// 计算距离
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static double Distance(double x1, double y1, double x2, double y2) => Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));


        /// <summary>
        /// 弧度转度
        /// </summary>
        /// <param name="Rad"></param>
        /// <returns></returns>
        public static double GetDMSOf(double Rad)
        {
            try
            {
                Rad = Rad * 180.0 / Math.PI;
                string str1 = Rad.ToString("F12");
                int length1 = str1.IndexOf(".");
                double num1 = Convert.ToDouble(str1.Substring(0, length1));
                string str2 = (Convert.ToDouble(str1.Substring(length1 + 1, str1.Length - (length1 + 1))) / Math.Pow(10.0, (double)(str1.Length - (length1 + 1))) * 60.0).ToString("F10");
                int length2 = str2.IndexOf(".");
                double num2 = Convert.ToDouble(str2.Substring(0, length2)) / 100.0;
                double num3 = Convert.ToDouble(str2.Substring(length2 + 1, str2.Length - (length2 + 1))) * 60.0 / Math.Pow(10.0, (double)(str2.Length - (length2 + 1) + 4));
                Rad = Rad >= 0.0 ? num1 + num2 + num3 : num1 - num2 - num3;
                return Rad;
            }
            catch
            {
                throw;
            }
        }

        public static double GetDOf(double DMS)
        {
            try
            {
                string str = DMS.ToString("F12");
                int length = str.IndexOf(".");
                double num1 = Convert.ToDouble(str.Substring(0, length));
                double num2 = Convert.ToDouble(str.Substring(length + 1, 2));
                double num3 = Convert.ToDouble(str.Substring(length + 3, str.Length - (length + 3))) / Math.Pow(10.0, (double)(str.Length - (length + 3) - 2));
                DMS = DMS >= 0.0 ? (3600.0 * num1 + 60.0 * num2 + num3) / 3600.0 : (3600.0 * num1 - 60.0 * num2 - num3) / 3600.0;
                return DMS;
            }
            catch
            {
                throw;
            }
        }

        public static double GetRADOf(double DMS)
        {
            try
            {
                string str = DMS.ToString("F12");
                int length = str.IndexOf(".");
                double num1 = Convert.ToDouble(str.Substring(0, length));
                double num2 = Convert.ToDouble(str.Substring(length + 1, 2));
                double num3 = Convert.ToDouble(str.Substring(length + 3, str.Length - (length + 3))) / Math.Pow(10.0, (double)(str.Length - (length + 3) - 2));
                DMS = DMS >= 0.0 ? (3600.0 * num1 + 60.0 * num2 + num3) / 648000.0 * Math.PI : (3600.0 * num1 - 60.0 * num2 - num3) / 648000.0 * Math.PI;
                return DMS;
            }
            catch
            {
                throw;
            }
        }

        public static double GetRADOfGon(double Gon)
        {
            Gon = Gon * 360.0 / 400.0 * Math.PI / 180.0;
            return Gon;
        }
    }
}
