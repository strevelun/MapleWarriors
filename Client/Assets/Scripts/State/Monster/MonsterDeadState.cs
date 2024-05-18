using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDeadState : ICreatureState
{
	private MonsterController m_mc;
	private AnimatorStateInfo m_stateInfo;

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
	}

	public void FixedUpdate()
	{
		UpdateAnimation();
	}

	public void Exit()
	{
	}

	void UpdateAnimation()
	{
		m_stateInfo = m_mc.Anim.GetCurrentAnimatorStateInfo(0);

		if(m_stateInfo.normalizedTime >= 1.0f)
		{
			m_mc.Die();
			return;
		}
	}
}
