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

		SceneType = Define.Scene.Login;
		GameObject obj, child;
		UIButton uiBtn;

		obj = UIManager.Inst.SetSceneUI(Define.Scene.Login).gameObject;
		if (obj)
		{
			child = Util.FindChild(obj, true, "LoginInput");
			if (child)
			{
				uiBtn = Util.FindChild<UIButton>(obj, true);
				m_input = child.GetComponent<TMP_InputField>();
				m_input.onEndEdit.AddListener(OnEndEdit);
				uiBtn.Init(() => OnLoginButtonClicked());
			}
		}

		UIPopup popup = UIManager.Inst.AddPopupUI(Define.UIPopup.UILoginFailPopup_WrongInput);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.UIPopup.UILoginFailPopup_WrongInput));

		popup = UIManager.Inst.AddPopupUI(Define.UIPopup.UILoginFailPopup_AlreadyLoggedIn);
		popup.SetButtonAction("OKBtn", () => UIManager.Inst.HidePopupUI(Define.UIPopup.UILoginFailPopup_AlreadyLoggedIn));

		popup = UIManager.Inst.AddPopupUI(Define.UIPopup.UIConnectFailPopup);
		popup.SetButtonAction("OKBtn", () => OnConnectFailButtonClicked());

		popup = UIManager.Inst.AddPopupUI(Define.UIPopup.UIDisconnectPopup, false);
		popup.SetButtonAction("OKBtn", () => { 
			UIManager.Inst.HidePopupUI(Define.UIPopup.UIDisconnectPopup);
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
		});

		NetworkManager.Inst.Connect("192.168.219.107", 30001);

		//InputManager.Inst.KeyAction += OnKeyboardEnter;
	}

	public override void Clear()
	{
		UIManager.Inst.Clear();
		ActionQueue.Inst.Clear();
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
		Debug.Log("just sent : " + UserData.Inst.Nickname);
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
