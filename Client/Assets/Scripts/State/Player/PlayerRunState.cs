using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : ICreatureState
{
	PlayerController m_pc = null;
	MyPlayerController m_mpc = null;

	public bool CanEnter(CreatureController _cs)
	{
		return true;
	}

	public void Enter(CreatureController _cs)
	{
		if (_cs is MyPlayerController)
		{
			m_mpc = _cs as MyPlayerController;
			m_mpc.Anim.SetBool("Run", true);
		}
		else if (_cs is PlayerController)
		{
			m_pc = _cs as PlayerController;
			m_pc.Anim.SetBool("Run", true);
		}
		InGameConsole.Inst.Log("PlayerRunState");
	}

	public void Update()
	{
		if (m_mpc)
		{
			m_mpc.UpdateMove(Time.deltaTime);
			m_mpc.HandleInputMovement();
			m_mpc.Flip();
			m_mpc.CheckMoveState();
			m_mpc.InputAttack();
			m_mpc.InputSkillChoice();
		}
		else
		{
			m_pc.UpdateMove(Time.deltaTime);
			m_pc.Flip();
			m_pc.CheckMoveState();
		}
	}

	public void FixedUpdate()
	{
		/*
		if (m_mpc)
		{
			m_mpc.HandleInputMovement();
			m_mpc.UpdateMove(Time.fixedDeltaTime);
		}
		else
		{
			m_pc.UpdateMove(Time.fixedDeltaTime);
		}
		*/
	}

	public void Exit()
	{
		if (m_mpc)
		{
			m_mpc.Anim.SetBool("Run", false);
		}
		else
		{
			m_pc.Anim.SetBool("Run", false);
		}
	}
}
