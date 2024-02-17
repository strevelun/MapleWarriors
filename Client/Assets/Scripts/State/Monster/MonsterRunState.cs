using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRunState : ICreatureState
{
	MonsterController m_mc = null;

	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		m_mc.Anim.SetBool("IsRunning", true);
	}

	public void Update()
	{
	}

	public void Exit()
	{
		m_mc.Anim.SetBool("IsRunning", false);
	}

}
