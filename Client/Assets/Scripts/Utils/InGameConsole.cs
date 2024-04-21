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

	void Start()
	{
		s_inst = this;
	}

	public void Init(GameObject _objInGameConsole)
	{
		GameObject textObj = Util.FindChild(_objInGameConsole, true, "Text");
		m_scrollRect = _objInGameConsole.GetComponent<ScrollRect>();
		//GameObject textObj.transform.parent;
		m_text = textObj.GetComponent<TextMeshProUGUI>();
	}

	public void Log(string _text)
	{
		m_text.text += $"{_text}\n"; 

		if (m_scrollRect.verticalNormalizedPosition <= 0.1f)
			m_scrollRect.verticalNormalizedPosition = 0;
	}

	private void OnDestroy()
	{
		
	}
}
