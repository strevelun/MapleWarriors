using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	bool m_bIsKeyDown = false;
	bool m_bIsKeyUp = false;

	Vector2Int m_prevMouseCellPos = Vector2Int.zero;

	//eDir m_eDirInput = eDir.None;

    void Start()
    {
        
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

	public void InputAttack()
	{
		// 스킬을 활성화한 상태에서만 마우스 포지션 추적
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		mousePos.y = -mousePos.y + 1.0f;
		Vector2Int mouseCellPos = MapManager.Inst.WorldToCell(mousePos.x, -mousePos.y);

		Vector2Int dist = mouseCellPos - CellPos;
		if (m_prevMouseCellPos != mouseCellPos)
		{
			if(CheckAttackRange(dist.x, dist.y))
			{
				MapManager.Inst.SetAimTile(mouseCellPos.x, mouseCellPos.y);
			}

			MapManager.Inst.RemoveAimTile(m_prevMouseCellPos.x, m_prevMouseCellPos.y);
			m_prevMouseCellPos = mouseCellPos;
		}

		if (CheckAttackRange(dist.x, dist.y))
		{
			if (Input.GetMouseButtonDown(0))
			{
				// 선택된 스킬 정보
				MonsterController mc = MapManager.Inst.GetMonster(mouseCellPos.x, mouseCellPos.y);
				ChangeState(new PlayerAttackState(mc, "Attack_0"));
				if (mc)
				{
					Packet pkt = InGamePacketMaker.Attack(mc.name);
					NetworkManager.Inst.Send(pkt);

				}
			}
		}
	}

	bool CheckAttackRange(int _cellXPos, int _cellYPos)
	{
		if ((-AttackRange <= _cellXPos && _cellXPos <= AttackRange) 
			&& (-AttackRange <= _cellYPos && _cellYPos <= AttackRange)) return true;
		return false;
	}
}
