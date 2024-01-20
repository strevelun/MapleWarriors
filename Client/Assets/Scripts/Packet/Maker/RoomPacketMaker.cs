using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomPacketMaker
{
	public static Packet ExitRoom()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.ExitRoom);
		return pkt;
	}

	public static Packet StartGame()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.StartGame);
		return pkt;
	}

	public static Packet RoomReady()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.RoomReady);
		return pkt;
	}

	public static Packet RoomStandby()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.RoomStandby);
		return pkt;
	}

	public static Packet SendChat(string _text)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.eClient.RoomChat)
			.Add(_text);
		return packet;
	}
}
