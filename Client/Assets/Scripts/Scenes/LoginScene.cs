using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginScene : BaseScene
{
	private TMP_InputField m_input;

	private Coroutine udpCoroutine;

	public override void Init()
	{
		base.Init();

		Application.runInBackground = true;
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
		Screen.SetResolution(1280, 720, false);
		SceneType = Define.SceneEnum.Login;
		GameObject obj, child;
		UIButton uiBtn;

		obj = UIManager.Inst.SetSceneUI(Define.SceneEnum.Login).gameObject;
		if (obj)
		{
			child = Util.FindChild(obj, true, "Input");
			if (child)
			{
				uiBtn = Util.FindChild<UIButton>(obj, true);
				m_input = child.GetComponent<TMP_InputField>();
				m_input.onEndEdit.AddListener(OnEndEdit);
				uiBtn.Init(() => OnLoginButtonClicked());
			}
		}
		UIPopup popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UILoginFailPopup_WrongInput);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UILoginFailPopup_WrongInput));

		popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UILoginFailPopup_AlreadyLoggedIn);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UILoginFailPopup_AlreadyLoggedIn));
		
		popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UILoginFailPopup_Full);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UILoginFailPopup_Full));

		popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UIConnectFailPopup);
		popup.SetButtonAction("OKBtn", () => OnConnectFailButtonClicked());

		popup = UIManager.Inst.AddUI(Define.UIPopupEnum.UIDisconnectPopup, false);
		popup.SetButtonAction("OKBtn", () => { 
			//UIManager.Inst.HidePopupUI(Define.eUIPopup.UIDisconnectPopup);
			Quit();
		});

		NetworkManager.Inst.Init(Define.ServerIP, Define.ServerPort);
		UDPCommunicator.Inst.Init();

		GameObject mappingMaintainer = new GameObject("MappingMaintainer");
		mappingMaintainer.AddComponent<MappingMaintainer>();

		InputManager.Inst.SetInputEnabled(false);
		IsLoading = false;
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

	private IEnumerator ConnectToServerUDP()
	{
		byte[] pkt = BitConverter.GetBytes(UserData.Inst.ConnectionID);

		while (true)
		{
			UDPCommunicator.Inst.Send(pkt, Define.ServerIP, Define.ServerPort);
			yield return new WaitForSeconds(1f);
		}
	}

	public void StartConnectToServerUDP()
	{
		if (udpCoroutine != null) return;

		udpCoroutine = StartCoroutine(ConnectToServerUDP());
	}

	public void StopConnectToServerUDP()
	{
		StopCoroutine(udpCoroutine);
	}

	private void OnLoginButtonClicked()
	{
		if (string.IsNullOrEmpty(m_input.text) || m_input.text.Contains(" "))
		{
			UIManager.Inst.ShowPopupUI(Define.UIPopupEnum.UILoginFailPopup_WrongInput);
			return;
		}

		UserData.Inst.Nickname = m_input.text;
		Packet packet = LoginPacketMaker.LoginReq(NetworkManager.Inst.MyConnection.LocalEndPoint.Address.GetAddressBytes());
		NetworkManager.Inst.Send(packet);
	}

	private void OnConnectFailButtonClicked()
	{
		UIManager.Inst.HidePopupUI(Define.UIPopupEnum.UIConnectFailPopup);
		Quit();
	}

	private void OnEndEdit(string _text)
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			OnLoginButtonClicked();
		}
	}
}
