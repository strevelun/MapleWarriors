using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadState : ICreatureState
{
	PlayerController m_player;
	AnimatorStateInfo m_stateInfo;

	public bool CanEnter(CreatureController _cs)
	{
		return true;
	}

	public void Enter(CreatureController _cs)
	{
		m_player = _cs as PlayerController;
		m_player.Anim.SetBool("Dead", true);
	
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
		m_stateInfo = m_player.Anim.GetCurrentAnimatorStateInfo(0);

		if (m_stateInfo.normalizedTime >= 1.0f)
		{
			m_player.Die();
			return;
		}
	}
}
