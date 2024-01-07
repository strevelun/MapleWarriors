using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketType
{
	public enum Server : ushort
	{
		None = 0,
		Test,
		LoginFailure,
		LoginSuccess,
		Chat,
	}

	public enum Client : ushort
	{
		None = 0,
		Test,
		LoginReq,
		Chat,
	}
}
