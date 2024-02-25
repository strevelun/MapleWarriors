using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDeadState : ICreatureState
{
	MonsterController m_mc;
	//bool m_animStart = false;
	AnimatorStateInfo m_stateInfo;

	public bool CanEnter(CreatureController _cs)
	{
		return true;
	}

	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		m_mc.Anim.SetBool("Dead", true);
		MapManager.Inst.RemoveMonster(m_mc.CellPos.x, m_mc.CellPos.y, m_mc.HitboxWidth, m_mc.HitboxHeight);
	}

	public void Update()
	{
		UpdateAnimation();
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

		//if (!m_animStart && m_stateInfo.IsName("Dead"))
		//	m_animStart = true;

		Debug.Log(m_stateInfo.normalizedTime);
		//if (m_animStart &&
		if(m_stateInfo.normalizedTime >= 1.0f)
		{
			m_mc.Die();
			return;
		}
	}
}
