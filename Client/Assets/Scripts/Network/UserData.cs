using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData 
{
	private static UserData s_inst = null;
	public static UserData Inst
	{
		get
		{
			if (s_inst == null) s_inst = new UserData();
			return s_inst;
		}
	}

	public string Nickname;
	public uint ConnectionID;
	public bool IsRoomOwner;
	public int RoomOwnerSlot;
	public int MyRoomSlot;
	public int MyPort;
}
