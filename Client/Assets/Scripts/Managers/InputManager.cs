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

	public Action KeyAction = null; // listener 패턴. 키보드 입력을 감지하면 구독자들에게 뿌림

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
