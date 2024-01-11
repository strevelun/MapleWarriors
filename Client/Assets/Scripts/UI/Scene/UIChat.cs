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
	private RectTransform			m_content;

	private uint					m_inputEnterCount = 0;
	private float					m_scrollbarValue = 0f;
	private bool					m_isUserScrolling = false;

	private void Init()
	{
		m_input = Util.FindChild<TMP_InputField>(gameObject);
		m_input.onEndEdit.AddListener(OnEndEdit);

		m_scrollbar = Util.FindChild<Scrollbar>(gameObject, true);
		m_scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);

		GameObject obj = Util.FindChild(gameObject, true, "SendBtn");
		m_btn = obj.GetComponent<UIButton>();
		m_btn.Init(() => OnSendButtonClicked());

		m_content = Util.FindChild(gameObject, true, "Content").GetComponent<RectTransform>();

		// 버튼 클릭 이벤트
	}

	void Start()
    {
		Init();
    }

    void Update()
	{ 
    }

	private void OnScrollbarValueChanged(float _value)
	{
		m_scrollbarValue = _value;

		if (m_scrollbarValue > 0f)
		{
			m_isUserScrolling = true;
		}
	}

	public void SetScrollbarDown() 
	{
		//if (m_inputEnterCount == 0 && m_scrollbarValue > 0f) return;

		if (m_inputEnterCount != 0 && !m_isUserScrolling)
		{
			StartCoroutine(SetScrollbarAfterLayoutRebuild());
		}
	}

	private IEnumerator SetScrollbarAfterLayoutRebuild()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_content);
		yield return new WaitForEndOfFrame();
		m_scrollbar.value = 0;
		m_isUserScrolling = false;
	}

	void OnSendButtonClicked()
	{
		if (string.IsNullOrWhiteSpace(m_input.text))			return;

		Packet packet = LobbyPacketMaker.SendChat(m_input.text);
		NetworkManager.Inst.Send(packet);
		m_input.text = string.Empty;
		m_input.Select();
		m_input.ActivateInputField();

		++m_inputEnterCount;
	}

	private void OnEndEdit(string _text)
	{
		if (string.IsNullOrWhiteSpace(m_input.text)) return;

		if (Input.GetKeyDown(KeyCode.Return))
		{
			OnSendButtonClicked();
		}
	}
}
