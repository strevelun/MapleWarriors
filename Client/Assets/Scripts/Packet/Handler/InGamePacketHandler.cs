using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public static class InGamePacketHandler
{
	public static void ResInitInfo(PacketReader _reader)
	{
		// ���� ��Ʈ�ѷ����� Ÿ�ϸ� �����Ϳ� ������ �� �־�� ��. (�浹 ���� �� ��� ����)
		long seed = _reader.GetInt64();
		int mapID = _reader.GetByte();
		int numOfUsers = _reader.GetByte();
		GameObject player;
		GameObject camObj = GameObject.Find("CM vcam1");
		CinemachineVirtualCamera vcam1 = camObj.GetComponent<CinemachineVirtualCamera>();

		GameObject mapObj = MapManager.Inst.Load(1, camObj); // TestMap : 1
		GameManager.Inst.SetPlayerCnt(numOfUsers);

		GameObject monsters = Util.FindChild(mapObj, false, "Monsters");
		int monsterCnt = monsters.transform.childCount;
		GameManager.Inst.SetMonsterCnt(monsterCnt);

		for (int i = 0; i < numOfUsers; ++i)
		{
			int connectionID = _reader.GetUShort();
			int idx = _reader.GetByte();
			string nickname = _reader.GetString();

			player = ResourceManager.Inst.Instantiate($"Creature/Player_1"); // �÷��̾� ����
			player.name = $"Player_1_{idx}"; // slot �ѹ���

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

		Debug.Log($"�÷��̾� �� : {GameManager.Inst.PlayerCnt}");
		Debug.Log($"���� �� : {GameManager.Inst.MonsterCnt}");
		Debug.Log($"�������� �� : {MapManager.Inst.MaxStage}");

		GameManager.Inst.GameStart = true;
	}

	public static void BeginMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;
		byte dir = _reader.GetByte();

		PlayerController pc = ObjectManager.Inst.FindPlayer($"Player_1_{roomSlot}");
		pc.SetDir(dir);
		pc.EndMovePosition(xpos, ypos);
	}

	public static void EndMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;

		PlayerController pc = ObjectManager.Inst.FindPlayer($"Player_1_{roomSlot}");
		pc.SetDir(0);
		pc.EndMovePosition(xpos, ypos);
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
			Debug.Log("���� �̸� ã�� �� ����");
			return;
		}

		mc.BeginMove(pathIdx, destCellXPos, destCellYPos);
	}

	public static void Attack(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		ushort count = _reader.GetUShort();

		PlayerController pc = ObjectManager.Inst.FindPlayer($"Player_1_{roomSlot}");
		List<MonsterController> targets = new List<MonsterController>();
		MonsterController mc;
		for (int i = 0; i < count; ++i)
		{
			mc = ObjectManager.Inst.FindMonster(_reader.GetString());
			targets.Add(mc);
		}

		eSkill skill = (eSkill)_reader.GetByte();

		pc.ChangeState(new PlayerAttackState(targets, skill));
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
