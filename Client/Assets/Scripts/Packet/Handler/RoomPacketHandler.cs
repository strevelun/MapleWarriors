using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class RoomPacketHandler
{
	public static void RoomChat(PacketReader _reader)
	{
		string nickname = _reader.GetString();
		string chat = _reader.GetString();

		UIChat uichat = UIManager.Inst.FindUI(Define.eUIChat.UIRoomChat);
		uichat.AddChat(nickname, chat);
		uichat.SetScrollbarDown();
	}

	public static void ExitRoom(PacketReader _reader)
	{
		UserData.Inst.IsRoomOwner = false;
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.eScene.Lobby);
		Debug.Log("ExitRoom");
	}	
	
	public static void NotifyRoomUserExit(PacketReader _reader)
	{
		int idx = _reader.GetByte(); // 나간놈
		int prevOwnerIdx = _reader.GetByte(); 
		int nextOwnerIdx = _reader.GetByte();

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.eUI.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(idx).gameObject;
		GameObject obj = Util.FindChild(slot, false, "CharacterBtn");
		obj.SetActive(false);
		obj = Util.FindChild(slot, false, "Nickname");
		obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Empty;
		obj = Util.FindChild(slot, false, "UserState");
		obj.transform.GetChild(0).gameObject.SetActive(false); // ready
		obj.transform.GetChild(1).gameObject.SetActive(false); // standby

		// 방장이 바뀌었다면
		if (prevOwnerIdx != nextOwnerIdx) 
		{
			GameObject badge;
			if (prevOwnerIdx < Define.RoomUserSlot)
			{
				slot = objRoomUsers.transform.GetChild(prevOwnerIdx).gameObject;
				badge = Util.FindChild(slot, false, "OwnerBadge");
				badge.SetActive(false);
			}

			slot = objRoomUsers.transform.GetChild(nextOwnerIdx).gameObject;
			badge = Util.FindChild(slot, false, "OwnerBadge");
			badge.SetActive(true);

			obj = Util.FindChild(slot, false, "UserState");
			obj.transform.GetChild(0).gameObject.SetActive(false);
			obj.transform.GetChild(1).gameObject.SetActive(false);

			obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_StandbyBtn);
			obj.SetActive(false);
			obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_ReadyBtn);
			obj.SetActive(false);
			obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_StartBtn);
			obj.SetActive(true);
		}

		if(UserData.Inst.MyRoomSlot == nextOwnerIdx)
		{
			UserData.Inst.IsRoomOwner = true;
			GameObject parentObj = UIManager.Inst.FindUI(Define.eUI.UIRoom_GamePanel);
			obj = Util.FindChild(parentObj, false, "MapChoiceBtn");
			UIButton uibtn = obj.GetComponent<UIButton>();
			uibtn.IsActive = true;
			Debug.Log("내가 이제 방장이다");
		}
	}	
	
	public static void NotifyRoomUserEnter(PacketReader _reader)
	{
		int idx = _reader.GetByte();
		string nickname = _reader.GetString();

		GameObject obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_Users);
		GameObject slot = obj.transform.GetChild(idx).gameObject;
		obj = Util.FindChild(slot, false, "CharacterBtn"); // 기본 캐릭터
		obj.SetActive(true);
		obj = Util.FindChild(slot, false, "Nickname");
		obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = nickname;
		obj = Util.FindChild(slot, false, "UserState");
		obj.transform.GetChild(0).gameObject.SetActive(false); // ready
		obj.transform.GetChild(1).gameObject.SetActive(true); // standby
	}	
	
	public static void RoomUsersInfo(PacketReader _reader)
	{
		int connectionID;
		int idx;
		bool isOwner;
		string nickname;
		Define.eRoomUserState eState;
		Define.eCharacterChoice eChoice;
		string roomTitle = _reader.GetString();
		int numOfUsers = _reader.GetByte();
		GameObject obj;
		Transform t;
		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.eUI.UIRoom_Users);

		for (int i=0; i<numOfUsers; ++i)
		{
			connectionID = _reader.GetUShort();
			idx = _reader.GetByte();
			isOwner = _reader.GetBool(); // 방장은 따로 표시
			nickname = _reader.GetString();
			eState = (Define.eRoomUserState)_reader.GetByte() - 1;
			eChoice = (Define.eCharacterChoice)_reader.GetByte();

			GameObject slot = objRoomUsers.transform.GetChild(idx).gameObject;
			if (connectionID == UserData.Inst.ConnectionID) // 본인
			{
				obj = Util.FindChild(slot, false, "Frame");
				obj.SetActive(true);
				UserData.Inst.MyRoomSlot = idx;
			}

			//GameObject chAnim = objRoomUsers.transform.GetChild(1).transform.GetChild(idx).gameObject;
			//chAnim.SetActive(true);
			//chAnim.GetComponent<SpriteRenderer>().sprite = ResourceManager.Inst.LoadSprite($"Player/player{(byte)eChoice}");
			//chAnim.GetComponent<Animator>().Play($"Player{(byte)eChoice}");

			if (i == UserData.Inst.MyRoomSlot)
			{
				obj = Util.FindChild(slot, false, "CharacterBtn");
				obj.SetActive(true);
				obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadSprite($"Player/player{(byte)eChoice}");
			}

			obj = Util.FindChild(slot, false, "Nickname");
			obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = nickname;
			if (isOwner)
			{
				obj = Util.FindChild(slot, false, "OwnerBadge");
				obj.SetActive(true);
			}
			else
			{
				obj = Util.FindChild(slot, false, "UserState");
				t = obj.transform.GetChild((int)eState);
				t.gameObject.SetActive(true);
			}
		}

		byte mapID = _reader.GetByte();

		GameObject parentObj = UIManager.Inst.FindUI(Define.eUI.UIRoom_GamePanel);
		obj = Util.FindChild(parentObj, false, "MapChoiceBtn");
		obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadImage($"MapProfile/map_{mapID}");
	}

	public static void StartGame_Success(PacketReader _reader)
	{
		// 방 멤버들은 방장이 맵을 바꾸면 바뀐 
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.eScene.InGame);
	}

	public static void StartGame_Fail(PacketReader _reader)
	{
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UIGameStartFailPopup);
	}

	// 방장이 본인에게 양도되는 패킷이 온 후에 Ready 패킷이 도착한다면
	public static void RoomReady(PacketReader _reader)
	{
		int roomUserIdx = _reader.GetByte();
		int connectionID = _reader.GetUShort();

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.eUI.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(roomUserIdx).gameObject;
		GameObject obj = Util.FindChild(slot, false, "UserState");
		obj.transform.GetChild(0).gameObject.SetActive(true); // 0 : Ready
		obj.transform.GetChild(1).gameObject.SetActive(false); // 0 : Standby

		if(connectionID == UserData.Inst.ConnectionID)
		{
			obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_ReadyBtn);
			UIButton btn = obj.GetComponent<UIButton>();
			obj.SetActive(false);
			btn.IsActive = true;
			obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_StandbyBtn);
			obj.SetActive(true);
		}
	}	
	
	public static void RoomReady_Fail(PacketReader _reader)
	{
		GameObject obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_ReadyBtn);
		UIButton btn = obj.GetComponent<UIButton>();
		btn.IsActive = true;
	}

	public static void RoomStandby(PacketReader _reader)
	{
		int roomUserIdx = _reader.GetByte();
		int connectionID = _reader.GetUShort();

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.eUI.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(roomUserIdx).gameObject;
		GameObject obj = Util.FindChild(slot, false, "UserState");
		obj.transform.GetChild(0).gameObject.SetActive(false); // 0 : Ready
		obj.transform.GetChild(1).gameObject.SetActive(true); // 0 : Standby

		if (connectionID == UserData.Inst.ConnectionID)
		{
			obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_StandbyBtn);
			UIButton btn = obj.GetComponent<UIButton>();
			obj.SetActive(false);
			btn.IsActive = true;
			obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_ReadyBtn);
			obj.SetActive(true);
		}
	}
	
	public static void RoomStandby_Fail(PacketReader _reader)
	{
		GameObject obj = UIManager.Inst.FindUI(Define.eUI.UIRoom_StandbyBtn);
		UIButton btn = obj.GetComponent<UIButton>();
		btn.IsActive = true;
	}
	
	public static void RoomMapChoice(PacketReader _reader)
	{
		byte mapID = _reader.GetByte();

		GameObject parentObj = UIManager.Inst.FindUI(Define.eUI.UIRoom_GamePanel);
		GameObject obj = Util.FindChild(parentObj, false, "MapChoiceBtn");
		obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadImage($"MapProfile/map_{mapID}");
	}
	
	public static void RoomCharacterChoice(PacketReader _reader)
	{
		byte roomUserIdx = _reader.GetByte();
		byte characterChoice = _reader.GetByte();

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.eUI.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(roomUserIdx).gameObject;
		GameObject obj = Util.FindChild(slot, false, "CharacterBtn");
		obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadSprite($"Player/player{characterChoice}");
	}
}