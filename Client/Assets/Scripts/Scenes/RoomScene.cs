using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class RoomScene : BaseScene
{
	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.SceneEnum.Room;

		UIScene uiScene = UIManager.Inst.SetSceneUI(Define.SceneEnum.Room);

		{
			GameObject obj = Util.FindChild(uiScene.gameObject, false, "BackBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(OnBackBtnClicked);
			uiScene.AddUI("Title");
			uiScene.AddUI("RoomID");
		}

		{
			UIChat uichat = UIManager.Inst.AddUI(Define.UIChatEnum.UIRoomChat);
			uichat.Init(RoomPacketMaker.SendChat, Define.UIChatEnum.UIRoomChat);
		}

		{
			GameObject parentObj = UIManager.Inst.AddUI(Define.UIEnum.UIRoom_Users);
			for (int i = 0; i < 4; ++i)
			{
				GameObject slot = parentObj.transform.GetChild(i).gameObject;
				UIButton btn = slot.GetComponent<UIButton>();
				btn.Init(OnCharacterChoiceBtnClicked, slot);
			}
		}

		{
			GameObject parentObj = UIManager.Inst.AddUI(Define.UIEnum.UIRoom_GamePanel);
			GameObject obj = Util.FindChild(parentObj, false, "MapChoiceBtn");
			obj.GetComponent<Image>().sprite = ResourceManager.Inst.LoadImage($"MapProfile/map_0");
			UIButton uibtn = obj.GetComponent<UIButton>();
			uibtn.Init(OnMapChoiceBtnClicked, obj);
			if (!UserData.Inst.IsRoomOwner) uibtn.IsActive = false;

			obj = UIManager.Inst.AddUI(Define.UIEnum.UIRoom_StartBtn);
			obj.transform.SetParent(parentObj.transform);
			RectTransform rectTransform = obj.GetComponent<RectTransform>();
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			if (!UserData.Inst.IsRoomOwner) obj.SetActive(false);
			uibtn = obj.GetComponent<UIButton>();
			uibtn.Init(OnStartBtnClicked, obj);

			obj = UIManager.Inst.AddUI(Define.UIEnum.UIRoom_ReadyBtn);
			obj.transform.SetParent(parentObj.transform);
			rectTransform = obj.GetComponent<RectTransform>();
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			if (UserData.Inst.IsRoomOwner) obj.SetActive(false);
			uibtn = obj.GetComponent<UIButton>();
			obj.transform.SetParent(parentObj.transform);
			uibtn.Init(OnReadyBtnClicked, obj);
			
			obj = UIManager.Inst.AddUI(Define.UIEnum.UIRoom_StandbyBtn);
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
			UIPopup popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UIGameStartFailPopup);
			GameObject obj = Util.FindChild(popup.gameObject, true, "OKBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIGameStartFailPopup);
			});
		}

		{ 
			UIPopup popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UIMapChoicePopup);
			GameObject obj = Util.FindChild(popup.gameObject, true, "CancelBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIMapChoicePopup);
			});
			obj = Util.FindChild(popup.gameObject, true, "Content");
			List<MapData> data = DataManager.Inst.GetAllMapData();
			foreach(MapData d in data)
			{
				GameObject popupBtn = ResourceManager.Inst.Instantiate("UI/Scene/Room/Popup/UIMapChoicePopupButton", obj.transform);
				popupBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Map_{d.name}";
				UIButton uibtn = popupBtn.GetComponent<UIButton>();
				uibtn.Init(() =>
				{
					UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIMapChoicePopup);
					Packet pkt = RoomPacketMaker.RoomMapChoice(int.Parse(d.name));
					NetworkManager.Inst.Send(pkt);
				});
			}
		}

		{ 
			UIPopup popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UICharacterChoicePopup);
			GameObject obj = Util.FindChild(popup.gameObject, true, "CancelBtn");
			Button btn = obj.GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UICharacterChoicePopup);
			});
			obj = Util.FindChild(popup.gameObject, true, "Content");
			for(int i= 1; i <= 2; ++i)
			{
				GameObject popupBtn = ResourceManager.Inst.Instantiate("UI/Scene/Room/Popup/UICharacterChoicePopupButton", obj.transform);
				popupBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Player{i}";
				UIButton uibtn = popupBtn.GetComponent<UIButton>();
				int choice = i;
				uibtn.Init(() =>
				{
					UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UICharacterChoicePopup);
					Packet pkt = RoomPacketMaker.RoomCharacterChoice(choice);
					NetworkManager.Inst.Send(pkt);
				});
			}
		}

		IsLoading = false;
		{
			Packet pkt = RoomPacketMaker.ReqRoomUsersInfo();
			NetworkManager.Inst.Send(pkt);
		}

		InputManager.Inst.SetInputEnabled(false);
		StartFadeCoroutine();
	}

	public override void Clear()
	{
		base.Clear();
	}

	private void OnBackBtnClicked()
	{
		Packet pkt = RoomPacketMaker.ExitRoom();
		NetworkManager.Inst.Send(pkt);
	}

	private void OnMapChoiceBtnClicked(GameObject _obj)
	{
		UIButton uibtn = _obj.GetComponent<UIButton>();
		if (uibtn.IsActive == false) return;

		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UIMapChoicePopup);
	}

	private void OnCharacterChoiceBtnClicked(GameObject _obj)
	{
		UIButton uibtn = _obj.GetComponent<UIButton>();
		if (uibtn.IsActive == false) return;

		UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UICharacterChoicePopup);
	}

	private void OnStartBtnClicked(GameObject _obj)
	{
		Packet pkt = RoomPacketMaker.StartGame();
		NetworkManager.Inst.Send(pkt);
	}

	private void OnReadyBtnClicked(GameObject _obj)
	{
		UIButton uibtn = _obj.GetComponent<UIButton>();
		if (uibtn.IsActive == false) return;

		uibtn.IsActive = false;

		Packet pkt = RoomPacketMaker.RoomReady();
		NetworkManager.Inst.Send(pkt);
	}

	private void OnStandbyBtnClicked(GameObject _obj)
	{
		UIButton uibtn = _obj.GetComponent<UIButton>();
		if (uibtn.IsActive == false) return;

		uibtn.IsActive = false;

		Packet pkt = RoomPacketMaker.RoomStandby();
		NetworkManager.Inst.Send(pkt);
	}

	private void Start()
    {
		Init();
	}
}
