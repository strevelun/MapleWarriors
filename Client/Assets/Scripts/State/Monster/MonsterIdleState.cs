using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIdleState : ICreatureState
{
	private MonsterController m_mc = null;

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
	}

	public void Update()
	{
		m_mc.UpdateMove(Time.deltaTime);
		m_mc.CheckMoveState();
		m_mc.Flip();
		m_mc.Attack();
	}

	public void FixedUpdate()
	{
	}

	public void Exit()
	{
	}

}
