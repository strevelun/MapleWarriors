using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
	private static InputManager s_inst = null;
	public static InputManager Inst { get { return s_inst; } }

	EventSystem m_eventSystem;
	public bool InputEnabled { get; private set; } = true;

	private void Awake()
	{
		DontDestroyOnLoad(this);
		s_inst = this;
	}

	private void Start()
	{
		m_eventSystem = EventSystem.current;
		DontDestroyOnLoad(m_eventSystem.gameObject);
	}

	private void Update()
	{
	}

	public void SetInputEnabled(bool _enabled)
	{
		//Debug.Log($"SetInputEnabled : {_enabled}");
		InputEnabled = _enabled;
		m_eventSystem.enabled = _enabled;
	}
}
