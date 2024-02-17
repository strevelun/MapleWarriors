using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIdleState : ICreatureState
{
	MonsterController m_mc = null;

	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
	}

	public void Update()
	{
	}

	public void Exit()
	{
	}

}
