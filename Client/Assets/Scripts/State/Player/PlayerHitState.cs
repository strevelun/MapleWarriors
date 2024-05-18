using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : ICreatureState
{
	private PlayerController m_player;
	private bool m_knockback = false;
	private bool m_animStart = false;

	private AnimatorStateInfo m_stateInfo;

	public PlayerHitState()
	{
		//m_monster = _mc;
	}

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

	private void UpdateAnimation()
	{
		m_stateInfo = m_player.Anim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Hit"))
			m_animStart = true;
		
		if (m_animStart && !m_stateInfo.IsName("Hit"))
		{
			if (m_player.IsDead)
			{
				m_player.ChangeState(new PlayerDeadState());
			}
			else
				m_player.ChangeState(new PlayerIdleState());
		}
	}
}
