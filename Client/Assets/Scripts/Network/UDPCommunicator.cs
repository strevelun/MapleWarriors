using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UDPCommunicator
{
	private static UDPCommunicator s_inst = null;
	public static UDPCommunicator Inst
	{
		get
		{
			if (s_inst == null) s_inst = new UDPCommunicator();
			return s_inst;
		}
	}

	Socket m_socket;
	SocketAsyncEventArgs m_recvArgs;
	UDPBuffer m_udpBuffer;
	bool m_isRecv;

	public Dictionary<int, IPEndPoint> DicSendInfo { get; private set; } = new Dictionary<int, IPEndPoint>();

	public bool Init()
	{
		m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		m_socket.Bind(new IPEndPoint(IPAddress.Any, 0));

		GameObject udpBuffer = GameObject.Find("UDPBuffer");
		if (!udpBuffer) return false;

		m_udpBuffer = udpBuffer.GetComponent<UDPBuffer>();
		if (!m_udpBuffer) return false;


		m_recvArgs = new SocketAsyncEventArgs();
		m_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
		RegisterRecv();

		return true;
	}

	public int GetPort()
	{
		IPEndPoint localEndPoint = m_socket.LocalEndPoint as IPEndPoint;
		return localEndPoint.Port;
	}

	public void Start()
	{
		m_isRecv = true;
		m_udpBuffer.Active = true;
		//InGameConsole.Inst.Log($"현재 저의 포트는 [{(m_socket.LocalEndPoint as IPEndPoint).Port}]");
	}

	public void Send(Packet _pkt, int _slot)
	{
		//InGameConsole.Inst.Log("Send");
		IPEndPoint ep;
		if (!DicSendInfo.TryGetValue(_slot, out ep)) return;

		int sendbyte = m_socket.SendTo(_pkt.GetBuffer(), 0, _pkt.Size, SocketFlags.None, ep);
		//InGameConsole.Inst.Log(m_socket.LocalEndPoint.ToString());
		//InGameConsole.Inst.Log($"[{_pkt.GetPacketType()}] {ep.Address}, {ep.Port}로 보냄 : {sendbyte}");
	}

	public void SendAll(Packet _pkt)
	{
		//InGameConsole.Inst.Log($"SendAll : {DicSendInfo.Count}");
		foreach (IPEndPoint ep in DicSendInfo.Values)
		{
			int sendbyte = m_socket.SendTo(_pkt.GetBuffer(), 0, _pkt.Size, SocketFlags.None, ep);
		//	InGameConsole.Inst.Log(m_socket.LocalEndPoint.ToString());
			//InGameConsole.Inst.Log($"[{_pkt.GetPacketType()}] {ep.Address}, {ep.Port}로 보냄 : {sendbyte}");
		}
	}

	public void RegisterRecv()
	{
		ArraySegment<byte> seg;
		m_udpBuffer.SetWriteSegment(out seg);

		m_recvArgs.SetBuffer(seg.Array, seg.Offset, seg.Count);

		bool pending = m_socket.ReceiveAsync(m_recvArgs);
		if (!pending) OnRecvCompleted(null, m_recvArgs);
	}

	public void OnRecvCompleted(object _sender, SocketAsyncEventArgs _args)
	{
		Debug.Log($"OnRecvCompleted : {_args.BytesTransferred}");

		if (!m_isRecv)
		{
			RegisterRecv();
			return;
		}

		if(_args.SocketError == SocketError.ConnectionReset)
		{
			Debug.Log($"ConnectionReset : {_args.BytesTransferred}");
			RegisterRecv();
			return;
		}

		if(_args.SocketError == SocketError.MessageSize)
		{
			Debug.Log($"MessageSize : {_args.BytesTransferred}");
			return;
		}

		if (_args.SocketError != SocketError.Success)
		{
			Debug.Log($"Error : {_args.SocketError}");
			//Disconnect();
			RegisterRecv();
			return;
		}

		m_udpBuffer.MoveWritePos(_args.BytesTransferred);
		RegisterRecv();
	}

	public void AddSendInfo(int _slot, string _ip, int _port)
	{
		InGameConsole.Inst.Log($"AddSendInfo : {DicSendInfo.Count}");
		DicSendInfo.Add(_slot, new IPEndPoint(IPAddress.Parse(_ip), _port));
	}

	public void RemoveSendInfo(int _slot)
	{
		DicSendInfo.Remove(_slot);
	}

	public void ClearIngameInfo()
	{
		DicSendInfo.Clear();
		m_isRecv = false;
		m_udpBuffer.Active = false;
	}

	public void Disconnect()
	{
		InGameConsole.Inst?.Log("UDP Disconnect");
		ClearIngameInfo();
		m_recvArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
		m_recvArgs.Dispose();
		m_recvArgs = null;
		m_socket.Close();
		m_socket = null;
	}
}
