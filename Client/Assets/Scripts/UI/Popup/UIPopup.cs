using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
	Dictionary<string, UIButton> m_dicButton = new Dictionary<string, UIButton>();

	private void Awake()
	{
		UIButton[] btns = gameObject.GetComponentsInChildren<UIButton>();
		foreach (UIButton btn in btns)
		{
			m_dicButton.Add(btn.gameObject.name, btn);
		}
	}

    public void SetButtonAction(string _buttonName, Action _action)
	{
		UIButton btn;
		if(m_dicButton.TryGetValue(_buttonName, out btn))
		{
			btn.Init(_action);
		}
	}
}
