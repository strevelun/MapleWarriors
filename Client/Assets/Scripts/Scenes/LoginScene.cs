using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginScene : BaseScene
{
	private TMP_InputField m_input;

	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.Scene.Login;
		GameObject obj, child;
		UIButton uiBtn;

		obj = UIManager.Inst.SetSceneUI(Define.Scene.Login).gameObject;
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
		UIPopup popup = UIManager.Inst.AddUI(Define.UIPopup.UILoginFailPopup_WrongInput);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.UIPopup.UILoginFailPopup_WrongInput));

		popup = UIManager.Inst.AddUI(Define.UIPopup.UILoginFailPopup_AlreadyLoggedIn);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.UIPopup.UILoginFailPopup_AlreadyLoggedIn));
		
		popup = UIManager.Inst.AddUI(Define.UIPopup.UILoginFailPopup_Full);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.UIPopup.UILoginFailPopup_Full));

		popup = UIManager.Inst.AddUI(Define.UIPopup.UIConnectFailPopup);
		popup.SetButtonAction("OKBtn", () => OnConnectFailButtonClicked());

		popup = UIManager.Inst.AddUI(Define.UIPopup.UIDisconnectPopup, false);
		popup.SetButtonAction("OKBtn", () => { 
			UIManager.Inst.HidePopupUI(Define.UIPopup.UIDisconnectPopup);
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
		});

		IsLoading = false;

		// NetworkManager.Inst.Connect("119.67.216.164", 30001); // 포트포워딩
		NetworkManager.Inst.Connect("192.168.219.173", 30001); 
		//NetworkManager.Inst.Connect("220.121.252.11", 30001); // gpm

		//InputManager.Inst.KeyAction += OnKeyboardEnter;
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

	void OnLoginButtonClicked()
	{
		if (string.IsNullOrEmpty(m_input.text) || m_input.text.Contains(" "))
		{
			UIManager.Inst.ShowPopupUI(Define.UIPopup.UILoginFailPopup_WrongInput);
			return;
		}

		UserData.Inst.Nickname = m_input.text;
		//Debug.Log("just sent : " + UserData.Inst.Nickname);
		Packet packet = LoginPacketMaker.CheckNickname();
		NetworkManager.Inst.Send(packet);
	}

	void OnConnectFailButtonClicked()
	{
		UIManager.Inst.HidePopupUI(Define.UIPopup.UIConnectFailPopup);

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
	}

	private void OnEndEdit(string _text)
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			OnLoginButtonClicked();
		}
	}
}
