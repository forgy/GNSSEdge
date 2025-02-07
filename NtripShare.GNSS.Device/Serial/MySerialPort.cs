//using log4net;
//using RJCP.IO.Ports;

using DotNetty.Common.Utilities;
using log4net;
using System.IO.Ports;
using System.Threading;
using System.Timers;

namespace NtripShare.GNSS.Device.Serial
{
	public class CustomSerialPort
	{
		static ILog log = LogManager.GetLogger("NtripShare", typeof(CustomSerialPort));
		protected SerialPort sp;

		private bool TimeoutCheckThreadIsWork;

		private int lastReceiveTick;
		private int lastSendTick;

		private int receiveDatalen;

		private byte[] recviceBuffer;

		System.Timers.Timer timer;

		public string PortName
		{
			get
			{
				return sp.PortName;
			}
			set
			{
				sp.PortName = value;
			}
		}

		public int BaudRate
		{
			get
			{
				return sp.BaudRate;
			}
			set
			{
				sp.BaudRate = value;
			}
		}

		public Parity Parity
		{
			get
			{
				return sp.Parity;
			}
			set
			{
				sp.Parity = value;
			}
		}

		public int DataBits
		{
			get
			{
				return sp.DataBits;
			}
			set
			{
				sp.DataBits = value;
			}
		}

		public StopBits StopBits
		{
			get
			{
				return sp.StopBits;
			}
			set
			{
				sp.StopBits = value;
			}
		}

		public bool IsOpen => sp.IsOpen;

		public bool DtrEnable
		{
			get
			{
				return sp.DtrEnable;
			}
			set
			{
				sp.DtrEnable = value;
			}
		}

		public bool RtsEnable
		{
			get
			{
				return sp.RtsEnable;
			}
			set
			{
				sp.RtsEnable = value;
			}
		}

		public bool ReceiveTimeoutEnable { get; set; } = false;


		public int ReceiveTimeout { get; set; } = 32;


		public int BufSize
		{
			get
			{
				if (recviceBuffer == null)
				{
					return 4096 * 10;
				}

				return recviceBuffer.Length;
			}
			set
			{
				recviceBuffer = new byte[value];
			}
		}

		public event CustomSerialPortReceivedEventHandle ReceivedEvent;

		public CustomSerialPort(string portName, int baudRate = 115200, Parity parity = Parity.None, int databits = 8, StopBits stopBits = StopBits.One)
		{
			sp = new SerialPort
			{
				PortName = portName,
				BaudRate = baudRate,
				Parity = parity,
				DataBits = databits,
				StopBits = stopBits
			};
			DtrEnable = true;
			RtsEnable = true;
		}

		public static string[] GetPortNames()
		{
			List<string> list = new List<string>();
			list.AddRange(SerialPort.GetPortNames());
			list.Sort();
			return list.ToArray();
		}

