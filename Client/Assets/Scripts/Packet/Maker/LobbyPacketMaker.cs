using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LobbyPacketMaker
{
	public static Packet Test()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ClientPacketTypeEnum.Test);
		return pkt;
	}

	public static Packet SendChat(string _text)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.ClientPacketTypeEnum.LobbyChat)
			.Add(_text);
		return packet;
	}

	public static Packet UpdateLobbyInfo(byte _userListPage, byte _roomListPage)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ClientPacketTypeEnum.LobbyUpdateInfo)
			.Add(_userListPage)
			.Add(_roomListPage);
		return pkt;
	}

	public static Packet UserListGetPageInfo(int _page)
	{
		Packet pkt = new Packet();
		pkt.Add(PacketType.ClientPacketTypeEnum.UserListGetPageInfo)
			.Add((byte)_page);
		return pkt;
	}

	public static Packet RoomListGetPageInfo(int _page)
	{
		Packet pkt = new Packet();
		pkt.Add(PacketType.ClientPacketTypeEnum.RoomListGetPageInfo)
			.Add((byte)_page);
		return pkt;
	}

	public static Packet CreateRoom(string _title)
	{
		Packet pkt = new Packet();
		pkt.Add(PacketType.ClientPacketTypeEnum.CreateRoom)
			.Add(_title);
		return pkt;
	}

	public static Packet EnterRoom(int _id)
	{
		Packet pkt = new Packet();
		pkt.Add(PacketType.ClientPacketTypeEnum.EnterRoom)
			.Add((ushort)_id);
		return pkt;
	}
}
