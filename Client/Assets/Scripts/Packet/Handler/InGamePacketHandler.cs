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
				mpc.SetNickname(nickname);
				ObjectManager.Inst.Add<MyPlayerController>(player.name, player);
			}
			else
			{
				PlayerController pc = player.AddComponent<PlayerController>();
				pc.SetNickname(nickname);
				ObjectManager.Inst.Add<PlayerController>(player.name, player);
			}
		}

		MapManager.Inst.Load(1, 1); // TestMap : 1
	}

	public static void BeginMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		byte dir = _reader.GetByte();

		PlayerController pc = ObjectManager.Inst.Find($"Player_{roomSlot}");
		pc.SetDir((CreatureController.Dir)dir);
	}

	public static void EndMove(PacketReader _reader)
	{
		byte roomSlot = _reader.GetByte();
		float xpos = _reader.GetInt32() / 1000000.0f;
		float ypos = _reader.GetInt32() / 1000000.0f;

		Debug.Log($"���� �������� : {xpos}, {ypos}");

		PlayerController pc = ObjectManager.Inst.Find($"Player_{roomSlot}");
		pc.SetDir(CreatureController.Dir.None);
		pc.EndMovePosition(xpos, ypos);
	}
}
