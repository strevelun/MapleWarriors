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

	TextMeshProUGUI m_text;
	ScrollRect m_scrollRect;
	Scrollbar m_scrollbar;

	const int MaxLine = 200;
	int curLineCnt = 0;

	Coroutine m_scrollbarDownCoroutine = null;

	void Start()
	{
		s_inst = this;
	}

	public void Init(GameObject _objInGameConsole)
	{
		m_scrollbar = Util.FindChild(_objInGameConsole, false, "Scrollbar").GetComponent<Scrollbar>();
		GameObject textObj = Util.FindChild(_objInGameConsole, true, "Text");
		m_scrollRect = _objInGameConsole.GetComponent<ScrollRect>();
		//GameObject textObj.transform.parent;
		m_text = textObj.GetComponent<TextMeshProUGUI>();
	}

	public void Log(string _text)
	{
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

	IEnumerator SetScrollbarDownCoroutine()
	{
		//Canvas.ForceUpdateCanvases();
		yield return null;
		m_scrollbar.value = 0f;
		m_scrollbarDownCoroutine = null;
	}

	private void OnDestroy()
	{
		
	}
}