		public bool Open()
		{
			try
			{
				if (recviceBuffer == null)
				{
					recviceBuffer = new byte[BufSize];
				}

				sp.Open();
				sp.DataReceived += Sp_DataReceived;
				//实例化Timer类，设置间隔时间为10000毫秒；
				timer = new System.Timers.Timer(ReceiveTimeout);

				timer.Elapsed += Timer_Elapsed;

				timer.AutoReset = false;  //设置是执行一次（false）还是一直执行(true)；

				return true;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
		{
			//while (TimeoutCheckThreadIsWork)
			//{
			//log.Info($" Environment.TickCount {lastReceiveTick}--{Environment.TickCount}");
			if (Environment.TickCount - lastReceiveTick > ReceiveTimeout || Environment.TickCount - lastSendTick > 500)
			{
				//log.Info($" XXXXXX {lastReceiveTick}--{Environment.TickCount}");
				if (this.ReceivedEvent != null)
				{
					byte[] array = new byte[receiveDatalen];
					Array.Copy(recviceBuffer, 0, array, 0, receiveDatalen);
					this.ReceivedEvent(this, array);
				}

				receiveDatalen = 0;
				lastSendTick = Environment.TickCount;
				TimeoutCheckThreadIsWork = false;
			}
			else
			{
				Thread.Sleep(8);
			}
			//}
		}


		public void Close()
		{
			if (sp != null && sp.IsOpen)
			{
				sp.DataReceived -= Sp_DataReceived;
				sp.Close();
				if (ReceiveTimeoutEnable)
				{
					Thread.Sleep(ReceiveTimeout);
					ReceiveTimeoutEnable = false;
				}
			}
		}

		public void Dispose()
		{
			if (sp != null)
			{
				sp.Dispose();
			}
		}

		protected void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				int num = 0;
				if (ReceiveTimeoutEnable)
				{
					while (sp.BytesToRead > 0)
					{
						num = sp.BytesToRead;
						while (receiveDatalen + num > BufSize)
						{
							log.Error($"Serial port receives buffer overflow!{receiveDatalen + num}--{BufSize}");
							BufSize = BufSize * 2;
						}

						int num2 = sp.Read(recviceBuffer, receiveDatalen, num);
						if (num2 != num)
						{
							receiveDatalen = 0;
							throw new Exception("Serial port receives exception!");
						}

					
						receiveDatalen += num2;
						lastReceiveTick = Environment.TickCount;
						//log.Info($" sp.Read {receiveDatalen}--{lastReceiveTick}");
						//if (!TimeoutCheckThreadIsWork)
						//{
						//	TimeoutCheckThreadIsWork = true;
						timer.Enabled = false;
						timer.Enabled = true;
						//Thread thread = new Thread(ReceiveTimeoutCheckFunc);
						//thread.Name = "ComReceiveTimeoutCheckThread";
						//thread.Start();
						//}
					}

					return;
				}

				if (this.ReceivedEvent != null)
				{
					int bytesToRead = sp.BytesToRead;
					if (bytesToRead == 0)
					{
						return;
					}

					byte[] array = new byte[bytesToRead];
					int i;
					int num3;
					for (i = 0; i < bytesToRead; i += num3)
					{
						num3 = sp.Read(recviceBuffer, i, bytesToRead - i);
					}

					Array.Copy(recviceBuffer, 0, array, 0, i);
					this.ReceivedEvent(this, array);
				}

				receiveDatalen = 0;
			}
			catch (Exception e2)
			{
				log.Error(PortName + e2);
			}

		}

		protected void ReceiveTimeoutCheckFunc()
		{
			while (TimeoutCheckThreadIsWork)
			{
				if (Environment.TickCount - lastReceiveTick > ReceiveTimeout)
				{
					if (this.ReceivedEvent != null)
					{
						byte[] array = new byte[receiveDatalen];
						Array.Copy(recviceBuffer, 0, array, 0, receiveDatalen);
						this.ReceivedEvent(this, array);
					}

					receiveDatalen = 0;
					TimeoutCheckThreadIsWork = false;
				}
				else
				{
					Thread.Sleep(16);
				}
			}
		}

		public void Write(byte[] buffer)
		{
			if (IsOpen)
			{
				sp.Write(buffer, 0, buffer.Length);
			}
		}

		public void Write(string text)
		{
			if (IsOpen)
			{
				sp.Write(text);
			}
		}

		public void WriteLine(string text)
		{
			if (IsOpen)
			{
				sp.WriteLine(text);
			}
		}

		public static string ByteToHexStr(byte[] bytes)
		{
			string text = "";
			if (bytes != null)
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					text = text + bytes[i].ToString("X2") + " ";
				}
			}

			return text;
		}
		/// <summary>
		/// 串口接收事件
		/// </summary>
		/// <param name="bytes">接收到的数据</param>
		public delegate void CustomSerialPortReceivedEventHandle(object sender, byte[] bytes);
	}
}