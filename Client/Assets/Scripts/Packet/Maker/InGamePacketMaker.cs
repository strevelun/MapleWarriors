using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InGamePacketMaker
{
	public static Packet ExitGame()
	{
		return null;
	}

	public static Packet ReqInitInfo()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.ReqInitInfo);
		return pkt;
	}

	public static Packet BeginMove(CreatureController.eDir _dir)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.BeginMove) 
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((byte)_dir);
		return pkt;
	}

	public static Packet EndMove(long _tick)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.EndMove)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add(_tick);
			//.Add((int)(_xpos * 1000000)) // 소수점 6자리 정밀도
			//.Add((int)(_ypos * 1000000));
		return pkt;
	}
}
