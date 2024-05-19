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
			.Add(PacketType.ClientPacketTypeEnum.ReqInitInfo);
		return pkt;
	}

	public static Packet BeginMove(byte _byteDir)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.BeginMove)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add(_byteDir);

		return pkt;
	}

	public static Packet Moving(Vector3 _vecStartPos, byte _byteDir)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.Moving)
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
			.Add(PacketType.ServerPacketTypeEnum.EndMove)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((int)(_vecEndPos.x * 1000000)) // 소수점 6자리 정밀도
			.Add((int)(_vecEndPos.y * 1000000));
		return pkt;
	}

	public static Packet MonsterAttack(List<PlayerController> _finalTargets, int _monsterIdx, int _monsterNum)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.MonsterAttack)
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
			.Add(PacketType.ServerPacketTypeEnum.BeginMoveMonster)
			.Add((byte)_monsterIdx)
			.Add((byte)_monsterNum)
			.Add((ushort)_cellXPos)
			.Add((ushort)_cellYPos);
		return pkt;
	}

	public static Packet Attack(byte _who, List<MonsterController> _targets, SkillEnum _skill) // 매개변수 : Attack 번호
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.Attack)
			.Add((byte)_who)
			.Add((ushort)_targets.Count)
			.Add((byte)_skill);

		foreach (MonsterController mc in _targets)
		{
			pkt
				.Add((byte)mc.Idx)
				.Add((byte)mc.Num)
				.Add((ushort)mc.HP); // 동시 타격시 순서보장 x
		}

		return pkt;
	}	

	public static Packet AttackReq(Vector2Int _mouseCellPos, SkillEnum _skill) // 매개변수 : Attack 번호
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.AttackReq)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((short)_mouseCellPos.x)
			.Add((short)_mouseCellPos.y)
			.Add((byte)_skill);

		return pkt;
	}	
	
	public static Packet RangedAttack(byte _who, List<MonsterController> _targets, SkillEnum _skill, Vector2Int _where) // 매개변수 : Attack 번호
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.RangedAttack)
			.Add(_who)
			.Add((ushort)_targets.Count)
			.Add((short)_where.x) // 음수
			.Add((short)_where.y)
			.Add((byte)_skill);

		foreach (MonsterController mc in _targets)
		{
			pkt
				.Add((byte)mc.Idx)
				.Add((byte)mc.Num)
				.Add((ushort)mc.HP);
		}

		return pkt;
	}
	
	public static Packet RangedAttackReq(Vector2Int _mouseCellPos, SkillEnum _skill, Vector2Int _where)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.RangedAttackReq)
			.Add((byte)UserData.Inst.MyRoomSlot)
			.Add((short)_mouseCellPos.x)
			.Add((short)_mouseCellPos.y)
			.Add((short)_where.x) 
			.Add((short)_where.y)
			.Add((byte)_skill);

		return pkt;
	}

	public static Packet GameOver()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ClientPacketTypeEnum.GameOver);
		return pkt;
	}

	public static Packet AllMonstersInfo()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.AllCreaturesInfo)
			.Add((ushort)ObjectManager.Inst.Monsters.Count);

		foreach (MonsterController info in ObjectManager.Inst.Monsters.Values)
		{
			pkt
				.Add((byte)info.Idx)
				.Add((byte)info.Num)
				.Add((ushort)info.HP);
		}

		pkt.Add((ushort)ObjectManager.Inst.Players.Count);

		foreach (PlayerController info in ObjectManager.Inst.Players.Values)
		{
			pkt
				.Add((byte)info.Idx)
				.Add((ushort)info.HP);
		}

		return pkt;
	}

	public static Packet Ready()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.Ready)
			.Add((byte)UserData.Inst.MyRoomSlot);
		return pkt;
	}

	public static Packet Start(int _curTime, int _startTime)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.Start)
			.Add(_startTime)
			.Add(_curTime);
		return pkt;
	}

	public static Packet NextStage()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.NextStage);
		return pkt;
	}

	public static Packet MapClear()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.MapClear);
		return pkt;
	}

	public static Packet StageClear()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.StageClear);
		return pkt;
	}

	public static Packet Annihilated()
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.Annihilated);
		return pkt;
	}

	public static Packet PlayerHit(MonsterController _mc, List<PlayerController> _targets, List<bool> _targetHit)
	{
		Packet pkt = new Packet();
		pkt
			.Add(PacketType.ServerPacketTypeEnum.PlayerHit)
			.Add((byte)_targets.Count)
			.Add((byte)_mc.Idx)
			.Add((byte)_mc.Num);

		int idx = 0;
		foreach (PlayerController pc in _targets)
		{
			pkt.Add((byte)pc.Idx);
			pkt.Add((byte)(_targetHit[idx++] ? 1 : 0));
		}
		
		return pkt;
	}
}
