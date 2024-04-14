using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Define;

public static class LobbyPacketHandler
{
	public static void Test(PacketReader _reader)
	{
		long ticks = _reader.GetInt64();

		//Debug.Log($"패킷 시간차 : {UserData.Inst.ticks} - {ticks} = {(UserData.Inst.ticks - ticks)}ms");
	}

	public static void LobbyChat(PacketReader _reader)
	{
		string nickname = _reader.GetString();
		string chat = _reader.GetString();

		UIChat uichat = UIManager.Inst.FindUI(Define.eUIChat.UILobbyChat);
		uichat.AddChat(nickname, chat);
		uichat.SetScrollbarDown();
	}

	public static void LobbyUpdateInfo_UserList(PacketReader _reader)
	{
		// TODO : UIPage로 로직 옮기기

		TextMeshProUGUI tmp;

		byte page = _reader.GetByte();

		GameObject userListObj = UIManager.Inst.FindUI(Define.eUI.UILobby_UserList);
		GameObject uiPageObj = Util.FindChild(userListObj, false, "Page");
		tmp = uiPageObj.GetComponent<TextMeshProUGUI>();
		tmp.text = "페이지 : " + page;

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
				tmp.text = _reader.GetString();
				obj = Util.FindChild(item, false, "Location");
				tmp = obj.GetComponent<TextMeshProUGUI>();
				Define.eScene eScene = (Define.eScene)_reader.GetByte();
				string sceneName = "";
				switch (eScene)
				{
					case Define.eScene.Lobby:
						sceneName = "로비";
						break;
					case Define.eScene.Room:
					case Define.eScene.InGame:
						roomID = _reader.GetByte();
						sceneName = roomID + "번방";
						break;
				}
				tmp.text = sceneName;

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
		TextMeshProUGUI tmp;

		byte page = _reader.GetByte();

		GameObject roomListObj = UIManager.Inst.FindUI(Define.eUI.UILobby_RoomList);
		GameObject uiPageObj = Util.FindChild(roomListObj, false, "Page");
		tmp = uiPageObj.GetComponent<TextMeshProUGUI>();
		tmp.text = "페이지 : " + page;

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
				uint id = _reader.GetUInt32();
				uibtn.Init(() =>
				{
					GameObject go = UIManager.Inst.FindUI(Define.eUI.UILobby_RoomList_Block);
					go.SetActive(true);

					Debug.Log($"버튼 클릭 : {id}");
					Packet pkt = LobbyPacketMaker.EnterRoom(id);
					NetworkManager.Inst.Send(pkt);
				});
				tmp.text = id.ToString();
				obj = Util.FindChild(item, false, "Title");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.text = _reader.GetString();
				obj = Util.FindChild(item, false, "Owner");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.text = _reader.GetString();
				obj = Util.FindChild(item, false, "NumOfUser");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.text = _reader.GetByte().ToString() + "/4";
				obj = Util.FindChild(item, false, "State");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				Define.eRoomState eState = (Define.eRoomState)_reader.GetByte();
				string sceneName = "";
				switch (eState)
				{
					case Define.eRoomState.Standby:
						sceneName = "대기";
						break;
					case Define.eRoomState.InGame:
						sceneName = "게임중";
						break;
				}
				tmp.text = sceneName;

				if (!item.activeSelf) item.SetActive(true);
				++activeCount;
			}
			else
			{
				if (item.activeSelf) item.SetActive(false);
			}
		}
		uiPage.ActiveItemCount = activeCount;
		//Debug.Log("LobbyUpdateInfo_RoomList : " + roomCount);
	}	
	
	// TODO : CreateRoom_Success
	public static void CreateRoom_Success(PacketReader _reader)
	{
		UserData.Inst.IsRoomOwner = true;
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.eScene.Room);
	}

	public static void CreateRoom_Fail(PacketReader _reader)
	{
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UICreateRoomFailPopup);
	}

	public static void EnterRoom_Success(PacketReader _reader)
	{
		UserData.Inst.IsRoomOwner = false;
		// 씬 전환 후 룸에 있는 슬롯 갱신
		SceneManagerEx.Inst.LoadSceneWithFadeOut(Define.eScene.Room);
	}

	public static void EnterRoom_Full(PacketReader _reader)
	{
		GameObject go = UIManager.Inst.FindUI(Define.eUI.UILobby_RoomList_Block);
		go.SetActive(false);
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UIEnterRoomFullPopup);
	}

	public static void EnterRoom_InGame(PacketReader _reader)
	{
		GameObject go = UIManager.Inst.FindUI(Define.eUI.UILobby_RoomList_Block);
		go.SetActive(false);
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UIEnterRoomInGamePopup);
	}

	public static void EnterRoom_NoRoom(PacketReader _reader)
	{
		GameObject go = UIManager.Inst.FindUI(Define.eUI.UILobby_RoomList_Block);
		go.SetActive(false);
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UIEnterRoomNopePopup);
	}
}
