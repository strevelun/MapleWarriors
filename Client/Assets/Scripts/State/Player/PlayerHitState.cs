using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : ICreatureState
{
	PlayerController m_player;
	MonsterController m_monster;
	bool m_knockback = false;
	bool m_animStart = false;

	AnimatorStateInfo m_stateInfo;

	public PlayerHitState(MonsterController _mc)
	{
		m_monster = _mc;
	}

	// Hit했는데 갑자기 Run으로 변경

	public bool CanEnter(CreatureController _cs)
	{
	//	if (_cs.CurState is PlayerHitState) return false;
		if (_cs.CurState is PlayerAttackState) return false;
		if (_cs.CurState is PlayerDeadState) return false;

		return true;
	}

	public void Enter(CreatureController _cs)
	{
		m_player = _cs as PlayerController;
		m_player.Anim.SetTrigger("Hit");

	//	InGameConsole.Inst.Log("PlayerHitState");
	}

	public void Update()
	{
		UpdateAnimation();
		if (!m_knockback && m_animStart)
		{
			m_player.Knockback(m_stateInfo.length);
			m_knockback = true;
		}
	}


	public void FixedUpdate()
	{

	}

	public void Exit()
	{
		if(m_player.HitObj.activeSelf) m_player.HitObj.SetActive(false);
	}

	void UpdateAnimation()
	{
		m_stateInfo = m_player.Anim.GetCurrentAnimatorStateInfo(0);
/*
		if (m_stateInfo.normalizedTime >= 1.0f)
		{
			if (m_player.IsDead)
			{
				m_monster.RemoveTarget(m_player);
				m_player.ChangeState(new PlayerDeadState());
			}
			else
				m_player.ChangeState(new PlayerIdleState());
		}
*/

		if (!m_animStart && m_stateInfo.IsName("Hit"))
			m_animStart = true;
		
		if (m_animStart && !m_stateInfo.IsName("Hit"))
		{
			if (m_player.IsDead)
			{
				m_monster.RemoveTarget(m_player);
				m_player.ChangeState(new PlayerDeadState());
			}
			else
				m_player.ChangeState(new PlayerIdleState());
			//Debug.Log($"PlayerHitState HP : {m_player.HP}, {m_player.IsDead}");
		}
	}
}
