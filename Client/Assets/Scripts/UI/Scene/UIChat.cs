using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChat : MonoBehaviour
{
	private TMP_InputField			m_input;
	private Scrollbar				m_scrollbar;
	private UIButton				m_btn;
	private GameObject				m_content;

	private const int m_chatLimit = 10;

	public uint ChatCount { get; set; } = 0;

	private void Init()
	{
		m_input = Util.FindChild<TMP_InputField>(gameObject);
		m_input.onEndEdit.AddListener(OnEndEdit);

		m_scrollbar = Util.FindChild<Scrollbar>(gameObject, true);
		//m_scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);

		GameObject obj = Util.FindChild(gameObject, true, "SendBtn");
		m_btn = obj.GetComponent<UIButton>();
		m_btn.Init(() => OnSendButtonClicked());

		m_content = Util.FindChild(gameObject, true, "Content");

		// 버튼 클릭 이벤트

	}

	void Start()
    {
		Init();
    }

    void Update()
	{ 
		//if(Input.GetKeyDown(KeyCode.Return))
		{
			//UIManager.Inst.AddChat(Define.UIChat.UILobbyChat, "TEST");
		}

		if (m_chatLimit < ChatCount)
		{
			--ChatCount;
		}
	}

	public void SetScrollbarDown() 
	{
		//if (m_inputEnterCount == 0 && m_scrollbarValue > 0f) return;

		m_scrollbar.value = -0.8f;

	}

	void OnSendButtonClicked()
	{
		if (string.IsNullOrWhiteSpace(m_input.text))			return;

		Packet packet = LobbyPacketMaker.SendChat(m_input.text);
		NetworkManager.Inst.Send(packet);
		m_input.text = string.Empty;
		m_input.Select();
		m_input.ActivateInputField();

		//++m_inputEnterCount;
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
			string curSceneType = SceneManagerEx.Inst.CurScene.SceneType.ToString();
			obj = ResourceManager.Inst.Instantiate("UI/Scene/" + curSceneType + "/UILobbyChatItem");
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
	}
}
