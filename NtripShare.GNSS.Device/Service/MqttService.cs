
using log4net;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Protocol;
using NtripShare.GNSS.Device.Config;
using NtripShare.GNSS.Device.Service;
using System.Text;

namespace NtripShare.GNSS.Service
{

	public class MqttService : BaseService
    {
        private static ILog log = LogManager.GetLogger("NtripShare", typeof(MqttService));
        private IMqttClient _MqttClient;
        private static MqttService _mqttService;
		public int DataSize { get; set; } = 0;

		public static MqttService getInstance()
        {
            if (_mqttService == null)
            {
                _mqttService = new MqttService();
            }
            return _mqttService;
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        public override void SendData(byte[] data)
        {
            if (!IsServiceStarted || !IsConnected || !ICY200OK)
            {
                return;
            }
            if (_MqttClient != null)
            {
                SysConfig mosConfig = SysConfig.getInstance();
                var message = new MqttApplicationMessage
                {
                    Topic = mosConfig.MqttConfig.MqttClientTopic,
                    Payload = data,
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                    Retain = true  // 服务端是否保留消息。true为保留，如果有新的订阅者连接，就会立马收到该消息。
                };
                _MqttClient.PublishAsync(message);
            }
        }

        /// <summary>
        /// 发送log到服务器
        /// </summary>
        /// <param name="logStr"></param>
        public void SendData(string arr)
        {
            if (!IsServiceStarted || !IsConnected || !ICY200OK)
            {
                return;
            }

            try
            {
                SysConfig mosConfig = SysConfig.getInstance();
                var message = new MqttApplicationMessage
                {
                    Topic = mosConfig.MqttConfig.MqttClientTopic,
                    Payload = Encoding.Default.GetBytes( arr),
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                    Retain = true  // 服务端是否保留消息。true为保留，如果有新的订阅者连接，就会立马收到该消息。
                };
                _MqttClient.PublishAsync(message);
            }
            catch (Exception e)
            {
                log.Error(e.StackTrace);
                log.Error(e.Message);
            }
        }

        /// <summary>
        /// 自动重连
        /// </summary>
        public override void StartService()
        {
            IsServiceStarted = true;
            tryConnect();
        }

        /// <summary>
        /// 链接
        /// </summary>
        private void tryConnect()
        {
            if (!IsServiceStarted)
            {
                return;
            }
            Thread retryThread = new Thread(new ThreadStart(delegate
             {
                 if (_MqttClient == null)
                 {
                     _BuildClient();
                     Thread.Sleep(3000);
                 }

                 try
                 {
                     //while (!IsConnected)
                     //{
                         _Connect();
                         //Thread.Sleep(2000);
                     //}
                 }
                 catch (Exception ce)
                 {
                     log.Debug("re connect exception:" + ce.Message);
                 }
             }));

            retryThread.Start();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public override void StopService()
        {
            IsServiceStarted = false;
            try
            {
                _MqttClient.DisconnectAsync();
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// 创建客户端
        /// </summary>
        private void _BuildClient()
        {
            try
            {
                _MqttClient = new MqttFactory().CreateMqttClient();
            }
            catch (Exception e)
            {
                log.Debug("build client error:" + e.Message);
                return;
            }

            // 消息到达事件绑定
            _MqttClient.ConnectedAsync += _mqttClient_ConnectedAsync; // 客户端连接成功事件
            _MqttClient.DisconnectedAsync += _mqttClient_DisconnectedAsync; // 客户端连接关闭事件
            _MqttClient.ApplicationMessageReceivedAsync += _MqttClient_ApplicationMessageReceivedAsync; // 收到消息事件

        }

        /// <summary>
        /// 客户端连接关闭事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task _mqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            log.Debug("MqttClient ConnectionClosed");
            IsConnected = false;
            ICY200OK = false;
            Thread.Sleep(3000);
            tryConnect();
			return Task.CompletedTask;
        }

        /// <summary>
        /// 客户端连接成功事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task _mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            IsConnected = true;
            ICY200OK = true;
            log.Debug("MqttClient Connected！");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task _MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            log.Debug("MqttClient Topic:" + e.ApplicationMessage.Topic);
            return Task.CompletedTask;
        }


        /// <summary>
        /// 发起一次连接，连接成功则订阅相关主题 
        /// </summary>
        private void _Connect()
        {
            SysConfig mosConfig = SysConfig.getInstance();
            if (String.IsNullOrEmpty(mosConfig.MqttConfig.MqttUserName))
            {
                var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(mosConfig.MqttConfig.MqttIP, mosConfig.MqttConfig.MqttPort) // 要访问的mqtt服务端的 ip 和 端口号
                .WithClientId(mosConfig.MqttConfig.MqttClientId) // 设置客户端id
                .WithCleanSession()
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = false  // 是否使用 tls加密
                });

                var clientOptions = optionsBuilder.Build();
                _MqttClient.ConnectAsync(clientOptions);
            }
            else
            {
                var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(mosConfig.MqttConfig.MqttIP, mosConfig.MqttConfig.MqttPort) // 要访问的mqtt服务端的 ip 和 端口号
                .WithCredentials(mosConfig.MqttConfig.MqttUserName, mosConfig.MqttConfig.MqttPassword) // 要访问的mqtt服务端的用户名和密码
               .WithClientId(mosConfig.MqttConfig.MqttClientId) // 设置客户端id
                .WithCleanSession()
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = false  // 是否使用 tls加密
                });

                var clientOptions = optionsBuilder.Build();
                _MqttClient.ConnectAsync(clientOptions);
            }
        }
    }
}
