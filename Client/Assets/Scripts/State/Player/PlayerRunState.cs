﻿using System.Collections;
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
	}

	public void Update()
	{
		if (m_mpc)
		{
			m_mpc.CheckMoveState();
			m_mpc.InputAttack();
		}
		else
		{
			m_pc.CheckMoveState();
		}
	}

	public void FixedUpdate()
	{
		if (m_mpc)
		{
			m_mpc.UpdateMove();
			m_mpc.Flip();
		}
		else
		{
			m_pc.UpdateMove();
			m_pc.Flip();
		}
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
