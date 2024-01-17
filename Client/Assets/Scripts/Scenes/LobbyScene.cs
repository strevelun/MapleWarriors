using System.Collections;
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
		SceneType = Define.Scene.Lobby;

        UIScene uiScene = UIManager.Inst.SetSceneUI(Define.Scene.Lobby);

        UIManager.Inst.AddUI(Define.UIChat.UILobbyChat);

        {
            GameObject parentObj = UIManager.Inst.AddUI(Define.UI.UILobby_RoomList);
			m_roomListPage = parentObj.GetComponent<UIPage>();
			m_roomListPage.Init(Define.RoomListPageMax, Define.RoomListMaxItemInPage, "UI/Scene/Lobby/", Define.UI.UILobby_RoomList, "Content");

			GameObject obj = Util.FindChild(parentObj, false, "PrevBtn");
            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(OnRoomListPrevBtnClicked);
            obj = Util.FindChild(parentObj, false, "NextBtn");
            btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(OnRoomListNextBtnClicked);
        }

		{
			GameObject parentObj = UIManager.Inst.AddUI(Define.UI.UILobby_UserList);
			m_userListPage = parentObj.GetComponent<UIPage>();
			m_userListPage.Init(Define.UserListPageMax, Define.UserListMaxItemInPage, "UI/Scene/Lobby/", Define.UI.UILobby_UserList, "Content");

			GameObject obj = Util.FindChild(parentObj, false, "PrevBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnUserListPrevBtnClicked);
			obj = Util.FindChild(parentObj, false, "NextBtn");
			btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnUserListNextBtnClicked);
		}

        {
            GameObject obj = Util.FindChild(uiScene.gameObject, false, "CreateRoomBtn");
            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(OnCreateBtnClicked);
            obj = Util.FindChild(uiScene.gameObject, false, "ExitBtn");
			btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnExitBtnClicked);
		}

		StartCoroutine(UpdateLobbyInfoCoroutine());

		UpdateLobbyInfo();
	}
	public override void Clear()
    {
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
		if (m_roomListPage.CurPage >= Define.RoomListPageMax) return;

		Packet pkt = new Packet();
		pkt.Add(PacketType.eClient.RoomListGetPageInfo)
			.Add((byte)(m_roomListPage.CurPage - 1));
		NetworkManager.Inst.Send(pkt);
    }   
    
    void OnRoomListNextBtnClicked()
	{
		if (m_roomListPage.CurPage <= 0) return;
		if (m_roomListPage.CurPage >= Define.RoomListPageMax) return;

		Packet pkt = new Packet();
		pkt.Add(PacketType.eClient.RoomListGetPageInfo)
			.Add((byte)(m_roomListPage.CurPage + 1));
		NetworkManager.Inst.Send(pkt);
	}

	void OnUserListPrevBtnClicked()
	{
		if (m_userListPage.CurPage <= 0) return;

		Packet pkt = new Packet();
		pkt.Add(PacketType.eClient.UserListGetPageInfo)
			.Add((byte)(m_userListPage.CurPage - 1));
		NetworkManager.Inst.Send(pkt);
		//++m_userListPage.BtnClickCount;
	}

	void OnUserListNextBtnClicked()
	{
		Debug.Log(m_userListPage.ActiveItemCount);
		if (m_userListPage.ActiveItemCount < Define.UserListMaxItemInPage) return;
		if (m_userListPage.CurPage >= Define.UserListPageMax - 1) return;

		Packet pkt = new Packet();
		pkt.Add(PacketType.eClient.UserListGetPageInfo)
			.Add((byte)(m_userListPage.CurPage + 1));
		NetworkManager.Inst.Send(pkt);
		//++m_userListPage.BtnClickCount;
		Debug.Log("NextBtn");
	}

	void OnCreateBtnClicked()
	{

	}

	void OnExitBtnClicked()
	{

	}
}
