using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketType
{
	public enum eServer : ushort
	{
		None = 0,
		Test,
		LoginFailure_AlreadyLoggedIn,
		LoginFailure_Full,
		LoginSuccess,
		LobbyChat,
		LobbyUpdateInfo_UserList,
		LobbyUpdateInfo_RoomList,
		CreateRoom,
	}

	public enum eClient : ushort
	{
		None = 0,
		Test,
		Exit,
		LoginReq,
		LobbyChat,
		LobbyUpdateInfo,
		UserListGetPageInfo,
		RoomListGetPageInfo,
		CreateRoom,
	}
}
