using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDeadState : ICreatureState
{
	MonsterController m_mc;

	public bool CanEnter(CreatureController _cs)
	{
		return true;
	}

	public void Enter(CreatureController _cs)
	{
		m_mc = _cs as MonsterController;
		m_mc.Anim.SetBool("Dead", true);
		MapManager.Inst.RemoveMonster(m_mc.CellPos.x, m_mc.CellPos.y);
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
	}

	public void Exit()
	{
		Debug.Log("DeadState Exit");
	}
}
