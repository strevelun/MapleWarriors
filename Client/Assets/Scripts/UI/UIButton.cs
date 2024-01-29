using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    private Action m_action;
    private Action<GameObject> m_action2;
    public bool IsActive { get; set; } = true;

    public void Init(Action _action)
    {
        m_action = _action;
        Button btn = GetComponent<Button>();

        btn.onClick.AddListener(()=>m_action());
    }

	public void Init(Action<GameObject> _action, GameObject _obj)
	{
		m_action2 = _action;
		Button btn = GetComponent<Button>();

		btn.onClick.AddListener(()=>m_action2(_obj));
	}
}
