using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitState : ICreatureState
{
	MonsterController m_mc;
	int m_damage;
	bool m_knockback = false;
	bool m_animStart = false;

	AnimatorStateInfo m_stateInfo;

	public bool CanEnter(CreatureController _cs)
	{
		return true;
	}

	// hit 애니메이션 도중 hit가 들어오면 처음부터 다시 
	public MonsterHitState(int _damage)
	{
		m_damage = _damage;
	}

	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		m_mc.Anim.SetTrigger("Hit"); // 이게 실행될때마다 항상 0부터 시작해야
		m_mc.Hit(m_damage);
	}

	public void Update()
	{
		UpdateAnimation();
		if (!m_knockback && m_animStart)
		{
			m_mc.Knockback(m_stateInfo.length);
			m_knockback = true;
		}

	}

	public void FixedUpdate()
	{

	}

	public void Exit()
	{

	}

	void UpdateAnimation()
	{
		m_stateInfo = m_mc.Anim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Hit"))
			m_animStart = true;

		Debug.Log(m_animStart);

		if (m_animStart && !m_stateInfo.IsName("Hit"))
		{
			if(IsDead())	
				m_mc.ChangeState(new MonsterDeadState());
			else			m_mc.ChangeState(new MonsterIdleState());

			Debug.Log($"MonsterHitState HP : {m_mc.HP}");

			return;
		}
	}

	bool IsDead()
	{
		if (m_mc.HP <= 0) return true;
		
		return false;
	}
}
