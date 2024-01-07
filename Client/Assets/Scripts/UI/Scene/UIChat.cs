using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIChat : MonoBehaviour
{
	private TMP_InputField m_input;

	private void Init()
	{
		m_input = Util.FindChild<TMP_InputField>(gameObject);
		m_input.onEndEdit.AddListener(OnEndEdit);
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
		if (string.IsNullOrEmpty(m_input.text))			return;

		Packet packet = LobbyPacketMaker.SendChat(m_input.text);
		NetworkManager.Inst.Send(packet);
	}

	private void OnEndEdit(string _text)
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			OnLoginButtonClicked();
		}
	}
}
