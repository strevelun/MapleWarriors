using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionQueue 
{
	Queue<Action> m_actionDelayed = new Queue<Action>();

	public void Update()
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
		m_actionDelayed.Enqueue(_action);
	}

	public Action Dequeue()
	{
		if (m_actionDelayed.Count <= 0) return null;

		return m_actionDelayed.Dequeue();
	}

	public void Clear()
	{
		if (m_actionDelayed.Count <= 0) return;

		m_actionDelayed.Clear();
	}
}
