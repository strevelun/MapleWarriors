using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
	public const int BufferMax = 131072; // 65536; // 428 * 300 <= 131072
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
	public const float MaxExpelTime = 5f;

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
		UICharacterChoicePopup,
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

		#region UIIngame
		UIInGame_SkillPanel,
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
		Map0,
		Map1,
	}

	public enum eCharacterChoice
	{
		None,
		Player1,
		Player2
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
	}

	public enum eSkillType
	{
		None,
		Melee,
		Ranged,
		Auto
	}

	public enum eDir
	{
		None,
		Up = 1, // 0001
		Right = 2, // 0010
		Down = 4, // 0100
		Left = 8, // 1000
		UpLeft = 9, // 1001
		UpRight = 3, // 0011
		DownLeft = 12, // 1100
		DownRight = 6 // 0110
	}
}
