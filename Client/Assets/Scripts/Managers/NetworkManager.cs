using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Windows;

public class NetworkManager
{
	private static NetworkManager s_inst = null;
	public static NetworkManager Inst
	{
		get
		{
			if (s_inst == null) s_inst = new NetworkManager();
			return s_inst;
		}
	}

	Connection m_connection = null;

	public bool Init(string _serverIp, int _port)
	{
		if (Connect(_serverIp, _port) == false) return false;

		return true;
	}

	bool Connect(string _serverIp, int _port)
	{
		IPAddress ip = IPAddress.Parse(_serverIp);
		IPEndPoint endPoint = new IPEndPoint(ip, _port);

		Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		
		try
		{
			socket.Connect(endPoint);
			m_connection = new Connection(socket);
		} 
		catch (SocketException e)
		{
			UIManager.Inst.ShowPopupUI(Define.eUIPopup.UIConnectFailPopup, "서버와 연결할 수 없습니다.\n(오류코드 : " + e.SocketErrorCode + ")");
			return false;
		}
		return true;
	}

	public void Send(Packet _packet)
	{
		m_connection?.Send(_packet);

		//Debug.Log($"send : {(DateTime.Now.Ticks - before) / 10000000.0}초");
	}

	public void Disconnect()
	{
		m_connection?.Shutdown();
		m_connection = null;
	}
}
