using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketType
{
	public enum Server : ushort
	{
		None = 0,
		Test,
		LoginReqOK,
		LoginReqFail,
	}

	public enum Client : ushort
	{
		None = 0,
		Test,
		LoginReq,
	}
}
