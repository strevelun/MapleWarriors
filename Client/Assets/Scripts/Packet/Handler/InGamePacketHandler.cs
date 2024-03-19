using Cinemachine;
using System.Collections;
using System.Collections.Generic;
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
		CinemachineVirtualCamera vcam1 = camObj.GetComponent<CinemachineVirtualCamera>();

		GameObject mapObj = MapManager.Inst.Load(mapID, camObj); // TestMap : 1
		GameManager.Inst.SetPlayerCnt(numOfUsers);

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

		for (int i = 0; i < numOfUsers; ++i)
		{
			int connectionID = _reader.GetUShort();
			int idx = _reader.GetByte();
			string nickname = _reader.GetString();
			byte characterChoice = _reader.GetByte();

			player = ResourceManager.Inst.Instantiate($"Creature/Player_{characterChoice}"); // 플레이어 선택
			player.name = $"Player_{characterChoice}_{idx}"; // slot 넘버로

			if (connectionID == UserData.Inst.ConnectionID)
			{
				MyPlayerController mpc = player.AddComponent<MyPlayerController>();
				mpc.Init(i+1, 1);
				mpc.Idx = idx;
				mpc.SetNickname(nickname);
				ObjectManager.Inst.AddPlayer(idx, player);
				vcam1.Follow = player.transform;
			}
			else
			{
				PlayerController pc = player.AddComponent<PlayerController>();
				pc.Init(i+1, 1);
				pc.Idx = idx;
				pc.SetNickname(nickname);
				ObjectManager.Inst.AddPlayer(idx, player);
			}
		}

		Debug.Log($"플레이어 수 : {GameManager.Inst.PlayerCnt}");
		Debug.Log($"몬스터 수 : {GameManager.Inst.MonsterCnt}");
		Debug.Log($"스테이지 수 : {MapManager.Inst.MaxStage}");

		GameManager.Inst.GameStart = true;
	}

	public static void BeginMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;
		byte dir = _reader.GetByte();

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		pc.SetDir(dir);
		pc.EndMovePosition(xpos, ypos);
	}

	public static void EndMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		pc.SetDir(0);
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

		mc.BeginMove(destCellXPos, destCellYPos);
	}

	public static void MonsterAttack(PacketReader _reader)
	{
		int targetCnt = _reader.GetByte();

		List<PlayerController> targets = new List<PlayerController>();
		for(int i=0; i<targetCnt; ++i)
		{
			PlayerController pc = ObjectManager.Inst.FindPlayer(_reader.GetByte());
			targets.Add(pc);
		}

		MonsterController mc = ObjectManager.Inst.FindMonster(_reader.GetByte(), _reader.GetByte());
		mc.ChangeState(new MonsterAttackState(targets));
	}

	public static void InGameExit(PacketReader _reader)
	{
		byte leftUserIdx = _reader.GetByte();
		byte nextRoomOwnerIdx = _reader.GetByte();

		if (UserData.Inst.MyRoomSlot == nextRoomOwnerIdx)
			UserData.Inst.IsRoomOwner = true;

		GameManager.Inst.SubPlayerCnt();
		PlayerController pc = ObjectManager.Inst.FindPlayer(leftUserIdx);
		ObjectManager.Inst.RemovePlayer(leftUserIdx);
		MapManager.Inst.RemoveAimTile(pc.CellPos.x, pc.CellPos.y);
		ResourceManager.Inst.Destroy(pc.gameObject);
	}

	public static void Attack(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		ushort count = _reader.GetUShort();

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		List<MonsterController> targets = new List<MonsterController>();
		MonsterController mc;
		for (int i = 0; i < count; ++i)
		{
			mc = ObjectManager.Inst.FindMonster(_reader.GetByte(), _reader.GetByte());
			targets.Add(mc);
		}

		eSkill skill = (eSkill)_reader.GetByte();
		pc.CurSkill.SetSkill(skill);

		pc.ChangeState(new PlayerAttackState(targets, pc.CurSkill));
	}	
	
	public static void RangedAttack(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		ushort count = _reader.GetUShort();
		short x = _reader.GetShort();
		short y = _reader.GetShort();

		PlayerController pc = ObjectManager.Inst.FindPlayer(roomSlot);
		List<MonsterController> targets = new List<MonsterController>();
		MonsterController mc;
		for (int i = 0; i < count; ++i)
		{
			mc = ObjectManager.Inst.FindMonster(_reader.GetByte(), _reader.GetByte());
			targets.Add(mc);
		}

		eSkill skill = (eSkill)_reader.GetByte();
		pc.CurSkill.SetSkill(skill);

		pc.ChangeState(new PlayerAttackState(targets, pc.CurSkill));
		pc.SetRangedSkillObjPos(new Vector2Int(x, y));
	}

	public static void GameOver(PacketReader _reader)
	{
		GameManager.Inst.Clear();
		ObjectManager.Inst.ClearPlayers();
		ObjectManager.Inst.ClearMonsters();
		MapManager.Inst.Destroy();
		SceneManagerEx.Inst.LoadSceneWithFadeOut(eScene.Room);
	}
}
