using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class LobbyPacketHandler
{
	public static void LobbyChat(PacketReader _reader)
	{
		string nickname = _reader.GetString();
		string chat = _reader.GetString();

		UIChat uichat = UIManager.Inst.FindUI(Define.UIChat.UILobbyChat);
		uichat.AddChat(nickname, chat);
		uichat.SetScrollbarDown();
	}

	public static void LobbyUpdateInfo_UserList(PacketReader _reader)
	{
		// TODO : UIPage로 로직 옮기기

		TextMeshProUGUI tmp;

		byte page = _reader.GetByte();

		GameObject userListObj = UIManager.Inst.FindUI(Define.UI.UILobby_UserList);
		GameObject uiPageObj = Util.FindChild(userListObj, false, "Page");
		tmp = uiPageObj.GetComponent<TextMeshProUGUI>();
		tmp.text = "페이지 : " + page;

		UIPage uiPage = userListObj.GetComponent<UIPage>();
		uiPage.CurPage = page;
		GameObject obj, item;
		int userNum = _reader.GetByte();
		int i = 0;
		int activeCount = 0;
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
				tmp.text = _reader.GetByte().ToString();

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

		GameObject roomListObj = UIManager.Inst.FindUI(Define.UI.UILobby_RoomList);
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
			if (i < roomCount)
			{
				obj = Util.FindChild(item, false, "Title");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.text = _reader.GetString();
				obj = Util.FindChild(item, false, "Owner");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.text = _reader.GetString();
				obj = Util.FindChild(item, false, "Id");
				tmp = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
				tmp.text = _reader.GetByte().ToString();

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
	public static void CreateRoom(PacketReader _reader)
	{
		SceneManagerEx.Inst.LoadScene(Define.Scene.Room);
	}

	
}
