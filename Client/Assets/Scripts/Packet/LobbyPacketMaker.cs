using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LobbyPacketMaker
{
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
			.Add(UserData.Inst.Nickname)
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
}
