using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
	public const int BufferMax = 65536; // 65536; // 428 * 300 <= 131072
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

	public enum eScene
	{
		None,
		Login,
		Lobby,
		Room,
		InGame
	}

	public enum eStage
	{
		None,
		Test,

	}

	public enum eUIPopup
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

	public enum eUIChat
	{
		UILobbyChat,
		UIRoomChat,
	}

	public enum eUI
	{
		UILobby_RoomList,
		UILobby_RoomList_Block,
		UILobby_UserList,

		#region UIRoom
		UIRoom_Users,
		UIRoom_GamePanel,
		UIRoom_StartBtn,
		UIRoom_ReadyBtn,
		UIRoom_StandbyBtn,
		#endregion
	}

	public enum eUIBtn
	{
		MapChoiceBtn,
	}

	public enum eRoomUserState
	{
		None,
		Ready,
		Standby
	}

	public enum eGameMap
	{
		None,
		Test,
	}

	public enum eRoomState
	{
		None,
		Standby,
		InGame,
	}

	public enum eSkill
	{
		None,
		Slash,
		Blast,
		Skill1,
		Skill2,
		Skill3,
	}

	public enum eSkillType
	{
		None,
		Melee,
		Ranged,
		Auto
	}
}
