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
	RingBuffer m_ringBuffer;

	public Dictionary<int, IPEndPoint> DicSendInfo { get; private set; } = new Dictionary<int, IPEndPoint>();

	public void Init(RingBuffer _ringBuffer, int _port)
	{
		m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		m_socket.Bind(new IPEndPoint(IPAddress.Any, _port));

		InGameConsole.Inst.Log($"{_port} 번호로 바인딩");

		m_ringBuffer = _ringBuffer;

		m_recvArgs = new SocketAsyncEventArgs();
		m_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
		RegisterRecv();
	}

	public void Send(Packet _pkt, int _slot)
	{
		//InGameConsole.Inst.Log("Send");
		IPEndPoint ep;
		if (!DicSendInfo.TryGetValue(_slot, out ep)) return;

		int sendbyte = m_socket.SendTo(_pkt.GetBuffer(), 0, _pkt.Size, SocketFlags.None, ep);
		//InGameConsole.Inst.Log($"[{_pkt.GetPacketType()}] {ep.Address}, {ep.Port}로 보냄 : {sendbyte}");
	}

	public void SendAll(Packet _pkt)
	{
		//InGameConsole.Inst.Log($"SendAll : {DicSendInfo.Count}");
		foreach (IPEndPoint ep in DicSendInfo.Values)
		{
			int sendbyte = m_socket.SendTo(_pkt.GetBuffer(), 0, _pkt.Size, SocketFlags.None, ep);
			InGameConsole.Inst.Log($"[{_pkt.GetPacketType()}] {ep.Address}, {ep.Port}로 보냄 : {sendbyte}");
		}
	}

	public void RegisterRecv()
	{
		ArraySegment<byte> seg;
		if (!m_ringBuffer.SetWriteSegment(out seg))
		{
			Debug.Log("버퍼에 공간이 없습니다");
			return;
		}

		m_recvArgs.SetBuffer(seg.Array, seg.Offset, seg.Count);

		Debug.Log("RegisterRecv");

		bool pending = m_socket.ReceiveAsync(m_recvArgs);
		if (!pending) OnRecvCompleted(null, m_recvArgs);
	}

	public void OnRecvCompleted(object _sender, SocketAsyncEventArgs _args)
	{
		if(_args.SocketError == SocketError.ConnectionReset)
		{
			InGameConsole.Inst.Log($"ConnectionReset : {_args.BytesTransferred}");
			RegisterRecv();

			return;
		}

		if (_args.BytesTransferred == 0 || _args.SocketError != SocketError.Success)
		{
			InGameConsole.Inst.Log($"Error : {_args.SocketError}");
			//Disconnect();
			RegisterRecv();
			return;
		}

		m_ringBuffer.MoveWritePos(_args.BytesTransferred);

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

	public void Disconnect()
	{
		Debug.Log("Disconnect");
		m_socket.Close();
		DicSendInfo.Clear();
		m_recvArgs = null;
		m_socket = null;
		m_ringBuffer = null;
	}
}
