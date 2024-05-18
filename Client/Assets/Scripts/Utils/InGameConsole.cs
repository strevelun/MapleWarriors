using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameConsole : MonoBehaviour
{
	private static InGameConsole s_inst = null;
	public static InGameConsole Inst { get { return s_inst; } }

	private TextMeshProUGUI m_text;
	private ScrollRect m_scrollRect;
	private Scrollbar m_scrollbar;

	private const int MaxLine = 150;
	private int curLineCnt = 0;

	private Coroutine m_scrollbarDownCoroutine = null;

	private bool m_inGame = false;

	private void Start()
	{
		s_inst = this;
	}

	public void Init(GameObject _objInGameConsole)
	{
		m_scrollbar = Util.FindChild(_objInGameConsole, false, "Scrollbar").GetComponent<Scrollbar>();
		GameObject textObj = Util.FindChild(_objInGameConsole, true, "Text");
		m_scrollRect = _objInGameConsole.GetComponent<ScrollRect>();
		m_text = textObj.GetComponent<TextMeshProUGUI>();
		m_inGame = true;
	}

	public void Log(string _text)
	{
		if (!m_inGame) return;

		if (curLineCnt >= MaxLine)
		{
			m_text.text = m_text.text.Substring(m_text.text.IndexOf('\n') + 1);
		}
		else
			++curLineCnt;

		m_text.text += $"{_text}\n";

		if (m_scrollRect.verticalNormalizedPosition <= 0.1f)
			m_scrollRect.verticalNormalizedPosition = 0;

		if(m_scrollbarDownCoroutine == null) m_scrollbarDownCoroutine = StartCoroutine(SetScrollbarDownCoroutine());
	}

	public void GameOver()
	{
		m_inGame = false;
	}

	private IEnumerator SetScrollbarDownCoroutine()
	{
		yield return new WaitForEndOfFrame();
		m_scrollbar.value = 0f;
		m_scrollbarDownCoroutine = null;
	}
}
