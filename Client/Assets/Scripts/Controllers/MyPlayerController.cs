using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class MyPlayerController : PlayerController
{
	private enum AttackDirEnum
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

	private bool m_bIsKeyDown = false;
	private bool m_bIsKeyUp = false;

	private byte LastByteDir = 0;

	private CinemachineVirtualCamera m_vcam;
	private int m_cameraIdx = 0;

	private void Start()
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

	private IEnumerator MovingCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.2f);
		
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

		if (Input.GetKeyDown(KeyCode.W))
		{
			m_bIsKeyDown = true;
			ByteDir |= (byte)DirEnum.Up;
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			m_bIsKeyDown = true;
			ByteDir |= (byte)DirEnum.Down;
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			m_bIsKeyDown = true;
			ByteDir |= (byte)DirEnum.Left;
		}
		if (Input.GetKeyDown(KeyCode.D))
		{
			m_bIsKeyDown = true;
			ByteDir |= (byte)DirEnum.Right;
		}

		byte temp;
		if (Input.GetKeyUp(KeyCode.W))
		{
			m_bIsKeyUp = true;
			temp = (byte)DirEnum.Up;
			ByteDir &= (byte)~temp;
		}
		if (Input.GetKeyUp(KeyCode.S))
		{
			m_bIsKeyUp = true;
			temp = (byte)DirEnum.Down;
			ByteDir &= (byte)~temp;
		}
		if (Input.GetKeyUp(KeyCode.A))
		{
			m_bIsKeyUp = true;
			temp = (byte)DirEnum.Left;
			ByteDir &= (byte)~temp;
		}
		if (Input.GetKeyUp(KeyCode.D))
		{
			m_bIsKeyUp = true;
			temp = (byte)DirEnum.Right;
			ByteDir &= (byte)~temp;
		}
	}

	public void HandleInputMovement()
	{
		if (m_bIsKeyUp && ByteDir == (byte)DirEnum.None)
		{
			m_bIsKeyUp = false;
			if (GameManager.Inst.PlayerCnt > 1)
			{
				Packet pkt = InGamePacketMaker.EndMove(transform.position);
				UDPCommunicator.Inst.SendAll(pkt);
			}
		}

		if (LastByteDir != 0 || m_bIsKeyDown || m_bIsKeyUp) // ByteDir가 0이 아니면서 m_bIsKeyUp이면 방향이 바뀐 것.
		{
			Packet pkt = InGamePacketMaker.BeginMove(transform.position, ByteDir);
			UDPCommunicator.Inst.SendAll(pkt);
			m_bIsKeyDown = false;
			m_bIsKeyUp = false;
			LastByteDir = 0;
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

		Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
		mousePos.y = -mousePos.y + 1.0f;
		Vector2Int mouseCellPos = MapManager.Inst.WorldToCell(mousePos.x, -mousePos.y);
		int xpos = mouseCellPos.x - CellPos.x;
		int ypos = CellPos.y - mouseCellPos.y;

		if (Input.GetMouseButtonDown(0))
		{
			LastByteDir = ByteDir;
			if (ByteDir != 0)
			{
				Packet pkt = InGamePacketMaker.EndMove(transform.position); 
				UDPCommunicator.Inst.SendAll(pkt);
			}

			if (CurSkill.GetSkillType() == SkillTypeEnum.Melee)
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

		SkillEnum curSkill = SkillEnum.None;

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			curSkill = SkillEnum.Slash;
		}
		else if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			curSkill = SkillEnum.Blast;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			curSkill = SkillEnum.Skill1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			curSkill = SkillEnum.Skill2;
		}

		if (curSkill != SkillEnum.None && curSkill != m_eCurSkill)
		{
			GameObject objSkillPanel = UIManager.Inst.SceneUI.FindUI("SkillPanel");
			GameObject chosen = Util.FindChild(objSkillPanel.transform.GetChild(((int)curSkill) - 1).gameObject, false, "Chosen");
			chosen.SetActive(true);
			chosen = Util.FindChild(objSkillPanel.transform.GetChild(((int)CurSkill.CurSkill) - 1).gameObject, false, "Chosen");
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
		Dir = DirEnum.None;
	}

	public override void OnChangeStage()
	{
		base.OnChangeStage();

		m_bIsKeyUp = false;
		m_bIsKeyDown = false;
	}
}
