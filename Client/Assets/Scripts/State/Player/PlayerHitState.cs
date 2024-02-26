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

	public bool CanEnter(CreatureController _cs)
	{
		if (_cs.CurState is PlayerAttackState) return false;

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

	}

	void UpdateAnimation()
	{
		m_stateInfo = m_player.Anim.GetCurrentAnimatorStateInfo(0);

		if (!m_animStart && m_stateInfo.IsName("Hit"))
			m_animStart = true;

		if (m_animStart && !m_stateInfo.IsName("Hit"))
		{
			if (m_player.IsDead)
			{
				m_monster.DequeueTarget();
				m_player.ChangeState(new PlayerDeadState());
			}
			else m_player.ChangeState(new PlayerIdleState());

			Debug.Log($"PlayerHitState HP : {m_player.HP}, {m_player.IsDead}");
		}
	}
}
