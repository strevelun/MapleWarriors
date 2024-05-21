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

	public Connection MyConnection { get; private set; } = null;
	private bool m_shutdown = false;

	public bool Init(string _serverIp, int _port)
	{
		if (Connect(_serverIp, _port) == false) return false;

		return true;
	}

	private bool Connect(string _serverIp, int _port)
	{
		IPAddress ip = IPAddress.Parse(_serverIp);
		IPEndPoint endPoint = new IPEndPoint(ip, _port);

		Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

		try
		{
			socket.Connect(endPoint);
			MyConnection = new Connection(socket)
			{
				LocalEndPoint = socket.LocalEndPoint as IPEndPoint
			};
		} 
		catch (SocketException e)
		{
			UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UIConnectFailPopup, "������ ������ �� �����ϴ�.\n(�����ڵ� : " + e.SocketErrorCode + ")");
			return false;
		}
		return true;
	}

	public void Send(Packet _packet)
	{
		MyConnection?.Send(_packet);

		//Debug.Log($"send : {(DateTime.Now.Ticks - before) / 10000000.0}��");
	}

	public void Disconnect()
	{
		if (m_shutdown) return;

		MyConnection?.Shutdown();
		m_shutdown = true;
	}
}
