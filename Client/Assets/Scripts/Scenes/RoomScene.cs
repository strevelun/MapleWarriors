using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomScene : BaseScene
{
	GameObject m_startBtn, m_readyBtn, m_standbyBtn;

	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.Scene.Room;

		UIScene uiScene = UIManager.Inst.SetSceneUI(Define.Scene.Room);

		{
			GameObject obj = Util.FindChild(uiScene.gameObject, false, "BackBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnBackBtnClicked);
		}

		{
			UIChat uichat = UIManager.Inst.AddUI(Define.UIChat.UIRoomChat);
			uichat.Init(RoomPacketMaker.SendChat, Define.UIChat.UIRoomChat);
		}

		UIManager.Inst.AddUI(Define.UI.UIRoom_Users);

		{
			GameObject parentObj = UIManager.Inst.AddUI(Define.UI.UIRoom_GamePanel);
			GameObject obj = Util.FindChild(parentObj, false, "MapChoiceBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnMapChoiceBtnClicked);

			m_startBtn = Util.FindChild(parentObj, false, "StartBtn");
			btn = m_startBtn.GetComponent<Button>();
			btn.onClick.AddListener(OnStartBtnClicked);

			m_readyBtn = Util.FindChild(parentObj, false, "ReadyBtn");
			btn = m_readyBtn.GetComponent<Button>();
			btn.onClick.AddListener(OnReadyBtnClicked);

			m_standbyBtn = Util.FindChild(parentObj, false, "StandbyBtn");
			btn = m_standbyBtn.GetComponent<Button>();
			btn.onClick.AddListener(OnStandbyBtnClicked);
		}

		{
			UIPopup popup = UIManager.Inst.AddUI(Define.UIPopup.UIGameStartFailPopup);
			GameObject obj = Util.FindChild(popup.gameObject, true, "OKBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				UIManager.Inst.HidePopupUI(Define.UIPopup.UIGameStartFailPopup);
			});
		}

		{ 
			UIPopup popup = UIManager.Inst.AddUI(Define.UIPopup.UIMapChoicePopup);
		}

		IsLoading = false;
		Packet pkt = RoomPacketMaker.ReqRoomUsersInfo();
		NetworkManager.Inst.Send(pkt);
	}

	public override void Clear()
	{
		base.Clear();
	}

	void OnBackBtnClicked()
	{
		Packet pkt = RoomPacketMaker.ExitRoom();
		NetworkManager.Inst.Send(pkt);
		Debug.Log("OnBackBtnClicked");
	}

	void OnMapChoiceBtnClicked()
	{
		UIManager.Inst.ShowPopupUI(Define.UIPopup.UIMapChoicePopup);
	}

	void OnStartBtnClicked()
	{
		Packet pkt = RoomPacketMaker.StartGame();
		NetworkManager.Inst.Send(pkt);
	}

	void OnReadyBtnClicked()
	{
		Packet pkt = RoomPacketMaker.RoomReady();
		NetworkManager.Inst.Send(pkt);
	}

	void OnStandbyBtnClicked()
	{
		Packet pkt = RoomPacketMaker.RoomStandby();
		NetworkManager.Inst.Send(pkt);
	}

	void Start()
    {
		Init();
	}

    void Update()
    {
        
    }
}
