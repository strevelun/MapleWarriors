using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitState : ICreatureState
{
	private MonsterController m_mc;
	private bool m_knockback = false;
	private bool m_animStart = false;

	private AnimatorStateInfo m_stateInfo;

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
		m_mc.Anim.SetTrigger("Hit");
	}

	public void Update()
	{
	
	}

	public void FixedUpdate()
	{
		UpdateAnimation();
		CheckState();

		if (!m_knockback && m_animStart)
		{
			m_mc.Knockback(m_stateInfo.length);
			m_knockback = true;
		}
	}

	public void Exit()
	{

	}

	private void UpdateAnimation()
	{
		m_stateInfo = m_mc.Anim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Hit"))
			m_animStart = true;
	}

	private void CheckState()
	{
		if (m_animStart && !m_stateInfo.IsName("Hit"))
		{
			if (m_mc.IsDead) m_mc.ChangeState(new MonsterDeadState());
			else m_mc.ChangeState(new MonsterIdleState());
		}
	}
}
