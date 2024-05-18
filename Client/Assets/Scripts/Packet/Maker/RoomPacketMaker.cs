using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomPacketMaker
{
	public static Packet ExitRoom()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ClientPacketTypeEnum.ExitRoom);
		return pkt;
	}

	public static Packet StartGame()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ClientPacketTypeEnum.StartGame);
		return pkt;
	}

	public static Packet RoomReady()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ClientPacketTypeEnum.RoomReady);
		return pkt;
	}

	public static Packet RoomStandby()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ClientPacketTypeEnum.RoomStandby);
		return pkt;
	}

	public static Packet SendChat(string _text)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.ClientPacketTypeEnum.RoomChat)
			.Add(_text);
		return packet;
	}

	public static Packet ReqRoomUsersInfo()
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.ClientPacketTypeEnum.ReqRoomUsersInfo);
		return packet;
	}

	public static Packet RoomMapChoice(int _mapID)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.ClientPacketTypeEnum.RoomMapChoice)
			.Add((byte)_mapID);
		return packet;
	}

	public static Packet RoomCharacterChoice(int _idx)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.ClientPacketTypeEnum.RoomCharacterChoice)
			.Add((byte)_idx);
		return packet;
	}
}
