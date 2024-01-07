using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
	private static InputManager s_inst = null;
	public static InputManager Inst
	{
		get
		{
			return s_inst;
		}
	}

	public Action KeyAction = null; // listener ����. Ű���� �Է��� �����ϸ� �����ڵ鿡�� �Ѹ�

	private void Awake()
	{
		DontDestroyOnLoad(this);
		s_inst = GetComponent<InputManager>();
	}

	private void Update()
    {
		if (EventSystem.current.IsPointerOverGameObject()) return;
		if (Input.anyKey && KeyAction != null) KeyAction.Invoke();
	}
}
