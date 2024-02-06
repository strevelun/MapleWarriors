using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InGamePacketHandler
{
	public static void ResInitInfo(PacketReader _reader)
	{
		// ���� ��Ʈ�ѷ����� Ÿ�ϸ� �����Ϳ� ������ �� �־�� ��. (�浹 ���� �� ��� ����)
		long seed = _reader.GetInt64();
		int mapID = _reader.GetByte();
		int numOfUsers = _reader.GetByte();
		GameObject player;

		MapManager.Inst.Load(1, 1); // TestMap : 1

		for (int i = 0; i < numOfUsers; ++i)
		{
			int connectionID = _reader.GetUShort();
			int idx = _reader.GetByte();
			string nickname = _reader.GetString();

			player = ResourceManager.Inst.Instantiate($"Creature/Player_1"); // �÷��̾� ����
			player.name = $"Player_{idx}"; // slot �ѹ���

			if (connectionID == UserData.Inst.ConnectionID)
			{
				MyPlayerController mpc = player.AddComponent<MyPlayerController>();
				mpc.Init(i, 1);
				mpc.SetNickname(nickname);
				ObjectManager.Inst.Add<MyPlayerController>(player.name, player);
			}
			else
			{
				PlayerController pc = player.AddComponent<PlayerController>();
				pc.Init(i, 1);
				pc.SetNickname(nickname);
				ObjectManager.Inst.Add<PlayerController>(player.name, player);
			}
		}


		GameObject monster;
		for (int i = 0; i < 30; ++i)
		{
			monster = ResourceManager.Inst.Instantiate("Creature/Slime");
			MonsterController mc = monster.GetComponent<MonsterController>();
			mc.Init(i+5, 10);
		}
	}

	public static void BeginMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		byte dir = _reader.GetByte();

		PlayerController pc = ObjectManager.Inst.Find($"Player_{roomSlot}");
		pc.SetDir((CreatureController.eDir)dir);
		pc.BeginMove();
	}

	public static void EndMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		//float xpos = _reader.GetInt32() / 1000000.0f;
		//float ypos = _reader.GetInt32() / 1000000.0f;
		//byte dir = _reader.GetByte();
		long tick = _reader.GetInt64();

		PlayerController pc = ObjectManager.Inst.Find($"Player_{roomSlot}");
		//pc.UnSetDir((CreatureController.Dir)dir);
		pc.EndMovePosition(tick);
		pc.SetDir(CreatureController.eDir.None);
		//pc.EndMovePosition(xpos, ypos);
	}
}
