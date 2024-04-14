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

	bool m_bIsKeyDown = false;
	bool m_bIsKeyUp = false;

	int m_endMovePlayerCnt = 0;
	bool m_bEndMove = false;
	bool[] m_endMoveCheck = new bool[4];
	List<int> m_otherPlayersSlot = null;

	int m_testMovingCnt = 0;

    void Start()
	{
		StartCoroutine(MovingCoroutine());
		StartCoroutine(MoveEndCheckCoroutine());
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

		HandleInputMovement();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	IEnumerator MovingCoroutine()
	{
		while (true)
		{
			if (Dir == eDir.None)
			{
				yield return null;
				continue;
			}

			yield return new WaitForSeconds(0.2f);

			++m_testMovingCnt;

			if (2 <= m_testMovingCnt && m_testMovingCnt <= 8) continue;

			Packet pkt = InGamePacketMaker.Moving(transform.position, ByteDir);
			UDPCommunicator.Inst.SendAll(pkt);
		}
	}

	IEnumerator MoveEndCheckCoroutine()
	{
		while(true)
		{
			if (!m_bEndMove)
			{
				yield return null;
				continue;
			}

			yield return new WaitForSeconds(0.2f);

			foreach(int slot in m_otherPlayersSlot)
			{
				if(m_endMoveCheck[slot] == false)
				{
					Packet pkt = InGamePacketMaker.EndMove(transform.position);
					UDPCommunicator.Inst.Send(pkt, slot);
				}
			}

			Debug.Log("EndMove 체크 중");
			
		}
	}

	public void SetEndCheck(int _slot)
	{
		if (!m_bEndMove) return;

		++m_endMovePlayerCnt;
		m_endMoveCheck[_slot] = true;

		Debug.Log($"{_slot}이 endMove 체크 완료");

		if (m_endMovePlayerCnt >= m_otherPlayersSlot.Count)
		{
			m_bEndMove = false;
			m_endMovePlayerCnt = 0;
			foreach (int slot in m_otherPlayersSlot)
			{
				m_endMoveCheck[slot] = false;
			}
		}
	}

	public void InputMovement()
	{
		if (!GameManager.Inst.GameStart) return;
		if (!InputManager.Inst.InputEnabled) return;

		//Debug.Log("InputDownMovement");
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
	}

	void HandleInputMovement()
	{
		if (m_bIsKeyUp && ByteDir == (byte)eDir.None)
		{
			m_bIsKeyUp = false;
			if (GameManager.Inst.PlayerCnt > 1)
			{
				Packet pkt = InGamePacketMaker.EndMove(transform.position);
				UDPCommunicator.Inst.SendAll(pkt);
				m_bEndMove = true;
			}
		}

		if (m_bIsKeyDown || m_bIsKeyUp) // ByteDir가 0이 아니면서 m_bIsKeyUp이면 방향이 바뀐 것.
		{
			Packet pkt = InGamePacketMaker.BeginMove(transform.position, ByteDir);
			UDPCommunicator.Inst.SendAll(pkt);
			m_bIsKeyDown = false;
			m_bIsKeyUp = false;
			m_bEndMove = false;


		}

		if (ByteDir != 0)
		{
			if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
			{
				ByteDir = 0;
				m_bIsKeyUp = true;
			}
		}
	}

	// SkillData의 정보를 토대로 
	public void InputAttack()
	{
		if (!GameManager.Inst.GameStart) return;
		if (!InputManager.Inst.InputEnabled) return;

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
					UDPCommunicator.Inst.SendAll(pkt);
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
					UDPCommunicator.Inst.SendAll(pkt);
				}
			}
		}
	}

	void InputSkillChoice()
	{
		if (!GameManager.Inst.GameStart) return;
		if (!InputManager.Inst.InputEnabled) return;

		// 스킬이펙트 재생 중 스킬 바꾸면 움직이는 버그

		eSkill curSkill = eSkill.None;

		if (Input.GetKeyDown(KeyCode.Alpha1))
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
		/*
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			curSkill = eSkill.Skill3;
		}
		*/
		if (curSkill != eSkill.None && curSkill != m_eCurSkill)
		{
			GameObject objSkillPanel = UIManager.Inst.SceneUI.FindUI("SkillPanel");
			GameObject chosen = Util.FindChild(objSkillPanel.transform.GetChild(((int)curSkill) - 1).gameObject, false, "Chosen");
			chosen.SetActive(true);
			chosen = Util.FindChild(objSkillPanel.transform.GetChild(((int)CurSkill.eCurSkill) - 1).gameObject, false, "Chosen");
			chosen.SetActive(false);

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

	public void SetOtherPlayerSlot(List<int> _slotList)
	{
		m_otherPlayersSlot = _slotList;
	}

	public override void OnChangeStage()
	{
		base.OnChangeStage();

		m_bIsKeyUp = false;
		m_bIsKeyDown = false;
	}
}
