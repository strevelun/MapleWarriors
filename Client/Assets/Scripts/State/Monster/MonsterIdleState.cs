using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIdleState : ICreatureState
{
	MonsterController m_mc = null;

	public bool CanEnter(CreatureController _cs)
	{
		MonsterController mc = _cs as MonsterController;
		// 지금바로 Idle이 될 수 없는 조건
		if (mc.CurState is MonsterDeadState) return false;
 
		return true;
	}

	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		//if (m_mc.Dir != CreatureController.eDir.None)
		//	m_mc.Dir = CreatureController.eDir.None;
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
	}

}
