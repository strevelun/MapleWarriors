using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;

public class NetworkManager
{
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
			Managers.UI.ShowPopupUI(Define.UIPopup.UIConnectFailPopup, "서버와 연결할 수 없습니다.\n(오류코드 : " + e.SocketErrorCode + ")");
		}
	}

	public void Send(Packet _packet)
	{
		m_connection?.Send(_packet);
	}
}
