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

	public static Packet BeginMove(Vector3 _vecStartPos, byte _byteDir)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.BeginMove)
			.Add((int)(_vecStartPos.x * 1000000))
			.Add((int)(_vecStartPos.y * 1000000))
			.Add(_byteDir);
		return pkt;
	}

	public static Packet EndMove(Vector3 _vecEndPos)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.EndMove)
			.Add((int)(_vecEndPos.x * 1000000)) // �Ҽ��� 6�ڸ� ���е�
			.Add((int)(_vecEndPos.y * 1000000));
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

	public static Packet Attack(List<MonsterController> _targets, eSkill _skill) // �Ű����� : Attack ��ȣ
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
	
	public static Packet RangedAttack(List<MonsterController> _targets, eSkill _skill, Vector2Int _where) // �Ű����� : Attack ��ȣ
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.RangedAttack)
			.Add((ushort)_targets.Count)
			.Add((short)_where.x) // ����
			.Add((short)_where.y); 

		foreach(MonsterController mc in _targets)
		{
			pkt.Add(mc.name);
		}

		pkt.Add((byte)_skill);

		return pkt;
	}

	public static Packet GameOver()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eClient.GameOver);
		return pkt;
	}
}
