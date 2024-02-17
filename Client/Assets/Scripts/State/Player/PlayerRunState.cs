using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : ICreatureState
{
	PlayerController m_pc = null;

	public void Enter(CreatureController _cs)
	{
		m_pc = _cs as PlayerController;
		m_pc.Anim.SetBool("IsRunning", true);
	}

	public void Update()
	{

	}

	public void Exit()
	{
		m_pc.Anim.SetBool("IsRunning", false);
	}
}
