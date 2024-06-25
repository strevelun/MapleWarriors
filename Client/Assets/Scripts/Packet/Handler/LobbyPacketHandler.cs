using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public static class LobbyPacketHandler
{
	public static void Test()
	{
		//long ticks = _reader.GetInt64();
	}

	public static void LobbyChat(PacketReader _reader)
	{
		string nickname = _reader.GetString();
		string chat = _reader.GetString();

		UIChat uichat = UIManager.Inst.FindUI(Define.UIChatEnum.UILobbyChat);
		uichat.AddChat(nickname, chat);
	}

	public static void LobbyUpdateInfo_UserList(PacketReader _reader)
	{
		if (SceneManagerEx.Inst.CurScene.IsLoading == true) return;

		TextMeshProUGUI tmp;

		GameObject userListObj = UIManager.Inst.FindUI(Define.UIEnum.UILobby_UserList);
		GameObject uiPageObj = Util.FindChild(userListObj, false, "Page");
		if (uiPageObj == null)
		{
			Debug.Log("uiPageObj is null");
			return;
		}

		byte page = _reader.GetByte();
		tmp = uiPageObj.GetComponent<TextMeshProUGUI>();
		tmp.SetText("페이지 : " + page);

		UIPage uiPage = userListObj.GetComponent<UIPage>();
		uiPage.CurPage = page;
		GameObject obj, item;
		int userNum = _reader.GetByte();
		int i = 0;
		int activeCount = 0;
		int roomID;
		for (; i < Define.UserListMaxItemInPage; ++i)
		{
			item = uiPage.GetItem(i);
			if (i < userNum)
			{
				obj = Util.FindChild(item, false, "Nickname");
				tmp = obj.GetComponent<TextMeshProUGUI>();
				tmp.SetText(_reader.GetString());
				obj = Util.FindChild(item, false, "Location");
				tmp = obj.GetComponent<TextMeshProUGUI>();
				Define.SceneEnum eScene = (Define.SceneEnum)_reader.GetByte();
				string sceneName = "";
				switch (eScene)
				{
					case Define.SceneEnum.Lobby:
						sceneName = "로비";
						break;
					case Define.SceneEnum.Room:
					case Define.SceneEnum.InGame:
						roomID = _reader.GetByte();
						sceneName = roomID + "번방";
						break;
				}
				tmp.SetText(sceneName);

				if (!item.activeSelf) item.SetActive(true);
				++activeCount;
			}
			else
			{
				if (item.activeSelf) item.SetActive(false);
			}
		}

		uiPage.ActiveItemCount = activeCount;
	}

	public static void LobbyUpdateInfo_RoomList(PacketReader _reader)
	{
		if (SceneManagerEx.Inst.CurScene.IsLoading == true) return;

		TextMeshProUGUI tmp;

		byte page = _reader.GetByte();

		GameObject roomListObj = UIManager.Inst.FindUI(Define.UIEnum.UILobby_RoomList);
		GameObject uiPageObj = Util.FindChild(roomListObj, false, "Page");
		tmp = uiPageObj.GetComponent<TextMeshProUGUI>();
		tmp.SetText("페이지 : " + page);

		UIPage uiPage = roomListObj.GetComponent<UIPage>();
		uiPage.CurPage = page;

		GameObject obj, item;
		int roomCount = _reader.GetByte();
		int i = 0;
		int activeCount = 0;
		for (; i < Define.RoomListMaxItemInPage; ++i)
		{
			item = uiPage.GetItem(i);
			UIButton uibtn = item.GetComponent<UIButton>();

			if (i < roomCount)
			{
				obj = Util.FindChild(item, false, "Id");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				int id = _reader.GetUShort();
				uibtn.Init(() =>
				{
					GameObject go = UIManager.Inst.FindUI(Define.UIEnum.UILobby_RoomList_Block);
					go.SetActive(true);

					Debug.Log($"버튼 클릭 : {id}");
					Packet pkt = LobbyPacketMaker.EnterRoom(id);
					NetworkManager.Inst.Send(pkt);
				});
				tmp.SetText(id.ToString());
				obj = Util.FindChild(item, false, "Title");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.SetText(_reader.GetString());
				obj = Util.FindChild(item, false, "Owner");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.SetText(_reader.GetString());
				obj = Util.FindChild(item, false, "NumOfUser");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.SetText(_reader.GetByte().ToString() + "/4");
				obj = Util.FindChild(item, false, "State");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				Define.RoomStateEnum eState = (Define.RoomStateEnum)_reader.GetByte();
				string sceneName = "";
				switch (eState)
				{
					case Define.RoomStateEnum.Standby:
						sceneName = "대기";
						break;
					case Define.RoomStateEnum.InGame:
						sceneName = "게임중";
						break;
				}
				tmp.SetText(sceneName);

				if (!item.activeSelf) item.SetActive(true);
				++activeCount;
			}
			else
			{
				if (item.activeSelf) item.SetActive(false);
			}
		}
		uiPage.ActiveItemCount = activeCount;
	}	
	
	public static void CreateRoom_Success()
	{
		UserData.Inst.IsRoomOwner = true;
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.SceneEnum.Room);
	}

	public static void CreateRoom_Fail()
	{
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UICreateRoomFailPopup);
	}

	public static void EnterRoom_Success()
	{
		UserData.Inst.IsRoomOwner = false;
		UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIFindRoomPopup);
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.SceneEnum.Room);
	}

	public static void EnterRoom_Full()
	{
		GameObject go = UIManager.Inst.FindUI(Define.UIEnum.UILobby_RoomList_Block);
		go.SetActive(false);
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UIEnterRoomFullPopup);
	}

	public static void EnterRoom_InGame()
	{
		GameObject go = UIManager.Inst.FindUI(Define.UIEnum.UILobby_RoomList_Block);
		go.SetActive(false);
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UIEnterRoomInGamePopup);
	}

	public static void EnterRoom_NoRoom()
	{
		GameObject go = UIManager.Inst.FindUI(Define.UIEnum.UILobby_RoomList_Block);
		go.SetActive(false);
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UIEnterRoomNopePopup);
	}
}
