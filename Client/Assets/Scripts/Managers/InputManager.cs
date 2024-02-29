using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
	private static InputManager s_inst = null;
	public static InputManager Inst { get { return s_inst; } }

	public Action KeyAction = null;

	EventSystem m_eventSystem;
	bool m_inputEnabled = false;

	private void Awake()
	{
		DontDestroyOnLoad(this);
		s_inst = GetComponent<InputManager>();
	}

	private void Start()
	{
		m_eventSystem = EventSystem.current;
		DontDestroyOnLoad(m_eventSystem.gameObject);
	}

	private void Update()
	{
		if (!m_inputEnabled || !m_eventSystem.enabled) return;

		if (m_eventSystem.IsPointerOverGameObject()) return;
		if (Input.anyKey && KeyAction != null) KeyAction.Invoke();
	}

	public void SetInputEnabled(bool _enabled)
	{
		m_inputEnabled = _enabled;
		m_eventSystem.enabled = _enabled;
	}
}
