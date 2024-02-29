using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : ICreatureState
{
	PlayerController m_pc = null;
	MyPlayerController m_mpc = null;

	// AttackState에 들어가는 순간 다음 프레임으로 전환되기 전에 ChangeState
	public bool CanEnter(CreatureController _cs) // if(_cs is CreatureController ) return false;
	{
		/*
		if (_cs is MyPlayerController && (_cs as MyPlayerController).CurState is PlayerAttackState)
		{
			return false;
		}
		if (_cs is PlayerController && (_cs as PlayerController).CurState is PlayerAttackState)
		{
			return false;
		}
		*/

		return true;
	}

	public void Enter(CreatureController _cs)
	{
		if (_cs is MyPlayerController)
			m_mpc = _cs as MyPlayerController;
		else if (_cs is PlayerController)
			m_pc = _cs as PlayerController;
	}

	public void Update()
	{
		if (m_mpc)
		{
			m_mpc.InputMovement();
			m_mpc.CheckMoveState();
			m_mpc.InputAttack();
			m_mpc.Flip();
		}
		else
		{
			m_pc.CheckMoveState();
			m_pc.Flip();
		}
	}

	public void FixedUpdate()
	{
	}

	public void Exit()
	{
	}


}
