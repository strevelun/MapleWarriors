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
			player.name = $"Player_{idx}"; // slot 넘버로

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
		for (int i = 0; i < 50; ++i)
		{
			monster = ResourceManager.Inst.Instantiate("Creature/Slime");
			MonsterController mc = monster.GetComponent<MonsterController>();
			mc.Init(i, 8);

			monster.name = $"Slime_{i}";
			ObjectManager.Inst.AddMonster(monster.name, monster);
		}
	}

	public static void BeginMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		byte dir = _reader.GetByte();

		PlayerController pc = ObjectManager.Inst.FindPlayer($"Player_{roomSlot}");
		pc.SetDir((CreatureController.eDir)dir);
		pc.BeginMove();
	}

	public static void EndMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;
		//byte dir = _reader.GetByte();
		//long tick = _reader.GetInt64();

		PlayerController pc = ObjectManager.Inst.FindPlayer($"Player_{roomSlot}");
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

	public static void EndMoveMonster(PacketReader _reader)
	{
		string name = _reader.GetString();

		MonsterController mc = ObjectManager.Inst.FindMonster(name);
		if (!mc) return;

		mc.EndMove();
	}
}
