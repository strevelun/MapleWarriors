using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InGamePacketHandler
{
	public static void ResInitInfo(PacketReader _reader)
	{
		// 몬스터 컨트롤러에서 타일맵 데이터에 접근할 수 있어야 함. (충돌 감지 및 경로 생성)
		long seed = _reader.GetInt64();
		int mapID = _reader.GetByte();
		int numOfUsers = _reader.GetByte();
		GameObject player;
		CinemachineVirtualCamera vcam1 = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();

		MapManager.Inst.Load(1, 1); // TestMap : 1

		for (int i = 0; i < numOfUsers; ++i)
		{
			int connectionID = _reader.GetUShort();
			int idx = _reader.GetByte();
			string nickname = _reader.GetString();

			player = ResourceManager.Inst.Instantiate($"Creature/Player_1"); // 플레이어 선택
			player.name = $"Player_1_{idx}"; // slot 넘버로

			if (connectionID == UserData.Inst.ConnectionID)
			{
				MyPlayerController mpc = player.AddComponent<MyPlayerController>();
				mpc.Init(i, 1);
				mpc.SetNickname(nickname);
				ObjectManager.Inst.AddPlayer(player.name, player);
				vcam1.Follow = player.transform;
			}
			else
			{
				PlayerController pc = player.AddComponent<PlayerController>();
				pc.Init(i, 1);
				pc.SetNickname(nickname);
				ObjectManager.Inst.AddPlayer(player.name, player);
			}
		}

		
		
		GameObject monster;
		for (int i = 0; i < 5; ++i)
		{
			monster = ResourceManager.Inst.Instantiate("Creature/Slime");
			MonsterController mc = monster.GetComponent<MonsterController>();
			MonsterData monsterData = DataManager.Inst.FindMonsterData("Slime");
			mc.Init(i * 5, 8);
			mc.SetMonsterData(monsterData);

			monster.name = $"Slime_{i}";
			ObjectManager.Inst.AddMonster(monster.name, monster);
			MapManager.Inst.AddMonster(mc, mc.CellPos.x, mc.CellPos.y);
		}

		for (int i = 0; i < 5; ++i)
		{
			monster = ResourceManager.Inst.Instantiate("Creature/Blue_Mushroom");
			MonsterController mc = monster.GetComponent<MonsterController>();
			MonsterData monsterData = DataManager.Inst.FindMonsterData("Blue_Mushroom");
			mc.Init(i * 5, 9);
			mc.SetMonsterData(monsterData);

			monster.name = $"Blue_Mushroom_{i}";
			ObjectManager.Inst.AddMonster(monster.name, monster);
			MapManager.Inst.AddMonster(mc, mc.CellPos.x, mc.CellPos.y);
		}
	}

	public static void BeginMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		byte dir = _reader.GetByte();

		PlayerController pc = ObjectManager.Inst.FindPlayer($"Player_1_{roomSlot}");
		pc.SetDir((CreatureController.eDir)dir);
		//pc.BeginMove();
	}

	public static void EndMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;
		//byte dir = _reader.GetByte();
		//long tick = _reader.GetInt64();

		PlayerController pc = ObjectManager.Inst.FindPlayer($"Player_1_{roomSlot}");
		//pc.UnSetDir((CreatureController.Dir)dir);
		pc.SetDir(CreatureController.eDir.None);
		pc.EndMovePosition(xpos, ypos);
		//pc.EndMovePosition(xpos, ypos);
	}

	public static void BeginMoveMonster(PacketReader _reader)
	{
		string name = _reader.GetString();
		int pathIdx = _reader.GetUShort();
		int destCellXPos = _reader.GetUShort();
		int destCellYPos = _reader.GetUShort();

		MonsterController mc = ObjectManager.Inst.FindMonster(name);
		if (!mc)
		{
			Debug.Log("몬스터 이름 찾을 수 없음");
			return;
		}

		mc.BeginMove(pathIdx, destCellXPos, destCellYPos);
	}

	public static void Attack(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		string monsterName = _reader.GetString();

		PlayerController pc = ObjectManager.Inst.FindPlayer($"Player_1_{roomSlot}");
		MonsterController mc = ObjectManager.Inst.FindMonster(monsterName);
		pc.ChangeState(new PlayerAttackState(mc, "Attack_0"));
	}
}
