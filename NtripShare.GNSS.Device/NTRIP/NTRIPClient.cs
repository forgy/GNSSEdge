using System.Text;
using System.Net;
using System.Net.Sockets;
using NtripShare.GNSS.Device.NMEA;

namespace NtripShare.GNSS.Device.NTRIP
{
    public class NTRIPClient : IDisposable
	{
		private Socket sckt;
		private string _username;
		private string _password;
		private IPEndPoint _broadcaster;
		byte[] rtcmDataBuffer = new byte[128];
		//IAsyncResult m_asynResult;
		public AsyncCallback pfnCallBack;

        /// <summary>
        /// NTRIP server Username
        /// </summary>
        public string UserName
		{
			get { return _username; }
			set { _username = value; }
		}

		/// <summary>
		/// NTRIP server password
		/// </summary>
		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		/// <summary>
		/// NTRIP Server
		/// </summary>
		public IPEndPoint BroadCaster
		{
			get { return _broadcaster; }
			set { _broadcaster = value; }
		}

		private NMEAHelper gps;

		public NTRIPClient(IPEndPoint Server, NMEAHelper gpsHandler)
		{
			//Initialization...
			gps = gpsHandler;
			BroadCaster = Server;
			//InitializeSocket();
		}

		public NTRIPClient(IPEndPoint Server, string strUserName, string strPassword, NMEAHelper gpsHandler) : this(Server, gpsHandler)
		{
			_username = strUserName;
			_password = strPassword;
			//InitializeSocket();
		}

		private void InitializeSocket()
		{
			sckt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		

		/// <summary>
		/// Creates request to NTRIP server
		/// </summary>
		/// <param name="strRequest">Resource to request. Leave blank to get NTRIP service data</param>
		private byte[] CreateRequest(string strRequest)
		{
			//Build request message
			string msg = "GET /" + strRequest + " HTTP/1.1\r\n";
			msg += "User-Agent: SharpGps iter.dk\r\n";
			//If password/username is specified, send login details
			if (_username != null && _username != "")
			{
				string auth = ToBase64(_username + ":" + _password);
				msg += "Authorization: Basic " + auth + "\r\n";
			}
			msg += "Accept: */*\r\nConnection: close\r\n";
			msg += "\r\n";

			return System.Text.Encoding.ASCII.GetBytes(msg);	
		}

		private void Connect()
		{
			//Connect to server	   
			if (!sckt.Connected)
				sckt.Connect(BroadCaster);
		}

		private void Close()
		{
			sckt.Shutdown(SocketShutdown.Both);
			sckt.Close();
		}

		public SourceTable GetSourceTable()
		{
			this.InitializeSocket();
			sckt.Blocking = true;
			this.Connect();
			sckt.Send(CreateRequest(""));
			string responseData = "";
			System.Threading.Thread.Sleep(1000); //Wait for response
			while (sckt.Available>0)
			{
				byte[] returndata = new byte[sckt.Available];
				sckt.Receive(returndata); //Get response
				responseData += System.Text.Encoding.ASCII.GetString(returndata, 0, returndata.Length);
				System.Threading.Thread.Sleep(100); //Wait for response
			}
			this.Close();

			if (!responseData.StartsWith("SOURCETABLE 200 OK"))
				return null;
			
			SourceTable result = new SourceTable();
			foreach (string line in responseData.Split('\n'))
			{
				if(line.StartsWith("STR"))
					result.DataStreams.Add(NTRIP.SourceTable.NTRIPDataStream.ParseFromString(line));
				else if (line.StartsWith("CAS"))
					result.Casters.Add(NTRIP.SourceTable.NTRIPCaster.ParseFromString(line));
				else if (line.StartsWith("NET"))
					result.Networks.Add(NTRIP.SourceTable.NTRIPNetwork.ParseFromString(line));
			}
			return result;
		}

		/// <summary>
		/// Opens the connection to the NTRIP server and starts receiving
		/// </summary>
		/// <param name="MountPoint">ID of Stream</param>
		public void StartNTRIP(string MountPoint)
		{
			this.InitializeSocket();
			sckt.Blocking = true;
			this.Connect();
			sckt.Send(CreateRequest(MountPoint));
			sckt.Blocking = false;
			WaitForData(sckt);
		}

		public void WaitForData(Socket sock)
		{
			//if (pfnCallBack == null)
			//	pfnCallBack = new AsyncCallback(OnDataReceived);
			AsyncCallback recieveData = new AsyncCallback(OnDataReceived);
			sock.BeginReceive(rtcmDataBuffer, 0, rtcmDataBuffer.Length, SocketFlags.None,
				recieveData, sock);
			//m_asynResult = sckt.BeginReceive(rtcmDataBuffer, 0, rtcmDataBuffer.Length, SocketFlags.None, pfnCallBack, null);
		}

		private void OnDataReceived(IAsyncResult ar)
		{
			Socket sock = (Socket)ar.AsyncState;
			int iRx = sock.EndReceive(ar);
			if (iRx > 0) //if we received at least one byte
			{
				try
				{
					//if (gps.GpsPort.IsPortOpen)
					//	//Send RTCM data to GPS. We assume the data is valid RTCM
					//	gps.GpsPort.Write(rtcmDataBuffer);
				}
				catch (System.Exception ex)
				{
					this.Close();
					throw (new System.Exception("Error sending RTCM data to device:" + ex.Message));
				}
			}
			WaitForData(sock);
		}

		/// <summary>
		/// Stops receiving data from the NTRIP server
		/// </summary>
		private void StopNTRIP()
		{
			this.Close();
		}

		/// <summary>
		/// Apply AsciiEncoding to user name and password to obtain it as an array of bytes
		/// </summary>
		/// <param name="str">username:password</param>
		/// <returns>Base64 encoded username/password</returns>
		private string ToBase64(string str)
		{
			Encoding asciiEncoding = Encoding.ASCII;
			byte[] byteArray = new byte[asciiEncoding.GetByteCount(str)];
			byteArray = asciiEncoding.GetBytes(str);
			return Convert.ToBase64String(byteArray, 0, byteArray.Length);
		}

		#region IDisposable Members

		public void Dispose()
		{
			this.Close();
		}

		#endregion
}
}
