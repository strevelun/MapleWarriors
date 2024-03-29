﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackState : ICreatureState
{
	MonsterController m_mc;
	bool m_animStart = false;
	bool m_hit = false;
	AnimatorStateInfo m_stateInfo;
	List<PlayerController> m_targets;

	public MonsterAttackState(List<PlayerController> _targets)
	{
		m_targets = new List<PlayerController>(_targets);
	}

	public bool CanEnter(CreatureController _cs)
	{
		// 이동 중에는 불가능

		return true;
	}

	// 몬스터는 어택 도중 쳐맞을 수 있다. -> 바로 Hit애니 재생 후 체력 감소
	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		m_mc.Anim.SetTrigger("Attack");

		foreach (PlayerController pc in m_targets)
			pc.Hit(m_mc.AttackDamage);
	}

	public void Update()
	{

	}

	public void FixedUpdate()
	{
		UpdateAnimation();

	}

	public void Exit()
	{
		m_mc.AttackReady = false;
	}

	// 몬스터가 AttackSTate일때 맞으면 Attack 끝까지 재생
	void UpdateAnimation()
	{
		m_stateInfo = m_mc.Anim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Attack"))
			m_animStart = true;

		if (m_animStart && !m_stateInfo.IsName("Attack"))
		{
			m_mc.ChangeState(new MonsterIdleState());
			//Debug.Log($"몬스터 어택 끝 : {m_mc.Dir}");
			return;
		}

		// 여러명 공격 가능
		if (!m_hit && m_targets.Count > 0 && m_stateInfo.normalizedTime >= 0.3f)
		{
			foreach (PlayerController pc in m_targets)
			{
				if(pc.ChangeState(new PlayerHitState(m_mc)) == false)
				{
					if (pc.IsDead) pc.ChangeState(new PlayerDeadState());
				}
			}

			m_hit = true;
		}
	}
}
