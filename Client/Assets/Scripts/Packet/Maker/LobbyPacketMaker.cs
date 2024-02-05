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
			.Add(PacketType.eClient.Test)
			.Add(DateTime.Now.Ticks);
		return pkt;
	}

	public static Packet ExitGame()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.Exit);
		return pkt;
	}

	public static Packet SendChat(string _text)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.eClient.LobbyChat)
			.Add(_text);
		return packet;
	}

	public static Packet UpdateLobbyInfo(byte _userListPage, byte _roomListPage)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.LobbyUpdateInfo)
			.Add(_userListPage)
			.Add(_roomListPage);
		return pkt;
	}

	public static Packet UserListGetPageInfo(int _page)
	{
		Packet pkt = new Packet();
		pkt.Add(PacketType.eClient.UserListGetPageInfo)
			.Add((byte)_page);
		return pkt;
	}

	public static Packet RoomListGetPageInfo(int _page)
	{
		Packet pkt = new Packet();
		pkt.Add(PacketType.eClient.RoomListGetPageInfo)
			.Add((byte)_page);
		return pkt;
	}

	public static Packet CreateRoom(string _title)
	{
		Packet pkt = new Packet();
		pkt.Add(PacketType.eClient.CreateRoom)
			.Add(_title);
		return pkt;
	}

	public static Packet EnterRoom(uint _id)
	{
		Packet pkt = new Packet();
		pkt.Add(PacketType.eClient.EnterRoom)
			.Add(_id);
		return pkt;
	}
}
