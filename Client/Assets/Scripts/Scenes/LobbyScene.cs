using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : BaseScene
{
	UIPage m_userListPage;
	UIPage m_roomListPage;

	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.eScene.Lobby;

        UIScene uiScene = UIManager.Inst.SetSceneUI(Define.eScene.Lobby);
		{
			GameObject obj = Util.FindChild(uiScene.gameObject, false, "CreateRoomBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnCreateBtnClicked);
			obj = Util.FindChild(uiScene.gameObject, false, "ExitBtn");
			btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnExitBtnClicked);
		}

		{
			UIChat uichat = UIManager.Inst.AddUI(Define.eUIChat.UILobbyChat);
			uichat.Init(LobbyPacketMaker.SendChat, Define.eUIChat.UILobbyChat);
		}

		{
            GameObject parentObj = UIManager.Inst.AddUI(Define.eUI.UILobby_RoomList);
			m_roomListPage = parentObj.GetComponent<UIPage>();
			m_roomListPage.Init(Define.RoomListPageMax, Define.RoomListMaxItemInPage, "UI/Scene/Lobby/", Define.eUI.UILobby_RoomList, "Content");

			GameObject obj = Util.FindChild(parentObj, false, "PrevBtn");
            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(OnRoomListPrevBtnClicked);
            obj = Util.FindChild(parentObj, false, "NextBtn");
            btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(OnRoomListNextBtnClicked);

			obj = UIManager.Inst.AddUI(Define.eUI.UILobby_RoomList_Block);
			obj.transform.SetParent(parentObj.transform);
			RectTransform rectTransform = obj.GetComponent<RectTransform>();
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
		}

		{
			GameObject parentObj = UIManager.Inst.AddUI(Define.eUI.UILobby_UserList);
			m_userListPage = parentObj.GetComponent<UIPage>();
			m_userListPage.Init(Define.UserListPageMax, Define.UserListMaxItemInPage, "UI/Scene/Lobby/", Define.eUI.UILobby_UserList, "Content");

			GameObject obj = Util.FindChild(parentObj, false, "PrevBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnUserListPrevBtnClicked);
			obj = Util.FindChild(parentObj, false, "NextBtn");
			btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnUserListNextBtnClicked);
		}

		{
			UIPopup popup = UIManager.Inst.AddUI(Define.eUIPopup.UICreateRoomPopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				if (string.IsNullOrWhiteSpace(popup.InputField.text)) return;

				UIManager.Inst.HidePopupUI(Define.eUIPopup.UICreateRoomPopup);
				Packet pkt = LobbyPacketMaker.CreateRoom(popup.InputField.text);
				popup.InputField.text = string.Empty;
				NetworkManager.Inst.Send(pkt);
			});
			popup.SetButtonAction("CancelBtn", () => 
			{
				popup.InputField.text = string.Empty;
				UIManager.Inst.HidePopupUI(Define.eUIPopup.UICreateRoomPopup); 
			});
		}		
		
		{
			UIPopup popup = UIManager.Inst.AddUI(Define.eUIPopup.UICreateRoomFailPopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				UIManager.Inst.HidePopupUI(Define.eUIPopup.UICreateRoomFailPopup);
			});

			popup = UIManager.Inst.AddUI(Define.eUIPopup.UIEnterRoomFullPopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				UIManager.Inst.HidePopupUI(Define.eUIPopup.UIEnterRoomFullPopup);
			});

			popup = UIManager.Inst.AddUI(Define.eUIPopup.UIEnterRoomInGamePopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				UIManager.Inst.HidePopupUI(Define.eUIPopup.UIEnterRoomInGamePopup);
			});

			popup = UIManager.Inst.AddUI(Define.eUIPopup.UIEnterRoomNopePopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				UIManager.Inst.HidePopupUI(Define.eUIPopup.UIEnterRoomNopePopup);
			});
		}

		StartCoroutine(UpdateLobbyInfoCoroutine());

		UpdateLobbyInfo();

		IsLoading = false;

		InputManager.Inst.SetInputEnabled(false);
		StartFadeCoroutine();
	}

	public override void Clear()
	{
		base.Clear();
	}

    void Start()
    {
        Init(); 
	}

    void Update()
    {
	}

	IEnumerator UpdateLobbyInfoCoroutine()
	{
		while(true)
		{
			yield return new WaitForSeconds(Define.LobbyUpdateTime);
			UpdateLobbyInfo();
			//Debug.Log("º¸³¿");
			//Packet pkt = LobbyPacketMaker.Test();
			//NetworkManager.Inst.Send(pkt);
		}
	}

	void UpdateLobbyInfo()
	{
		//if (m_userListPage.BtnClickCount != 0) return;
		Packet pkt = LobbyPacketMaker.UpdateLobbyInfo(m_userListPage.CurPage, m_roomListPage.CurPage);
		NetworkManager.Inst.Send(pkt);
	}

	void OnRoomListPrevBtnClicked()
    {
		if (m_roomListPage.CurPage <= 0) return;

		Packet pkt = LobbyPacketMaker.RoomListGetPageInfo(m_roomListPage.CurPage - 1);
		NetworkManager.Inst.Send(pkt);
    }   
    
    void OnRoomListNextBtnClicked()
	{
		if (m_roomListPage.ActiveItemCount < Define.RoomListMaxItemInPage) return;
		if (m_roomListPage.CurPage >= Define.RoomListPageMax - 1) return;

		Packet pkt = LobbyPacketMaker.RoomListGetPageInfo(m_roomListPage.CurPage + 1);
		NetworkManager.Inst.Send(pkt);
	}

	void OnUserListPrevBtnClicked()
	{
		if (m_userListPage.CurPage <= 0) return;

		Packet pkt = LobbyPacketMaker.UserListGetPageInfo(m_userListPage.CurPage - 1);
		NetworkManager.Inst.Send(pkt);
	}

	void OnUserListNextBtnClicked()
	{
		Debug.Log(m_userListPage.ActiveItemCount);
		if (m_userListPage.ActiveItemCount < Define.UserListMaxItemInPage) return;
		if (m_userListPage.CurPage >= Define.UserListPageMax - 1) return;

		Packet pkt = LobbyPacketMaker.UserListGetPageInfo(m_userListPage.CurPage + 1);
		NetworkManager.Inst.Send(pkt);
		//Debug.Log("NextBtn");
	}

	void OnCreateBtnClicked()
	{
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UICreateRoomPopup);
	}

	void OnExitBtnClicked()
	{
		NetworkManager.Inst.Disconnect();
		Quit();
	}
}
