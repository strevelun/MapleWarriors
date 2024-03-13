using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitState : ICreatureState
{
	MonsterController m_mc;
	bool m_knockback = false;
	bool m_animStart = false;

	AnimatorStateInfo m_stateInfo;

	// hit 애니메이션 도중 hit가 들어오면 처음부터 다시 
	public MonsterHitState()
	{
	}

	public bool CanEnter(CreatureController _cs)
	{
		if (_cs.CurState is MonsterAttackState) return false;
		if (_cs.CurState is MonsterDeadState) return false;

		return true;
	}

	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		m_mc.Anim.SetTrigger("Hit"); // 이게 실행될때마다 항상 0부터 시작해야
		//if(m_mc.CurState is MonsterRunState) m_mc.ChangeState(new MonsterIdleState());
	}

	public void Update()
	{
	
	}

	public void FixedUpdate()
	{
		UpdateAnimation();
		if (!m_knockback && m_animStart)
		{
			m_mc.Knockback(m_stateInfo.length);
			m_knockback = true;
		}
	}

	public void Exit()
	{

	}

	void UpdateAnimation()
	{
		m_stateInfo = m_mc.Anim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Hit"))
			m_animStart = true;

		//Debug.Log(m_stateInfo.IsName("Hit"));

		if (m_animStart && !m_stateInfo.IsName("Hit"))
		{
			if(m_mc.IsDead)		m_mc.ChangeState(new MonsterDeadState());
			else				m_mc.ChangeState(new MonsterIdleState());

			Debug.Log($"MonsterHitState HP : {m_mc.HP}");
		}
	}
}
