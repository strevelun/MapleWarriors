using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

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
			.Add((byte)_dir);
		return pkt;
	}

	public static Packet EndMove(float _xpos, float _ypos)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.EndMove)
			//.Add(_tick);
			.Add((int)(_xpos * 1000000)) // 소수점 6자리 정밀도
			.Add((int)(_ypos * 1000000));
		return pkt;
	}

	// TODO : remove _pathIDx
	public static Packet BeginMoveMonster(string _name, int _cellXPos, int _cellYPos, int _pathIdx)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.BeginMoveMonster)
			.Add(_name)
			.Add((ushort)_pathIdx)
			.Add((ushort)_cellXPos)
			.Add((ushort)_cellYPos);
		return pkt;
	}

	public static Packet Attack(List<MonsterController> _targets, eSkill _skill) // 매개변수 : Attack 번호
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.Attack)
			.Add((ushort)_targets.Count);

		foreach(MonsterController mc in _targets)
		{
			pkt.Add(mc.name);
		}

		pkt.Add((byte)_skill);

		return pkt;
	}
}
