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
			.Add(PacketType.eServer.BeginMove) // Client�� �ٷ� ����
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((int)(_vecStartPos.x * 1000000))
			.Add((int)(_vecStartPos.y * 1000000))
			.Add(_byteDir);
		return pkt;
	}

	public static Packet Moving(Vector3 _vecStartPos, byte _byteDir)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eServer.Moving)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((int)(_vecStartPos.x * 1000000))
			.Add((int)(_vecStartPos.y * 1000000))
			.Add(_byteDir);
		return pkt;
	}

	public static Packet EndMove(Vector3 _vecEndPos)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eServer.EndMove)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((int)(_vecEndPos.x * 1000000)) // �Ҽ��� 6�ڸ� ���е�
			.Add((int)(_vecEndPos.y * 1000000));
		return pkt;
	}

	// ��Ŷ�� ���� �Ŀ� ���� AttackState�� ��ȯ
	public static Packet MonsterAttack(List<PlayerController> _finalTargets, int _monsterIdx, int _monsterNum)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eServer.MonsterAttack)
			.Add((byte)_finalTargets.Count);

		foreach(PlayerController pc in _finalTargets)
		{
			pkt.Add((byte)pc.Idx);
		}
		pkt
			.Add((byte)_monsterIdx)
			.Add((byte)_monsterNum);
		return pkt;
	}

	public static Packet BeginMoveMonster(int _monsterIdx, int _monsterNum, int _cellXPos, int _cellYPos)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eServer.BeginMoveMonster)
			.Add((byte)_monsterIdx)
			.Add((byte)_monsterNum)
			.Add((ushort)_cellXPos)
			.Add((ushort)_cellYPos);
		return pkt;
	}

	public static Packet Attack(List<MonsterController> _targets, eSkill _skill) // �Ű����� : Attack ��ȣ
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eServer.Attack)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((ushort)_targets.Count);

		foreach(MonsterController mc in _targets)
		{
			pkt
				.Add((byte)mc.Idx)
				.Add((byte)mc.Num);
		}

		pkt.Add((byte)_skill);

		return pkt;
	}	
	
	public static Packet RangedAttack(List<MonsterController> _targets, eSkill _skill, Vector2Int _where) // �Ű����� : Attack ��ȣ
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.eServer.RangedAttack)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((ushort)_targets.Count)
			.Add((short)_where.x) // ����
			.Add((short)_where.y); 

		foreach(MonsterController mc in _targets)
		{
			pkt
				.Add((byte)mc.Idx)
				.Add((byte)mc.Num);
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
