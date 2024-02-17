using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : ICreatureState
{
	PlayerController m_pc = null;

	public void Enter(CreatureController _cs)
	{
		m_pc = _cs as PlayerController;
	}

	public void Update()
	{
	}

	public void Exit()
	{
	}
}
