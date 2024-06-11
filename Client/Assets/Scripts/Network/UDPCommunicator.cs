﻿using System;
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

	private Socket m_socket;
	private SocketAsyncEventArgs m_recvArgs;
	private UDPBuffer m_udpBuffer;
	private bool m_isRecv;

	public Dictionary<int, IPEndPoint> DicSendInfo { get; private set; } = new Dictionary<int, IPEndPoint>();

	public bool Init()
	{
		try
		{
			m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}
		catch(SocketException _ex)
		{
			Debug.Log(_ex.ToString());
			return false;
		}

		GameObject udpBuffer = GameObject.Find("UDPBuffer");
		if (!udpBuffer) return false;

		m_udpBuffer = udpBuffer.GetComponent<UDPBuffer>();
		if (!m_udpBuffer) return false;

		m_recvArgs = new SocketAsyncEventArgs();
		m_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
		RegisterRecv();

		return true;
	}

	public void Start()
	{
		m_isRecv = true;
		m_udpBuffer.Active = true;
	}

	public void Send(Packet _pkt, string _ip, int _port)
	{
		m_socket.SendTo(_pkt.GetBuffer(), new IPEndPoint(IPAddress.Parse(_ip), _port));
	}

	public void Send(byte[] _pkt, string _ip, int _port)
	{
		m_socket.SendTo(_pkt, new IPEndPoint(IPAddress.Parse(_ip), _port));
		Debug.Log((m_socket.LocalEndPoint as IPEndPoint).Port);
	}

	public void Send(Packet _pkt, int _slot)
	{
		if (!DicSendInfo.TryGetValue(_slot, out IPEndPoint ep)) return;

		m_socket.SendTo(_pkt.GetBuffer(), 0, _pkt.Size, SocketFlags.None, ep);
		//InGameConsole.Inst.Log($"[{_pkt.GetPacketType()}] {ep.Address}, {ep.Port}로 보냄 : {sendbyte}");
	}

	public void SendAll(Packet _pkt)
	{
		foreach (IPEndPoint ep in DicSendInfo.Values)
		{
			m_socket.SendTo(_pkt.GetBuffer(), 0, _pkt.Size, SocketFlags.None, ep);
			//InGameConsole.Inst.Log($"[{_pkt.GetPacketType()}] {ep.Address}, {ep.Port}로 보냄 : {sendbyte}");
		}
	}

	public void RegisterRecv()
	{
		m_udpBuffer.SetWriteSegment(out ArraySegment<byte> seg);

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
		//InGameConsole.Inst.Log($"AddSendInfo : {DicSendInfo.Count}");
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
		ClearIngameInfo();
		m_recvArgs.Completed -= new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
		m_recvArgs.Dispose();
		m_recvArgs = null;
		m_socket.Close();
		m_socket = null;
	}
}
