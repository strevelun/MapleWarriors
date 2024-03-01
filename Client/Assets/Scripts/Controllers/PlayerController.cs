using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
	public enum eState
	{
		None,
		Idle,
		Walking,
		Attack
	}

	protected Skill m_curSkill = null;

	GameObject m_tombstoneAnimObj;

	GameObject m_skillAnimObj;
	public Animator SkillAnim { get; private set; }
	protected eSkill m_eCurSkill = eSkill.None;
	protected eSkill m_eBeforeSkill = eSkill.None;

	TextMeshProUGUI m_nicknameTMP;
	//TextMeshProUGUI m_positionTMP;
	string m_strNickname;

	[SerializeField]
	Vector2 m_nameTagOffset = new Vector2(0.5f, 1.5f); 
	RectTransform m_nameTagUI;
	[SerializeField]
	//Vector2 m_positionTagOffset = new Vector2(0.5f, 2.5f);
	//RectTransform m_positionTagUI;

	public eState State { get; private set; } = eState.None;

	void Start()
	{
	}

	protected override void Update()
	{
		base.Update();

		/*
		if (Dir == eDir.Right)
		{
			Vector3 currentScale = m_skillAnimObj.transform.localScale;
			currentScale.x = -1;
			m_skillAnimObj.transform.localScale = currentScale;
		}
		else if(Dir == eDir.Left)
		{
			Vector3 currentScale = m_skillAnimObj.transform.localScale;
			currentScale.x = 1;
			m_skillAnimObj.transform.localScale = currentScale;
		}
		*/
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		//m_positionTMP.text = $"x = {transform.position.x}, y = {transform.position.y}";

		m_nameTagUI.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_nameTagOffset);
		//m_positionTagUI.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_positionTagOffset);
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		MaxSpeed = 4f;
		HP = 100;
		AttackDamage = 5;
		AttackRange = 2;

		HitboxWidth = 1;
		HitboxHeight = 1;

		//m_eCurSkill = eSkill.Slash;

		GameObject nickname = Util.FindChild(gameObject, true, "Nickname");
		m_nicknameTMP = nickname.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_nameTagUI = nickname.GetComponent<RectTransform>();

		//GameObject position = Util.FindChild(gameObject, true, "Position");
		//m_positionTMP = position.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		//m_positionTagUI = position.GetComponent<RectTransform>();

		m_skillAnimObj = Util.FindChild(gameObject, true, "Skill");
		SkillAnim = m_skillAnimObj.transform.GetChild(0).GetComponent<Animator>();

		m_tombstoneAnimObj = Util.FindChild(gameObject, true, "Tombstone");
		m_tombstoneAnimObj.SetActive(false);

		ChangeState(new PlayerIdleState());
	}

	// 매 프레임마다 new
	public void CheckMoveState()
	{
		if(CurState is PlayerIdleState)
		{
			if(Dir != eDir.None)
				ChangeState(new PlayerRunState());
			return;
		}
		if (CurState is PlayerRunState)
		{
			if (Dir == eDir.None)
				ChangeState(new PlayerIdleState());
			return;
		}	
	}

	public void SetNickname(string _nickname)
	{
		m_strNickname = _nickname;
		m_nicknameTMP.text = m_strNickname;
	}

	// _destXPos가 0이 나오는 경우
	public void EndMovePosition(float _destXPos, float _destYPos)
	{
		CellPos = ConvertToCellPos(_destXPos, _destYPos);

		transform.position = new Vector3(_destXPos, _destYPos);
	}

	public void PlayCurSkillAnim(eSkill _eSkill)
	{
		SkillAnim.Play(_eSkill.ToString());
	}

	public void Hit(int _damage)
	{
		if (HP <= 0) return;

		HP -= _damage;
	}

	public override void Die()
	{
		base.Die();

		m_tombstoneAnimObj.SetActive(true);

		GameManager.Inst.SubPlayerCnt();
	}

	public override void Flip()
	{
		/*
		eDir eAfterSkillPlayerDir = eDir.None;
		m_curSkill.SetSkillDir(m_skillAnimObj, ref eAfterSkillPlayerDir);
		if (eAfterSkillPlayerDir != eDir.None)
			LastDir = eAfterSkillPlayerDir;
		*/
		if (Dir == eDir.Right && m_bIsFacingLeft)
		{
			m_skillAnimObj.transform.localScale = new Vector3(-1, 1);
			Debug.Log("플립1");
		}
		else if(Dir == eDir.Left && !m_bIsFacingLeft)
		{
			m_skillAnimObj.transform.localScale = new Vector3(1, 1);
			Debug.Log("플립2");
		}


		base.Flip();
	}
}
