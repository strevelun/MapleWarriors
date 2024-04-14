using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

public class UDPCommunicator : MonoBehaviour
{
	private static UDPCommunicator s_inst = null;
	public static UDPCommunicator Inst { get { return s_inst; } }

	UdpClient m_udpClient;
	PacketReader m_reader = new PacketReader();

	public struct tSendInfo
	{
		public string ip;
		public int port;
	}

	public Dictionary<int, tSendInfo> DicSendInfo { get; } = new Dictionary<int, tSendInfo>();

	private void Awake()
	{
		s_inst = GetComponent<UDPCommunicator>();
	}

	void Start()
    {
	}

    void Update()
    {
        
    }

	public void Init(int _port)
	{
		m_udpClient = new UdpClient(_port);
		StartReceive();
	}

	async void StartReceive()
	{
		await ReceiveUDP();
	}

	private void OnDestroy()
	{
		m_udpClient?.Close();
		s_inst = null;
	}

	public void Send(Packet _pkt, int _slot)
	{
		tSendInfo info;
		if (!DicSendInfo.TryGetValue(_slot, out info)) return;

		int sendbyte = m_udpClient.Send(_pkt.GetBuffer(), _pkt.Size, info.ip, info.port);
		Debug.Log($"{info.ip}, {info.port}로 보냄 : {sendbyte}");
	}

	public void SendAll(Packet _pkt)
	{
		foreach (tSendInfo info in DicSendInfo.Values)
		{
			int sendbyte = m_udpClient.Send(_pkt.GetBuffer(), _pkt.Size, info.ip, info.port);
			Debug.Log($"{info.ip}, {info.port}로 보냄 : {sendbyte}");
		}
	}

	private async Task ReceiveUDP()
	{
		while (true)
		{
			try
			{
				var result = await m_udpClient.ReceiveAsync();
				m_reader.SetBuffer(result.Buffer);
				PacketHandler.Handle(m_reader);
				//Debug.Log("result");
			} 
			catch(ObjectDisposedException)
			{
				break;
			}
		}
		Debug.Log("its damned");
	}

	public void AddSendInfo(int _slot, string _ip, int _port)
	{
		tSendInfo info = new tSendInfo();
		info.ip = _ip;
		info.port = _port;
		DicSendInfo.Add(_slot, info);
	}

	public void RemoveSendInfo(int _slot)
	{
		DicSendInfo.Remove(_slot);
	}
}
