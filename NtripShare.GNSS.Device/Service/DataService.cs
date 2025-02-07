using log4net;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Serial;
using NtripShare.GNSS.Service;
using System.Diagnostics;

namespace NtripShare.GNSS.Device.Service
{
    public class DataService
    {
        static ILog log = LogManager.GetLogger("NtripShare", typeof(DataService));
        private static DataService _configService;
        private CustomSerialPort _customSerialPort;
        public static DataService getInstance()
        {
            if (_configService == null)
            {
                _configService = new DataService();
            }
            return _configService;
        }


        /// <summary>
        /// 启动服务
        /// </summary>
        public void StartService()
        {
            try
            {
                _customSerialPort = new CustomSerialPort(SysConfig.getInstance().DataSerialPort, SysConfig.getInstance().DataBaudRate);
				_customSerialPort.ReceivedEvent += Data_ReceivedEvent;
				_customSerialPort.ReceiveTimeoutEnable = true;
                _customSerialPort.ReceiveTimeout = 20;

				try
                {
                    _customSerialPort.Open();
                    log.Info($"Service Open DataService Uart [{SysConfig.getInstance().DataSerialPort}] Succful!");
                }
                catch (Exception ex)
                {
                    log.Info($"RunService Open DataService Uart [{SysConfig.getInstance().DataSerialPort}] Exception:{ex}");
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes"></param>
        public void SendData(byte[] bytes)
        {
            _customSerialPort.Write(bytes);
        }

        /// <summary>
        /// 配置串口收到数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="bytes"></param>
        private void Data_ReceivedEvent(object sender, byte[] bytes)
        {
            try
            {
                log.Info($"DataService Data_Received -{bytes.Length}");
                NtripSourceService.getInstance().SendData(bytes);
				NtripSource2Service.getInstance().SendData(bytes);
				NtripSource3Service.getInstance().SendData(bytes);
				NtripCasterService.getInstance().SendData(bytes);
				if (MqttService.getInstance().IsServiceStarted && MqttService.getInstance().IsConnected) {
                    if ("rtcm3".Equals(SysConfig.getInstance().MqttConfig.MqttStream) ) {
                        MqttService.getInstance().SendData(bytes);
                    }
                }
                if (TcpService.getInstance().IsServiceStarted && TcpService.getInstance().IsConnected)
                {
                    if ("rtcm3".Equals(SysConfig.getInstance().TcpConfig.TcpStream))
                    {
                        TcpService.getInstance().SendData(bytes);
                    }
                }
				if (Tcp2Service.getInstance().IsServiceStarted && Tcp2Service.getInstance().IsConnected)
				{
					if ("rtcm3".Equals(SysConfig.getInstance().TcpConfig2.TcpStream))
					{
						Tcp2Service.getInstance().SendData(bytes);
					}
				}
				if (Tcp3Service.getInstance().IsServiceStarted && Tcp3Service.getInstance().IsConnected)
				{
					if ("rtcm3".Equals(SysConfig.getInstance().TcpConfig3.TcpStream))
					{
						Tcp3Service.getInstance().SendData(bytes);
					}
				}
			}
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

    }
}
