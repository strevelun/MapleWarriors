using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public static class InGamePacketHandler
{
	public static void ResInitInfo(PacketReader _reader)
	{
		// 몬스터 컨트롤러에서 타일맵 데이터에 접근할 수 있어야 함. (충돌 감지 및 경로 생성)
		//long seed = _reader.GetInt64();
		int mapID = _reader.GetByte();
		int numOfUsers = _reader.GetByte();
		GameObject player;
		GameObject camObj = GameObject.Find("CM vcam1");
		GameObject udpBufferObj = GameObject.Find("IngameUDPBuffer");
		UDPBuffer udpBuffer = null;
		if (udpBufferObj) udpBuffer = udpBufferObj.GetComponent<UDPBuffer>();
		CinemachineVirtualCamera vcam1 = camObj.GetComponent<CinemachineVirtualCamera>();

		GameObject mapObj = MapManager.Inst.Load(mapID, camObj); // TestMap : 1
		GameManager.Inst.SetPlayerCnt(numOfUsers);
		GameManager.Inst.SetPlayerAliveCnt(numOfUsers);

		GameObject connections = UIManager.Inst.FindUI(Define.eUI.Connections);
		TextMeshProUGUI tmp = connections.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		tmp.text = string.Empty;

		GameObject monsters = Util.FindChild(mapObj, false, "Monsters");
		int activeCnt = 0;
		for (int i = 0; i < monsters.transform.childCount; i++)
		{
			if (monsters.transform.GetChild(i).gameObject.activeSelf)
			{
				++activeCnt;
			}
		}
		GameManager.Inst.SetMonsterCnt(activeCnt);

		List<int> idxList = new List<int>();
		MyPlayerController mpc = null;

		for (int i = 0; i < numOfUsers; ++i)
		{
			int connectionID = _reader.GetUShort();
			int idx = _reader.GetByte();
			string nickname = _reader.GetString();
			byte characterChoice = _reader.GetByte();
			ushort port = _reader.GetUShort();
			string ip = "";
			for (int j = 0; j < 4; ++j)
			{
				ip += _reader.GetByte();
				if (j < 3) ip += ".";
			}

			tmp.text += $"{nickname} [{ip}, {port}]\n";

			player = ResourceManager.Inst.Instantiate($"Creature/Player_{characterChoice}"); // 플레이어 선택
			player.name = $"Player_{characterChoice}_{idx}"; // slot 넘버로

			GameManager.Inst.AddPlayer(player);

			if (connectionID == UserData.Inst.ConnectionID)
			{
				mpc = player.AddComponent<MyPlayerController>();
				mpc.Init(i + 1, 1);
				mpc.Idx = idx;
				mpc.SetNickname(nickname);
				vcam1.Follow = player.transform;
				UDPCommunicator.Inst.Init(udpBuffer, port);
			}
			else
			{
				PlayerController pc = player.AddComponent<PlayerController>();
				pc.Init(i + 1, 1);
				pc.Idx = idx;
				idxList.Add(idx);
				pc.SetNickname(nickname);

				UDPCommunicator.Inst.AddSendInfo(idx, ip, port);
				InGameConsole.Inst.Log($"{nickname}, port : {port}, ip : {ip}");
			}
			ObjectManager.Inst.AddPlayer(idx, player);
		}

		InGameConsole.Inst.Log($"플레이어 수 : {GameManager.Inst.PlayerCnt}");
		InGameConsole.Inst.Log($"몬스터 수 : {GameManager.Inst.MonsterCnt}");
		InGameConsole.Inst.Log($"스테이지 수 : {MapManager.Inst.MaxStage}");

		GameManager.Inst.SetOtherPlayerSlot(idxList);
		//GameManager.Inst.GameStart = true;
	}

	public static void BeginMove(PacketReader _reader)
	{
		//InGameConsole.Inst.Log("BeginMove");

		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;
		byte dir = _reader.GetByte();

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		pc.BeginMovePosition(xpos, ypos, dir);
	}

	public static void Moving(PacketReader _reader)
	{
		//long time = _reader.GetInt64();
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;
		byte byteDir = _reader.GetByte();

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		pc.Moving(xpos, ypos, byteDir);
	}

	public static void EndMove(PacketReader _reader)
	{
		//InGameConsole.Inst.Log("EndMove");
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		pc.EndMovePosition(xpos, ypos);
	}

	public static void BeginMoveMonster(PacketReader _reader)
	{
		byte monsterIdx = _reader.GetByte();
		byte monsterNum = _reader.GetByte();
		int destCellXPos = _reader.GetUShort();
		int destCellYPos = _reader.GetUShort();

		MonsterController mc = ObjectManager.Inst.FindMonster(monsterIdx, monsterNum);
		if (!mc)
		{
			Debug.Log("몬스터 이름 찾을 수 없음");
			return;
		}

		mc.ReserveBeginMove(destCellXPos, destCellYPos);
	}

	public static void MonsterAttack(PacketReader _reader)
	{
		int targetCnt = _reader.GetByte();

		List<PlayerController> targets = new List<PlayerController>();
		for (int i = 0; i < targetCnt; ++i)
		{
			PlayerController pc = ObjectManager.Inst.FindPlayer(_reader.GetByte());
			targets.Add(pc);
		}

		MonsterController mc = ObjectManager.Inst.FindMonster(_reader.GetByte(), _reader.GetByte());
		mc.StartFlyingAttackCoroutine(targets);
		mc.StartRangedAttackCoroutine(targets);
		if (mc) mc.ChangeState(new MonsterAttackState(targets));
	}

	public static void InGameExit(PacketReader _reader)
	{
		byte leftUserIdx = _reader.GetByte();
		InGameConsole.Inst.Log($"InGameExit : {leftUserIdx}");
		byte nextRoomOwnerIdx = _reader.GetByte();

		if (UserData.Inst.MyRoomSlot == nextRoomOwnerIdx)
		{
			UserData.Inst.IsRoomOwner = true;
		}

		if(nextRoomOwnerIdx < Define.RoomUserSlot)
			InGameConsole.Inst.Log($"방장이 바뀌었습니다 : {leftUserIdx} -> {nextRoomOwnerIdx}");

		UserData.Inst.RoomOwnerSlot = nextRoomOwnerIdx;
		GameManager.Inst.SubPlayerCnt();
		GameManager.Inst.RemoveOtherPlayerSlot(leftUserIdx);

		PlayerController pc = ObjectManager.Inst.FindPlayer(leftUserIdx);
		GameManager.Inst.RemovePlayer(pc.name);
		MapManager.Inst.RemoveAimTile(pc.CellPos.x, pc.CellPos.y);
		ResourceManager.Inst.Destroy(pc.gameObject);
		ObjectManager.Inst.RemovePlayer(leftUserIdx);

		UDPCommunicator.Inst.RemoveSendInfo(leftUserIdx);
	}

	public static void Attack(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		ushort count = _reader.GetUShort();
		eSkill skill = (eSkill)_reader.GetByte();

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		pc.CurSkill.SetSkill(skill);

		List<MonsterController> targets = new List<MonsterController>();
		MonsterController mc;
		for (int i = 0; i < count; ++i)
		{
			mc = ObjectManager.Inst.FindMonster(_reader.GetByte(), _reader.GetByte());
			targets.Add(mc);
			ushort hp = _reader.GetUShort();
			mc.Hit(hp);
		}

		if (roomSlot != UserData.Inst.MyRoomSlot)
			pc.ChangeState(new PlayerAttackState(pc.CurSkill));
	}

	public static void AttackReq(PacketReader _reader) // 방장이 받는다.
	{
		byte roomSlot = _reader.GetByte();
		short mouseCellPosX = _reader.GetShort();
		short mouseCellPosY = _reader.GetShort();
		eSkill skill = (eSkill)_reader.GetByte();

		Debug.Log($"{roomSlot}이 공격함");
		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		pc.CurSkill.SetSkill(skill);
		pc.CurSkill.SetDist(new Vector2Int(mouseCellPosX, mouseCellPosY), pc.CellPos, pc.LastDir);
		pc.Attack(roomSlot, 0, 0);
	}

	public static void RangedAttack(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		ushort count = _reader.GetUShort();
		short x = _reader.GetShort();
		short y = _reader.GetShort();
		eSkill skill = (eSkill)_reader.GetByte();

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		pc.CurSkill.SetSkill(skill);

		List<MonsterController> targets = new List<MonsterController>();
		MonsterController mc;
		for (int i = 0; i < count; ++i)
		{
			mc = ObjectManager.Inst.FindMonster(_reader.GetByte(), _reader.GetByte());
			targets.Add(mc);
			ushort hp = _reader.GetUShort();
			mc.Hit(hp);
		}

		if (roomSlot != UserData.Inst.MyRoomSlot)
		{
			pc.ChangeState(new PlayerAttackState(pc.CurSkill));
			pc.SetRangedSkillObjPos(new Vector2Int(x, y));
		}
	}

	public static void RangedAttackReq(PacketReader _reader)  // 방장이 받는다.
	{
		byte roomSlot = _reader.GetByte();
		short mouseCellPosX = _reader.GetShort();
		short mouseCellPosY = _reader.GetShort();
		short x = _reader.GetShort();
		short y = _reader.GetShort();

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);

		eSkill skill = (eSkill)_reader.GetByte();
		pc.CurSkill.SetSkill(skill);
		pc.CurSkill.SetDist(new Vector2Int(mouseCellPosX, mouseCellPosY), pc.CellPos, pc.LastDir);
		pc.Attack(roomSlot, x, y);
	}

	public static void GameOver(PacketReader _reader)
	{
		InGameConsole.Inst.Log("GameOver");
		GameManager.Inst.Clear();
		ObjectManager.Inst.ClearPlayers();
		ObjectManager.Inst.ClearMonsters();
		MapManager.Inst.Destroy();
		SceneManagerEx.Inst.LoadSceneWithFadeOut(eScene.Room);
	}

	//static int cnt = 0;

	public static void Awake(PacketReader _reader)
	{
		int slot = _reader.GetByte();

		//InGameConsole.Inst.Log($"Awake : {cnt++}");

		GameManager.Inst.Awake(slot);
	}

	public static void AllMonstersInfo(PacketReader _reader)
	{
		ushort count = _reader.GetUShort();
		MonsterController mc;
		int idx, num;

		for (int i = 0; i < count; ++i)
		{
			idx = _reader.GetByte();
			num = _reader.GetByte();
			mc = ObjectManager.Inst.FindMonster(idx, num);
			ushort hp = _reader.GetUShort();
			mc.Hit(hp);

			//InGameConsole.Inst.Log($"{idx}_{num} : HP[{hp}], IsDead[{mc.IsDead}]");
		}

		ushort playerCnt = _reader.GetUShort();
		PlayerController pc;

		for (int i = 0; i < playerCnt; ++i)
		{
			idx = _reader.GetByte();
			pc = ObjectManager.Inst.FindPlayer(idx);
			ushort hp = _reader.GetUShort();
			pc.HP = hp;
			// Run에서 바로 Dead가 됨.
			//if (pc.IsDead) pc.ChangeState(new PlayerDeadState());
		}
	}

	public static void Ready(PacketReader _reader)
	{
		int slot = _reader.GetByte();

		GameManager.Inst.SetPlayerReady(slot);
		InGameConsole.Inst.Log($"READY");
	}

	public static void Start(PacketReader _reader)
	{
		long startTime = _reader.GetInt64();

		GameManager.Inst.SetStartTime(startTime);
		InGameConsole.Inst.Log($"Start");
	}

	public static void NextStage(PacketReader _reader)
	{
		GameManager.Inst.OnChangeStage();
	}

	public static void MapClear(PacketReader _reader)
	{
		InGameScene scene = SceneManagerEx.Inst.CurScene as InGameScene;
		scene.OnMapClear();
	}

	public static void StageClear(PacketReader _reader)
	{
		InGameScene scene = SceneManagerEx.Inst.CurScene as InGameScene;
		scene.OnStageClear();
	}

	public static void Annihilated(PacketReader _reader)
	{
		InGameScene scene = SceneManagerEx.Inst.CurScene as InGameScene;
		scene.OnAnnihilated();
	}
}
