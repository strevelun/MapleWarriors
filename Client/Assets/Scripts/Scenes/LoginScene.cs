using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class LoginScene : BaseScene
{
	// SceneManagerEx.LoadScene("LobbyScene");

	protected override void Init()
	{
		base.Init();	

		SceneType = Define.Scene.Login;
		GameObject obj, child;
		UIButton uiBtn;

		obj = Managers.UI.SetSceneUI(Define.Scene.Login).gameObject;
		if (obj)
		{
			child = Util.FindChild(obj, true, "LoginInput");
			if(child)
			{
				uiBtn = Util.FindChild<UIButton>(obj, true);
				TMP_InputField input = child.GetComponent<TMP_InputField>();
				uiBtn.Init(() => OnLoginButtonClicked(input));
			}
		}

		UIPopup popupOK = Managers.UI.AddPopupUI<UIPopup>(Define.UIPopup.UILoginFailPopup);
		popupOK.SetButtonAction("OKBtn", () => Managers.UI.HidePopupUI(Define.UIPopup.UILoginFailPopup));

		popupOK = Managers.UI.AddPopupUI<UIPopup>(Define.UIPopup.UIConnectFailPopup);
		popupOK.SetButtonAction("OKBtn", () =>  OnConnectFailButtonClicked() );
		
	}

	public override void Clear()
	{
		Managers.UI.Clear();
		Managers.Action.Clear();
	}

	void Start()
	{
		Init();
	}

	void Update()
	{

	}

	void OnLoginButtonClicked(TMP_InputField _input)
	{
		if (string.IsNullOrEmpty(_input.text) || _input.text.Contains(" "))
		{
			Managers.UI.ShowPopupUI(Define.UIPopup.UILoginFailPopup);
			return;
		}

		Packet packet = LoginPacketMaker.CheckNickname(_input.text);
		Managers.Network.Send(packet);
	}

	void OnConnectFailButtonClicked()
	{
		Managers.UI.HidePopupUI(Define.UIPopup.UIConnectFailPopup);
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
	}
}
