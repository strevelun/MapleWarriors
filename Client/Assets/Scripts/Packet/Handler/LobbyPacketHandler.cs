using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LobbyPacketHandler
{
	public static void LobbyChat(PacketReader _reader)
	{
		string chat = _reader.GetString();

		ActionQueue.Inst.Enqueue(() =>
		{
			UIManager.Inst.AddChat(Define.UIChat.UILobbyChat, chat);
		});
	}
}
