using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomPacketHandler
{
	public static void ExitRoom(PacketReader _reader)
	{
		SceneManagerEx.Inst.LoadScene(Define.Scene.Lobby);
	}
}