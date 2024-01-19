using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
	Dictionary<string, UIButton> m_dicButton = new Dictionary<string, UIButton>();
	public TMP_InputField InputField { get; private set; } = null;


	public bool IsDestroyedOnLoad { private set; get; } = false;

	private void Awake()
	{
		UIButton[] btns = gameObject.GetComponentsInChildren<UIButton>();
		foreach (UIButton btn in btns)
		{
			m_dicButton.Add(btn.gameObject.name, btn);
		}

		GameObject input = Util.FindChild(gameObject, true, "Input");
		if(input)
			InputField = input.GetComponent<TMP_InputField>();
	}

    public void SetButtonAction(string _buttonName, Action _action)
	{
		UIButton btn;
		if(m_dicButton.TryGetValue(_buttonName, out btn))
		{
			btn.Init(_action);
		}
	}

	public void SetDestroyOnLoad()
	{
		DontDestroyOnLoad(this);
		IsDestroyedOnLoad = true;
	}
}
