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

		UIChat uichat = UIManager.Inst.FindUI(Define.UIChat.UIRoomChat);
		uichat.AddChat(nickname, chat);
		uichat.SetScrollbarDown();
	}

	public static void ExitRoom(PacketReader _reader)
	{
		SceneManagerEx.Inst.LoadScene(Define.Scene.Lobby);
		Debug.Log("ExitRoom");
	}	
	
	public static void NotifyRoomUserExit(PacketReader _reader)
	{
		int idx = _reader.GetByte(); // 나간놈
		int prevOwnerIdx = _reader.GetByte(); 
		int nextOwnerIdx = _reader.GetByte(); 

		GameObject objRoomUsers = UIManager.Inst.FindUI(Define.UI.UIRoom_Users);
		GameObject slot = objRoomUsers.transform.GetChild(idx).gameObject;
		GameObject obj = Util.FindChild(slot, false, "CharacterBtn");
		obj.SetActive(false);
		obj = Util.FindChild(slot, false, "Nickname");
		obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = string.Empty;

		Debug.Log(prevOwnerIdx + ", " + nextOwnerIdx);

		if(prevOwnerIdx != nextOwnerIdx)
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
		}
	}	
	
	public static void NotifyRoomUserEnter(PacketReader _reader)
	{
		int idx = _reader.GetByte();
		string nickname = _reader.GetString();

		GameObject obj = UIManager.Inst.FindUI(Define.UI.UIRoom_Users);
		GameObject slot = obj.transform.GetChild(idx).gameObject;
		obj = Util.FindChild(slot, false, "CharacterBtn"); // 기본 캐릭터
		obj.SetActive(true);
		obj = Util.FindChild(slot, false, "Nickname");
		obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = nickname;
	}	
	
	public static void RoomUsersInfo(PacketReader _reader)
	{
		int idx;
		bool isOwner;
		string nickname;
		string roomTitle = _reader.GetString();
		int numOfUsers = _reader.GetByte();

		for(int i=0; i<numOfUsers; ++i)
		{
			idx = _reader.GetByte();
			isOwner = _reader.GetBool(); // 방장은 따로 표시
			nickname = _reader.GetString();

			GameObject objRoomUsers = UIManager.Inst.FindUI(Define.UI.UIRoom_Users);
			GameObject slot = objRoomUsers.transform.GetChild(idx).gameObject;
			GameObject obj = Util.FindChild(slot, false, "CharacterBtn");
			obj.SetActive(true);
			obj = Util.FindChild(slot, false, "Nickname");
			obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = nickname;
			obj = Util.FindChild(slot, false, "OwnerBadge");
			if(isOwner)		obj.SetActive(true);
			else			obj.SetActive(false);

		}
	}
}