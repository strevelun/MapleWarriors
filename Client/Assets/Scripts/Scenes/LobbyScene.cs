using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : BaseScene
{
	private UIPage m_userListPage;
	private UIPage m_roomListPage;

	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.SceneEnum.Lobby;

        UIScene uiScene = UIManager.Inst.SetSceneUI(Define.SceneEnum.Lobby);
		{
			GameObject obj = Util.FindChild(uiScene.gameObject, false, "CreateRoomBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnCreateBtnClicked);
			obj = Util.FindChild(uiScene.gameObject, false, "ExitBtn");
			btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnExitBtnClicked);
			obj = Util.FindChild(uiScene.gameObject, false, "FindRoomBtn");
			btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnFindRoomBtnClicked);
		}

		{
			UIChat uichat = UIManager.Inst.AddUI(Define.UIChatEnum.UILobbyChat);
			uichat.Init(LobbyPacketMaker.SendChat, Define.UIChatEnum.UILobbyChat);
		}

		{
            GameObject parentObj = UIManager.Inst.AddUI(Define.UIEnum.UILobby_RoomList);
			m_roomListPage = parentObj.GetComponent<UIPage>();
			m_roomListPage.Init(Define.RoomListPageMax, Define.RoomListMaxItemInPage, "UI/Scene/Lobby/", Define.UIEnum.UILobby_RoomList, "Content");

			GameObject obj = Util.FindChild(parentObj, false, "PrevBtn");
            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(OnRoomListPrevBtnClicked);
            obj = Util.FindChild(parentObj, false, "NextBtn");
            btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(OnRoomListNextBtnClicked);

			obj = UIManager.Inst.AddUI(Define.UIEnum.UILobby_RoomList_Block);
			obj.transform.SetParent(parentObj.transform);
			RectTransform rectTransform = obj.GetComponent<RectTransform>();
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
		}

		{
			GameObject parentObj = UIManager.Inst.AddUI(Define.UIEnum.UILobby_UserList);
			m_userListPage = parentObj.GetComponent<UIPage>();
			m_userListPage.Init(Define.UserListPageMax, Define.UserListMaxItemInPage, "UI/Scene/Lobby/", Define.UIEnum.UILobby_UserList, "Content");

			GameObject obj = Util.FindChild(parentObj, false, "PrevBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnUserListPrevBtnClicked);
			obj = Util.FindChild(parentObj, false, "NextBtn");
			btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnUserListNextBtnClicked);
		}

		{
			UIPopup popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UICreateRoomPopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				if (string.IsNullOrWhiteSpace(popup.InputField.text)) return;

				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UICreateRoomPopup);
				Packet pkt = LobbyPacketMaker.CreateRoom(popup.InputField.text);
				popup.InputField.text = string.Empty;
				NetworkManager.Inst.Send(pkt);
			});
			popup.SetButtonAction("CancelBtn", () => 
			{
				popup.InputField.text = string.Empty;
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UICreateRoomPopup); 
			});
		}		
		
		{
			UIPopup popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UICreateRoomFailPopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UICreateRoomFailPopup);
			});

			popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UIEnterRoomFullPopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIEnterRoomFullPopup);
			});

			popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UIEnterRoomInGamePopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIEnterRoomInGamePopup);
			});

			popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UIEnterRoomNopePopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIEnterRoomNopePopup);
			});

			popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UIFindRoomPopup);
			popup.SetButtonAction("OKBtn", () =>
			{
				if (string.IsNullOrWhiteSpace(popup.InputField.text)) return;
				if (!int.TryParse(popup.InputField.text.Trim(), out int result)) return;

				Packet pkt = LobbyPacketMaker.EnterRoom(result);
				NetworkManager.Inst.Send(pkt);
			});
			popup.SetButtonAction("CancelBtn", () =>
			{
				popup.InputField.text = string.Empty;
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIFindRoomPopup);
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

	private void Start()
    {
        Init(); 
	}

	private IEnumerator UpdateLobbyInfoCoroutine()
	{
		while(true)
		{
			yield return new WaitForSeconds(Define.LobbyUpdateTime);
			UpdateLobbyInfo();
		}
	}

	private void UpdateLobbyInfo()
	{
		Packet pkt = LobbyPacketMaker.UpdateLobbyInfo(m_userListPage.CurPage, m_roomListPage.CurPage);
		NetworkManager.Inst.Send(pkt);
	}

	private void OnRoomListPrevBtnClicked()
    {
		if (m_roomListPage.CurPage <= 0) return;

		Packet pkt = LobbyPacketMaker.RoomListGetPageInfo(m_roomListPage.CurPage - 1);
		NetworkManager.Inst.Send(pkt);
    }

	private void OnRoomListNextBtnClicked()
	{
		if (m_roomListPage.ActiveItemCount < Define.RoomListMaxItemInPage) return;
		if (m_roomListPage.CurPage >= Define.RoomListPageMax - 1) return;

		Packet pkt = LobbyPacketMaker.RoomListGetPageInfo(m_roomListPage.CurPage + 1);
		NetworkManager.Inst.Send(pkt);
	}

	private void OnUserListPrevBtnClicked()
	{
		if (m_userListPage.CurPage <= 0) return;

		Packet pkt = LobbyPacketMaker.UserListGetPageInfo(m_userListPage.CurPage - 1);
		NetworkManager.Inst.Send(pkt);
	}

	private void OnUserListNextBtnClicked()
	{
		Debug.Log(m_userListPage.ActiveItemCount);
		if (m_userListPage.ActiveItemCount < Define.UserListMaxItemInPage) return;
		if (m_userListPage.CurPage >= Define.UserListPageMax - 1) return;

		Packet pkt = LobbyPacketMaker.UserListGetPageInfo(m_userListPage.CurPage + 1);
		NetworkManager.Inst.Send(pkt);
	}

	private void OnCreateBtnClicked()
	{
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UICreateRoomPopup);
	}

	private void OnExitBtnClicked()
	{
		NetworkManager.Inst.Disconnect();
	}

	private void OnFindRoomBtnClicked()
	{
		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UIFindRoomPopup);
	}
}
