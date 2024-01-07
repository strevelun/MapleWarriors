using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueue : MonoBehaviour
{
	private static ActionQueue s_inst = null;
	public static ActionQueue Inst
	{
		get
		{
			return s_inst;
		}
	}

	Queue<Action> m_action = new Queue<Action>();

	private void Awake()
	{
		DontDestroyOnLoad(this);
		s_inst = GetComponent<ActionQueue>();
	}

	private void Update()
	{
		while (true)
		{
			Action action = Dequeue();
			if (action == null) break;

			action.Invoke();
		}
	}

	public void Enqueue(Action _action)
	{
		m_action.Enqueue(_action);
	}

	public Action Dequeue()
	{
		if (m_action.Count <= 0) return null;

		return m_action.Dequeue();
	}

	public void Clear()
	{
		if (m_action.Count <= 0) return;

		m_action.Clear();
	}
}
