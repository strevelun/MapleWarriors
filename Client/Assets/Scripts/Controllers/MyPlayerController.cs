using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
	enum eAttackDir
	{
		None,
		LeftUp,
		Up,
		RightUp,
		Left,
		Right,
		LeftDown,
		Down,
		RightDown
	}

	Skill m_curSkill = null;

	KeyCode m_curKeyCode = KeyCode.None;
	bool m_bIsKeyDown = false;
	bool m_bIsKeyUp = false;


	//eDir m_eDirInput = eDir.None;

    void Start()
	{
		m_eCurSkill = eSkill.Slash;
		m_eBeforeSkill = eSkill.Slash;
		m_curSkill = new Skill(m_eCurSkill);
    }

    protected override void Update()
    {
		base.Update();

		if (Input.GetKeyUp(m_curKeyCode))
		{
			m_curKeyCode = KeyCode.None;
			m_bIsKeyUp = true;
			Dir = eDir.None;
		}

		InputSkillChoice();

		m_curSkill.Update(CellPos);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}
	
	// 이동 방향키를 누르고 있다가 다른 상태에서 방향키를 떼면 계속 이동함
	public void InputMovement()
	{
		if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.W))
		{
			m_curKeyCode = KeyCode.W;
			Dir = eDir.Up;
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.S))
		{
			m_curKeyCode = KeyCode.S;
			Dir = eDir.Down;
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.A))
		{
			m_curKeyCode = KeyCode.A;
			Dir = eDir.Left;
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.D))
		{
			m_curKeyCode = KeyCode.D;
			Dir = eDir.Right;
			m_bIsKeyDown = true;
		}

		HandleInputMovement();
	}

	void HandleInputMovement()
	{
		if (m_bIsKeyUp)
		{
			Packet pkt = InGamePacketMaker.EndMove(transform.position.x, transform.position.y);
			NetworkManager.Inst.Send(pkt);
			m_bIsKeyUp = false;
		}

		if (m_bIsKeyDown)
		{
			Packet pkt = InGamePacketMaker.BeginMove(Dir);
			NetworkManager.Inst.Send(pkt);
			m_bIsKeyDown = false;
		}
	}

	// SkillData의 정보를 토대로 
	public void InputAttack()
	{


		// 스킬을 활성화한 상태에서만 마우스 포지션 추적


		if (Input.GetMouseButtonDown(0))
		{
			List<MonsterController> targets = new List<MonsterController>();
			bool activated = m_curSkill.Activate(targets);
			if (activated)
			{
				ChangeState(new PlayerAttackState(targets, m_eCurSkill));
				Packet pkt = InGamePacketMaker.Attack(targets, m_eCurSkill); //mc ? mc.name : string.Empty);
				NetworkManager.Inst.Send(pkt);
			}
		}
	}

	void InputSkillChoice()
	{
		eSkill curSkill = eSkill.None;

		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
			curSkill = eSkill.Slash;
		}
		else if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			curSkill = eSkill.Blast;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			curSkill = eSkill.Skill1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			curSkill = eSkill.Skill2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			curSkill = eSkill.Skill3;
		}

		if (curSkill != eSkill.None && curSkill != m_eCurSkill)
		{
			m_eCurSkill = curSkill;
			m_curSkill.RemoveAimTiles();
			m_curSkill.SetSkill(curSkill);
		}
	}
}
