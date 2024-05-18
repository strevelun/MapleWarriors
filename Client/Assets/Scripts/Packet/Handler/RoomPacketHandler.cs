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

		UIChat uichat = UIManager.Inst.FindUI(Define.UIChatEnum.UIRoomChat);
		uichat.AddChat(nickname, chat);
	}

	public static void ExitRoom()
	{
		UserData.Inst.IsRoomOwner = false;
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.SceneEnum.Lobby);
		Debug.Log("ExitRoom");
	}	
	
	public static void NotifyRoomUserExit(PacketReader _reader)
	{
		int idx = _reader.GetByte(); // 나간놈
		int prevOwnerIdx = _reader.GetByte(); 
		int nextOwnerIdx = _reader.GetByte();

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(idx).gameObject;
		slot.GetComponent<UIButton>().IsActive = false;

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

			obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_StandbyBtn);
			obj.SetActive(false);
			obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_ReadyBtn);
			obj.SetActive(false);
			obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_StartBtn);
			obj.SetActive(true);

			UserData.Inst.RoomOwnerSlot = nextOwnerIdx;
		}

		if(UserData.Inst.MyRoomSlot == nextOwnerIdx)
		{
			UserData.Inst.IsRoomOwner = true;
			GameObject parentObj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_GamePanel);
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

		GameObject obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_Users);
		GameObject slot = obj.transform.GetChild(idx).gameObject;
		obj = Util.FindChild(slot, false, "CharacterBtn"); // 기본 캐릭터
		obj.SetActive(true);
		obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadSprite($"Player/player1");
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
		Define.RoomUserStateEnum eState;
		Define.CharacterChoiceEnum eChoice;
		int roomID = _reader.GetByte();
		string roomTitle = _reader.GetString();
		int roomOwnerIdx = _reader.GetByte();
		int numOfUsers = _reader.GetByte();
		GameObject obj;
		Transform t;
		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_Users);

		UserData.Inst.RoomOwnerSlot = roomOwnerIdx;

		GameObject titleObj = UIManager.Inst.SceneUI.FindUI("Title");
		titleObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{roomTitle}";
		GameObject roomIDObj = UIManager.Inst.SceneUI.FindUI("RoomID");
		roomIDObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{roomID}";

		for (int i=0; i<numOfUsers; ++i)
		{
			connectionID = _reader.GetUShort();
			idx = _reader.GetByte();
			isOwner = _reader.GetBool(); // 방장은 따로 표시
			nickname = _reader.GetString();
			eState = (Define.RoomUserStateEnum)_reader.GetByte() - 1;
			eChoice = (Define.CharacterChoiceEnum)_reader.GetByte();

			GameObject slot = objRoomUsers.transform.GetChild(idx).gameObject;
			if (connectionID == UserData.Inst.ConnectionID) // 본인
			{
				obj = Util.FindChild(slot, false, "Frame");
				obj.SetActive(true);
				UserData.Inst.MyRoomSlot = idx;

			}
			else
				slot.GetComponent<UIButton>().IsActive = false;

			obj = Util.FindChild(slot, false, "CharacterBtn");
			obj.SetActive(true);
			obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadSprite($"Player/player{(byte)eChoice}");

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

		GameObject parentObj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_GamePanel);
		obj = Util.FindChild(parentObj, false, "MapChoiceBtn");
		obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadImage($"MapProfile/map_{mapID}");
	}

	public static void StartGame_Success()
	{
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.SceneEnum.InGame);
	}

	public static void StartGame_Fail()
	{
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UIGameStartFailPopup);
	}

	public static void RoomReady(PacketReader _reader)
	{
		int roomUserIdx = _reader.GetByte();
		int connectionID = _reader.GetUShort();

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(roomUserIdx).gameObject;
		GameObject obj = Util.FindChild(slot, false, "UserState");
		obj.transform.GetChild(0).gameObject.SetActive(true); // 0 : Ready
		obj.transform.GetChild(1).gameObject.SetActive(false); // 0 : Standby

		if(connectionID == UserData.Inst.ConnectionID)
		{
			obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_ReadyBtn);
			UIButton btn = obj.GetComponent<UIButton>();
			obj.SetActive(false);
			btn.IsActive = true;
			obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_StandbyBtn);
			obj.SetActive(true);
		}
	}	
	
	public static void RoomReady_Fail()
	{
		GameObject obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_ReadyBtn);
		UIButton btn = obj.GetComponent<UIButton>();
		btn.IsActive = true;
	}

	public static void RoomStandby(PacketReader _reader)
	{
		int roomUserIdx = _reader.GetByte();
		int connectionID = _reader.GetUShort();

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(roomUserIdx).gameObject;
		GameObject obj = Util.FindChild(slot, false, "UserState");
		obj.transform.GetChild(0).gameObject.SetActive(false); // 0 : Ready
		obj.transform.GetChild(1).gameObject.SetActive(true); // 0 : Standby

		if (connectionID == UserData.Inst.ConnectionID)
		{
			obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_StandbyBtn);
			UIButton btn = obj.GetComponent<UIButton>();
			obj.SetActive(false);
			btn.IsActive = true;
			obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_ReadyBtn);
			obj.SetActive(true);
		}
	}
	
	public static void RoomStandby_Fail()
	{
		GameObject obj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_StandbyBtn);
		UIButton btn = obj.GetComponent<UIButton>();
		btn.IsActive = true;
	}
	
	public static void RoomMapChoice(PacketReader _reader)
	{
		byte mapID = _reader.GetByte();

		GameObject parentObj = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_GamePanel);
		GameObject obj = Util.FindChild(parentObj, false, "MapChoiceBtn");
		obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadImage($"MapProfile/map_{mapID}");
	}
	
	public static void RoomCharacterChoice(PacketReader _reader)
	{
		byte roomUserIdx = _reader.GetByte();
		byte characterChoice = _reader.GetByte();

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.UIEnum.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(roomUserIdx).gameObject;
		GameObject obj = Util.FindChild(slot, false, "CharacterBtn");
		obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadSprite($"Player/player{characterChoice}");
	}
}