using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIChat : MonoBehaviour
{
	private Func<string, Packet>	m_sendBtnFunc;
	private TMP_InputField			m_input;
	private Scrollbar				m_scrollbar;
	private UIButton				m_btn;
	private GameObject				m_content;
	private string					m_curSceneType;
	private string					m_uiChat;
	private string					m_uiChatItemPath;

	private const int m_chatLimit = 300;

	public uint ChatCount { get; set; } = 0;

	private Coroutine m_scrollbarCoroutine = null;

	public void Init(Func<string, Packet> _sendBtnFunc, Define.UIChatEnum _uiChat)
	{
		m_sendBtnFunc = _sendBtnFunc;
		m_curSceneType = SceneManagerEx.Inst.CurScene.SceneType.ToString();
		m_uiChat = _uiChat.ToString();
		m_uiChatItemPath = "UI/Scene/" + m_curSceneType + "/" + m_uiChat + "Item";

		m_input = Util.FindChild<TMP_InputField>(gameObject);
		m_input.onEndEdit.AddListener(OnEndEdit);

		m_scrollbar = Util.FindChild<Scrollbar>(gameObject, true);

		GameObject obj = Util.FindChild(gameObject, true, "SendBtn");
		m_btn = obj.GetComponent<UIButton>();
		m_btn.Init(() => OnSendButtonClicked());

		m_content = Util.FindChild(gameObject, true, "Content");
	}

	private void Update()
	{ 
		if (m_chatLimit < ChatCount)
		{
			--ChatCount;
		}
	}

	private IEnumerator SetScrollbarDownCoroutine()
	{
		yield return null;
		yield return null;
		m_scrollbar.value = 0f;
		m_scrollbarCoroutine = null;
	}

	private void OnSendButtonClicked()
	{
		if (string.IsNullOrWhiteSpace(m_input.text))			return;
		if (m_sendBtnFunc == null)								return;

		Packet packet = m_sendBtnFunc.Invoke(m_input.text);
		NetworkManager.Inst.Send(packet);
		m_input.text = string.Empty;
		m_input.Select();
		m_input.ActivateInputField();
	}

	private void OnEndEdit(string _text)
	{
		if (string.IsNullOrWhiteSpace(m_input.text)) return;

		if (Input.GetKeyDown(KeyCode.Return))
		{
			OnSendButtonClicked();
		}
	}

	public void AddChat(string _nickname, string _text)
	{
		GameObject obj;
		TextMeshProUGUI tmp;

		if (ChatCount < m_chatLimit)
		{
			obj = ResourceManager.Inst.Instantiate(m_uiChatItemPath);
			tmp = obj.GetComponent<TextMeshProUGUI>();
			tmp.text = "[" + _nickname + "] : " + _text;
			obj.transform.SetParent(m_content.transform);

			++ChatCount;
		}
		else
		{
			Transform t = m_content.transform.GetChild(0);
			obj = t.gameObject;
			tmp = obj.GetComponent<TextMeshProUGUI>();
			tmp.text = "[" + _nickname + "] : " + _text;
			t.SetAsLastSibling();
		}
		if(m_scrollbarCoroutine == null)
			m_scrollbarCoroutine = StartCoroutine(SetScrollbarDownCoroutine());
	}
}
