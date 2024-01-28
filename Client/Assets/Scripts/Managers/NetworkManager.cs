using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

	public void Connect(string _serverIp, int _port)
	{
		IPAddress ip = IPAddress.Parse(_serverIp);
		IPEndPoint endPoint = new IPEndPoint(ip, _port);

		Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		
		try
		{
			socket.Connect(endPoint);
			m_connection = new Connection(socket);
		} catch(SocketException e)
		{
			UIManager.Inst.ShowPopupUI(Define.UIPopup.UIConnectFailPopup, "서버와 연결할 수 없습니다.\n(오류코드 : " + e.SocketErrorCode + ")");
		}
	}

	public void Send(Packet _packet)
	{
		m_connection?.Send(_packet);
	}
	
	public void Disconnect()
	{
		m_connection.Disconnect();
		m_connection = null;
	}
}
