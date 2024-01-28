using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
	public const int BufferMax = 65536; // 428 * 300 <= 131072
	public const int PacketBufferMax = 512;
	public const int PacketHeaderSize = 4;
	public const int PacketSize = 2;
	public const int PacketType = 2;
	public const float LobbyUpdateTime = 5.0f;

	public const int UserListPageMax = 30;
	public const int UserListMaxItemInPage = 10;
	public const int RoomListPageMax = 50;
	public const int RoomListMaxItemInPage = 4;
	public const int RoomUserSlot = 4;

	public const int RoomTitleMaxCount = 15;

	public enum Scene
	{
		None,
		Login,
		Lobby,
		Room,
		InGame
	}

	public enum UIPopup
	{
		UILoginFailPopup_WrongInput,
		UILoginFailPopup_AlreadyLoggedIn,
		UILoginFailPopup_Full,
		UIConnectFailPopup,
		UIDisconnectPopup,
		UICreateRoomPopup,
		UICreateRoomFailPopup,
		UIGameStartFailPopup,
		UIMapChoicePopup,
		UIEnterRoomFullPopup,
		UIEnterRoomInGamePopup,
		UIEnterRoomNopePopup,
	}

	public enum UIChat
	{
		UILobbyChat,
		UIRoomChat,
	}

	public enum UI
	{
		UILobby_RoomList,
		UILobby_UserList,
		UIRoom_Users,
		UIRoom_GamePanel
	}

	public enum UIBtn
	{
		MapChoiceBtn,
		StartBtn,
		ReadyBtn,
		StandbyBtn,
	}
}
