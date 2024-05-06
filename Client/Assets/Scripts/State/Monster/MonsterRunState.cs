using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRunState : ICreatureState
{
	MonsterController m_mc = null;

	public bool CanEnter(CreatureController _cs)
	{

		return true;
	}

	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		m_mc.Anim.SetBool("Run", true);
	}

	public void Update()
	{
		m_mc.Attack();
	}

	public void FixedUpdate()
	{
		m_mc.UpdateMove(Time.fixedDeltaTime);
		m_mc.CheckMoveState();
		m_mc.Flip();
	}

	public void Exit()
	{
		m_mc.Anim.SetBool("Run", false);
	}

}
