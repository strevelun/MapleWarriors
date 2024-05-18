using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadState : ICreatureState
{
	private PlayerController m_pc = null;
	private MyPlayerController m_mpc = null;
	private AnimatorStateInfo m_stateInfo;

	public bool CanEnter(CreatureController _cs)
	{
		if (_cs.CurState is PlayerDeadState) return false;

		return true;
	}

	public void Enter(CreatureController _cs)
	{
		if (_cs is MyPlayerController)
		{
			m_mpc = _cs as MyPlayerController;
			m_mpc.Anim.SetBool("Dead", true);
		}
		else if (_cs is PlayerController)
		{
			m_pc = _cs as PlayerController;
			m_pc.Anim.SetBool("Dead", true);
		}
	}

	public void Update()
	{
		UpdateAnimation();
		if (m_mpc) m_mpc.InputDead();
	}

	public void FixedUpdate()
	{

	}

	public void Exit()
	{
		if(m_mpc)
		{
			m_mpc.SetCameraFollowMe();
		}
	}

	private void UpdateAnimation()
	{
		if(m_mpc) m_stateInfo = m_mpc.Anim.GetCurrentAnimatorStateInfo(0);
		else if(m_pc) m_stateInfo = m_pc.Anim.GetCurrentAnimatorStateInfo(0);

		if (m_stateInfo.normalizedTime >= 1.0f)
		{
			if(m_mpc) m_mpc.Die();
			else if(m_pc) m_pc.Die();
			return;
		}
	}
}
