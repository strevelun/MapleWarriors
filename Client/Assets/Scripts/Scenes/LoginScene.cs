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


		Application.runInBackground = true;
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
		Screen.SetResolution(1280, 720, false);
		SceneType = Define.eScene.Login;
		GameObject obj, child;
		UIButton uiBtn;

		obj = UIManager.Inst.SetSceneUI(Define.eScene.Login).gameObject;
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
		UIPopup popup = UIManager.Inst.AddUI(Define.eUIPopup.UILoginFailPopup_WrongInput);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.eUIPopup.UILoginFailPopup_WrongInput));

		popup = UIManager.Inst.AddUI(Define.eUIPopup.UILoginFailPopup_AlreadyLoggedIn);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.eUIPopup.UILoginFailPopup_AlreadyLoggedIn));
		
		popup = UIManager.Inst.AddUI(Define.eUIPopup.UILoginFailPopup_Full);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.eUIPopup.UILoginFailPopup_Full));

		popup = UIManager.Inst.AddUI(Define.eUIPopup.UIConnectFailPopup);
		popup.SetButtonAction("OKBtn", () => OnConnectFailButtonClicked());

		popup = UIManager.Inst.AddUI(Define.eUIPopup.UIDisconnectPopup, false);
		popup.SetButtonAction("OKBtn", () => { 
			UIManager.Inst.HidePopupUI(Define.eUIPopup.UIDisconnectPopup);
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
		});

		//NetworkManager.Inst.Init("119.67.216.164", 30001); // 포트포워딩
		NetworkManager.Inst.Init("192.168.219.149", 30001);
		//NetworkManager.Inst.Connect("118.32.36.41", 30001); // gpm

		//InputManager.Inst.KeyAction += OnKeyboardEnter;

		InputManager.Inst.SetInputEnabled(false);
		IsLoading = false;
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
		//base.Update();
	}

	void OnLoginButtonClicked()
	{
		if (string.IsNullOrEmpty(m_input.text) || m_input.text.Contains(" "))
		{
			UIManager.Inst.ShowPopupUI(Define.eUIPopup.UILoginFailPopup_WrongInput);
			return;
		}

		UserData.Inst.Nickname = m_input.text;
		//Debug.Log("just sent : " + UserData.Inst.Nickname);
		Packet packet = LoginPacketMaker.CheckNickname();
		NetworkManager.Inst.Send(packet);
	}

	void OnConnectFailButtonClicked()
	{
		UIManager.Inst.HidePopupUI(Define.eUIPopup.UIConnectFailPopup);

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
