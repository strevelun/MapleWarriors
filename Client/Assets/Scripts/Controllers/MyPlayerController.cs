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


	KeyCode m_curKeyCode = KeyCode.None;
	KeyCode m_curSecondKeyCode = KeyCode.None;
	bool m_bIsKeyDown = false;
	bool m_bIsKeyUp = false;

	Dictionary<KeyCode, bool> m_keyPressed = new Dictionary<KeyCode, bool>();
	Dictionary<KeyCode, eDir> m_keyCodeDir = new Dictionary<KeyCode, eDir>()
	{
		{ KeyCode.W, eDir.Up },
		{ KeyCode.S, eDir.Down },
		{ KeyCode.A, eDir.Left },
		{ KeyCode.D, eDir.Right }
	};


	//int m_keyPressedCnt = 0;

	//eDir m_eDirInput = eDir.None;

    void Start()
	{
	}

	private void OnEnable()
	{
	}

	protected override void Update()
    {
		base.Update();

		InputMovement();

		if (!IsDead)
		{
			InputSkillChoice();
			CurSkill.Update(CellPos, LastDir);
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}
	
	public void InputMovement()
	{
		if (Input.GetKeyDown(KeyCode.W))
		{
			m_bIsKeyDown = true;
			ByteDir |= (byte)eDir.Up;
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			m_bIsKeyDown = true;
			ByteDir |= (byte)eDir.Down;
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			m_bIsKeyDown = true;
			ByteDir |= (byte)eDir.Left;
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			m_bIsKeyDown = true;
			ByteDir |= (byte)eDir.Right;
		}

		byte temp;
		if (Input.GetKeyUp(KeyCode.W))
		{
			m_bIsKeyUp = true;
			temp = (byte)eDir.Up;
			ByteDir &= (byte)~temp;
		}
		if (Input.GetKeyUp(KeyCode.S))
		{
			m_bIsKeyUp = true;
			temp = (byte)eDir.Down;
			ByteDir &= (byte)~temp;
		}
		if (Input.GetKeyUp(KeyCode.A))
		{
			m_bIsKeyUp = true;
			temp = (byte)eDir.Left;
			ByteDir &= (byte)~temp;
		}
		if (Input.GetKeyUp(KeyCode.D))
		{
			m_bIsKeyUp = true;
			temp = (byte)eDir.Right;
			ByteDir &= (byte)~temp;
		}

		if (ByteDir != 0)
		{
			if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
			{
				ByteDir = 0;
				m_bIsKeyUp = true;
			}
		}

		HandleInputMovement();
	}

	void HandleInputMovement()
	{
		if (m_bIsKeyUp && ByteDir == (byte)eDir.None)
		{
			Packet pkt = InGamePacketMaker.EndMove(transform.position);
			NetworkManager.Inst.Send(pkt);
			m_bIsKeyUp = false;
		}

		if (m_bIsKeyDown || m_bIsKeyUp)
		{
			Packet pkt = InGamePacketMaker.BeginMove(transform.position, ByteDir);
			NetworkManager.Inst.Send(pkt);
			m_bIsKeyDown = false;
			m_bIsKeyUp = false;
		}
	}

	// SkillData의 정보를 토대로 
	public void InputAttack()
	{


		// 스킬을 활성화한 상태에서만 마우스 포지션 추적


		if (Input.GetMouseButtonDown(0))
		{

			if (CurSkill.GetSkillType() == eSkillType.Melee)
			{
				List<MonsterController> targets = new List<MonsterController>();
				bool activated = CurSkill.Activate(targets);
				if (activated)
				{
					ChangeState(new PlayerAttackState(targets, CurSkill));
					Packet pkt = InGamePacketMaker.Attack(targets, m_eCurSkill);
					NetworkManager.Inst.Send(pkt);
				}
			}
			else
			{
				Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
				mousePos.y = -mousePos.y + 1.0f;
				Vector2Int mouseCellPos = MapManager.Inst.WorldToCell(mousePos.x, -mousePos.y);
				int xpos = mouseCellPos.x - CellPos.x;
				int ypos = CellPos.y - mouseCellPos.y;
				SetRangedSkillObjPos(new Vector2Int(xpos, ypos));

				List<MonsterController> targets = new List<MonsterController>();
				bool activated = CurSkill.Activate(targets);
				if (activated)
				{
					ChangeState(new PlayerAttackState(targets, CurSkill));
					Packet pkt = InGamePacketMaker.RangedAttack(targets, m_eCurSkill, new Vector2Int(xpos, ypos));
					NetworkManager.Inst.Send(pkt);
				}
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
			CurSkill.RemoveAimTiles();
			CurSkill.SetSkill(curSkill);
		}
	}

	public override void Die()
	{
		base.Die();

		CurSkill.RemoveAimTiles();
	}
}
