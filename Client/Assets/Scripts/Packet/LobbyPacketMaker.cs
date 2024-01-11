using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LobbyPacketMaker
{
	public static Packet SendChat(string _text)
	{
		Packet packet = new Packet();
		packet
			.Add(PacketType.Client.LobbyChat)
			.Add(_text);
		return packet;
	}
}
