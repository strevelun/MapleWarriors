using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIButton : MonoBehaviour
{
    private Action m_action;
    public bool IsActive { get; set; } = true;

    public void Init(Action _action)
    {
        m_action = _action;
        Button btn = GetComponent<Button>();
        //if (!btn) btn = this.AddComponent<Button>();

        btn.onClick.AddListener(() => m_action?.Invoke());
    }
}
