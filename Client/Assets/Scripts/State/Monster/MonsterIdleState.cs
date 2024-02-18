using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIdleState : ICreatureState
{
	MonsterController m_mc = null;

	public bool CanEnter(CreatureController _cs)
	{
		// 지금바로 Idle이 될 수 없는 조건
		//if (m_mc.CurState.GetType() != typeof(MonsterRunState)) return false;
		//if (m_mc.CurState.GetType() != typeof(MonsterAttackState)) return false;
		//if (m_mc.CurState.GetType() != typeof(MonsterHitState)) return false;
		//if(m_mc.CurState.GetType() != typeof(MonsterDieState))

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
		m_mc.CheckMoveState();
	}

	public void FixedUpdate()
	{
	}

	public void Exit()
	{
	}

}
