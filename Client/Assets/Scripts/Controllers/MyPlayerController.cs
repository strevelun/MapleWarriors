using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

	//bool m_bMove = false;
	bool m_bIsKeyDown = false;
	bool m_bIsKeyUp = false;

	//bool m_bEndMove = false;
	bool[] m_endMoveCheck = new bool[4];

	byte LastByteDir = 0;

	CinemachineVirtualCamera m_vcam;
	int m_cameraIdx = 0;

	//int m_testMovingCnt = 0;

	void Start()
	{
		GameObject camObj = GameObject.Find("CM vcam1");
		m_vcam = camObj.GetComponent<CinemachineVirtualCamera>();

		StartCoroutine(MovingCoroutine());
	}

	protected override void Update()
	{
		InputMovement();

		base.Update();


		CurSkill.Update(CellPos, LastDir);

		//HandleInputMovement();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		m_nickname.GetComponent<Image>().color = new Color(69 / 255f, 144 / 255f, 255 / 255f, 190 / 255f);
	}

	IEnumerator MovingCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.2f);

			if (IsDead) continue;
		
			Packet pkt = InGamePacketMaker.Moving(transform.position, ByteDir);
			UDPCommunicator.Inst.SendAll(pkt);
		}
	}

	public void SetCameraFollowMe()
	{
		m_vcam.Follow = transform;
	}

	public void InputDead()
	{
		if (!IsDead) return;

		if(Input.GetKeyDown(KeyCode.Space))
		{
			m_cameraIdx = (m_cameraIdx + 1) % GameManager.Inst.PlayerCnt;
			GameManager.Inst.ChangeCamera(m_vcam, m_cameraIdx);
		}
	}

	public void InputMovement()
	{
		if (!GameManager.Inst.GameStart) return;
		if (!InputManager.Inst.InputEnabled) return;
		if (IsDead) return;

		/*
		if(ByteDir == 0 && m_bMove) // 스킬사용 후 이동 체크
		{
			if(Input.GetKey(KeyCode.W))
				ByteDir |= (byte)eDir.Up;
			if (Input.GetKey(KeyCode.S))
				ByteDir |= (byte)eDir.Down;
			if (Input.GetKey(KeyCode.A))
				ByteDir |= (byte)eDir.Left;
			if (Input.GetKey(KeyCode.D))
				ByteDir |= (byte)eDir.Right;

			Packet pkt = InGamePacketMaker.BeginMove(transform.position, ByteDir);
			UDPCommunicator.Inst.SendAll(pkt);
		}
		*/

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
			//InGameConsole.Inst.Log("DDDDDfdsfasfewtwertwrtwrDDDDDfdsfasfewtwertwrtwrDDDDDfdsfasfewtwertwrtwrDDDDDfdsfasfewtwertwrtwr");
		}

		//if (m_bIsKeyDown) m_bMove = true;

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

	public void HandleInputMovement()
	{
		if (m_bIsKeyUp && ByteDir == (byte)eDir.None)
		{
			long endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			m_bIsKeyUp = false;
			if (GameManager.Inst.PlayerCnt > 1)
			{
				//InGameConsole.Inst.Log("EndMove 직전");
				Packet pkt = InGamePacketMaker.EndMove(transform.position);
				UDPCommunicator.Inst.SendAll(pkt);
				//m_bEndMove = true;
			}
		}


		if (LastByteDir != 0 || m_bIsKeyDown || m_bIsKeyUp) // ByteDir가 0이 아니면서 m_bIsKeyUp이면 방향이 바뀐 것.
		{
			Packet pkt = InGamePacketMaker.BeginMove(transform.position, ByteDir);
			UDPCommunicator.Inst.SendAll(pkt);
			m_bIsKeyDown = false;
			m_bIsKeyUp = false;
			LastByteDir = 0;
			//m_bEndMove = false;
		}

		//if (ByteDir == 0) m_bMove = false;

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

		Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		mousePos.y = -mousePos.y + 1.0f;
		Vector2Int mouseCellPos = MapManager.Inst.WorldToCell(mousePos.x, -mousePos.y);
		int xpos = mouseCellPos.x - CellPos.x;
		int ypos = CellPos.y - mouseCellPos.y;

		if (Input.GetMouseButtonDown(0))
		{
			LastByteDir = ByteDir;
			//ByteDir = 0;
			if (ByteDir != 0)
			{
				Packet pkt = InGamePacketMaker.EndMove(transform.position); 
				UDPCommunicator.Inst.SendAll(pkt);
				//m_bEndMove = true;
			}

			if (CurSkill.GetSkillType() == eSkillType.Melee)
			{
				List<MonsterController> targets = new List<MonsterController>();
				bool activated = CurSkill.Activate(targets);

				if (UserData.Inst.IsRoomOwner)
				{
					if (activated)
					{
						ChangeState(new PlayerAttackState(CurSkill)); 
						foreach (MonsterController mc in targets)
							mc.Hit(CurSkill);
						Packet pkt = InGamePacketMaker.Attack((byte)UserData.Inst.MyRoomSlot, targets, m_eCurSkill);
						UDPCommunicator.Inst.SendAll(pkt);
					}
				}
				else
				{
					if (activated)
					{
						ChangeState(new PlayerAttackState(CurSkill));
						Packet pkt = InGamePacketMaker.AttackReq(mouseCellPos, m_eCurSkill);
						UDPCommunicator.Inst.Send(pkt, UserData.Inst.RoomOwnerSlot);
					}
				}
			}
			else
			{
				List<MonsterController> targets = new List<MonsterController>();
				bool activated = CurSkill.Activate(targets);
				SetRangedSkillObjPos(new Vector2Int(xpos, ypos));

				if (UserData.Inst.IsRoomOwner)
				{
					if (activated)
					{
						ChangeState(new PlayerAttackState(CurSkill));
						foreach (MonsterController mc in targets)
							mc.Hit(CurSkill);
						Packet pkt = InGamePacketMaker.RangedAttack((byte)UserData.Inst.MyRoomSlot, targets, m_eCurSkill, new Vector2Int(xpos, ypos));
						UDPCommunicator.Inst.SendAll(pkt);
					}
				}
				else
				{
					if (activated)
					{
						ChangeState(new PlayerAttackState(CurSkill));
						Packet pkt = InGamePacketMaker.RangedAttackReq(mouseCellPos, m_eCurSkill, new Vector2Int(xpos, ypos));
						UDPCommunicator.Inst.Send(pkt, UserData.Inst.RoomOwnerSlot);
						InGameConsole.Inst.Log($"{UserData.Inst.RoomOwnerSlot}에게 보냄");
					}
				}
			}
		}
	}

	public void InputSkillChoice()
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
		ByteDir = 0;
		Dir = eDir.None;
	}

	public override void OnChangeStage()
	{
		base.OnChangeStage();

		m_bIsKeyUp = false;
		m_bIsKeyDown = false;
	}
}
