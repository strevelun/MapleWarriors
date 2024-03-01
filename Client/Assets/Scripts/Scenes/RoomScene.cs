using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomScene : BaseScene
{
	protected override void Init()
	{
		// TODO : 모든 버튼에 UIButton을
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.eScene.Room;

		UIScene uiScene = UIManager.Inst.SetSceneUI(Define.eScene.Room);

		{
			GameObject obj = Util.FindChild(uiScene.gameObject, false, "BackBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnBackBtnClicked);
		}

		{
			UIChat uichat = UIManager.Inst.AddUI(Define.eUIChat.UIRoomChat);
			uichat.Init(RoomPacketMaker.SendChat, Define.eUIChat.UIRoomChat);
		}

		UIManager.Inst.AddUI(Define.eUI.UIRoom_Users);

		{
			GameObject parentObj = UIManager.Inst.AddUI(Define.eUI.UIRoom_GamePanel);
			GameObject obj = Util.FindChild(parentObj, false, "MapChoiceBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnMapChoiceBtnClicked);

			obj = UIManager.Inst.AddUI(Define.eUI.UIRoom_StartBtn);
			obj.transform.SetParent(parentObj.transform);
			RectTransform rectTransform = obj.GetComponent<RectTransform>();
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			if (!UserData.Inst.IsRoomOwner) obj.SetActive(false);
			UIButton uibtn = obj.GetComponent<UIButton>();
			uibtn.Init(OnStartBtnClicked, obj);

			obj = UIManager.Inst.AddUI(Define.eUI.UIRoom_ReadyBtn);
			obj.transform.SetParent(parentObj.transform);
			rectTransform = obj.GetComponent<RectTransform>();
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			if (UserData.Inst.IsRoomOwner) obj.SetActive(false);
			uibtn = obj.GetComponent<UIButton>();
			obj.transform.SetParent(parentObj.transform);
			uibtn.Init(OnReadyBtnClicked, obj);
			
			obj = UIManager.Inst.AddUI(Define.eUI.UIRoom_StandbyBtn);
			obj.transform.SetParent(parentObj.transform);
			rectTransform = obj.GetComponent<RectTransform>();
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			obj.SetActive(false);
			uibtn = obj.GetComponent<UIButton>();
			obj.transform.SetParent(parentObj.transform);
			uibtn.Init(OnStandbyBtnClicked, obj);
		}

		{
			UIPopup popup = UIManager.Inst.AddUI(Define.eUIPopup.UIGameStartFailPopup);
			GameObject obj = Util.FindChild(popup.gameObject, true, "OKBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				UIManager.Inst.HidePopupUI(Define.eUIPopup.UIGameStartFailPopup);
			});
		}

		{ 
			UIPopup popup = UIManager.Inst.AddUI(Define.eUIPopup.UIMapChoicePopup);
			GameObject obj = Util.FindChild(popup.gameObject, true, "CancelBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				UIManager.Inst.HidePopupUI(Define.eUIPopup.UIMapChoicePopup);
			});
		}

		IsLoading = false;
		Packet pkt = RoomPacketMaker.ReqRoomUsersInfo();
		NetworkManager.Inst.Send(pkt);


		InputManager.Inst.SetInputEnabled(false);
		StartFadeCoroutine();
	}

	public override void Clear()
	{
		base.Clear();
	}

	void OnBackBtnClicked()
	{
		Packet pkt = RoomPacketMaker.ExitRoom();
		NetworkManager.Inst.Send(pkt);
	}

	void OnMapChoiceBtnClicked()
	{
		UIManager.Inst.ShowPopupUI(Define.eUIPopup.UIMapChoicePopup);
	}

	void OnStartBtnClicked(GameObject _obj)
	{
		Packet pkt = RoomPacketMaker.StartGame();
		NetworkManager.Inst.Send(pkt);
	}

	void OnReadyBtnClicked(GameObject _obj)
	{
		UIButton uibtn = _obj.GetComponent<UIButton>();
		if (uibtn.IsActive == false) return;

		uibtn.IsActive = false;

		Packet pkt = RoomPacketMaker.RoomReady();
		NetworkManager.Inst.Send(pkt);
	}

	void OnStandbyBtnClicked(GameObject _obj)
	{
		UIButton uibtn = _obj.GetComponent<UIButton>();
		if (uibtn.IsActive == false) return;

		uibtn.IsActive = false;

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
