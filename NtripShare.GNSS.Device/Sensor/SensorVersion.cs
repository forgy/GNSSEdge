namespace NtripShare.GNSS.Device.Sensor
{
    public class SensorVersion
    {
        /// <summary>
        /// 产品类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 固件版本 
        /// </summary>
        public string SwVersion { get; set; }
        /// <summary>
        /// 接收机型号 
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 产品 PN 号
        /// </summary>
        public string Pn { get; set; }
        /// <summary>
        /// 序列号 
        /// </summary>
        public string Sn { get; set; }
        /// <summary>
        /// 板卡 ID  
        /// </summary>
        public string EfuseID { get; set; }
        /// <summary>
        /// 固件编译日期 
        /// </summary>
        public string CompTime { get; set; }

        public string WebVersion { get; set; }
    }
}
