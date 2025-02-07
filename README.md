# NtripShare GNSS Edge Web Sys
# NtripShare GNSS接收机Web配置管理系统

系统基于.netcore 开发，需要运行在支持.netcore运行环境的linux发行版（Ubuntu、Debian等）的硬件方案中，已测试硬件方案包括全志T133、H6、H618，H3，瑞芯微Rk3128、Rk3568、Rk3288等主流Arm 32/64硬件方案。

# 系统主要功能如下
1、	支持局域网网络设备发现，双击即可访问配置系统（参考BD970）。
2、	接收机状态实时查询，包括观测卫星分布情况、实时定位结果情况等。
3、	接收机配置，包括跟踪卫星、工作模式（基站或测站）、天线参数、报文设置等。
4、	IO设置，支持Ntrip Client、Ntrip Server、MQTT、TCP与串口输出，支持输出RTCM与NMEA。
5、	支持和芯星通GNSS模块与板卡，支持天宝、诺瓦泰、HemiSphere、司南等常见品牌模块或板卡（需要进一步测试）。
6、	支持手机端访问。
7、	支持语言切换。
8、	支持远程访问（基于frp，请自行搭建frp服务器）


# 安装部署方式：
1、	使用VisualStudio打开工程，发布程序。
2、	嵌入式端安装.netcore运行环境，安装create_ap热点常见文件。
3、	将发布程序安装至嵌入式Linux系统。
4、	修改配置文件appsettings.json，指定GNSS模块/板卡品牌、串口与波特率等。
5、	配置Linux系统，开机自启动。

# 本开源软件遵循GPL协议，如有商用请开源或获取商业授权.
# 技术交流VX：NtripShare
