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
		CreateRoom_Success,
		CreateRoom_Fail,
		ExitRoom,
		StartGame_Success,
		StartGame_Fail,
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
		ExitRoom,
		StartGame,
		RoomReady,
		RoomStandby,
		RoomChat,
	}
}
