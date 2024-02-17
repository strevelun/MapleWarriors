using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : CreatureController
{
	PlayerIdleState m_idleState = new PlayerIdleState();
	PlayerRunState m_runState = new PlayerRunState();

	public enum eState
	{
		None,
		Idle,
		Walking,
		Attack
	}

	TextMeshProUGUI m_nicknameTMP;
	TextMeshProUGUI m_positionTMP;
	string m_strNickname;

	[SerializeField]
	Vector2 m_nameTagOffset = new Vector2(0.5f, 1.5f); 
	RectTransform m_nameTagUI;
	[SerializeField]
	Vector2 m_positionTagOffset = new Vector2(0.5f, 2.5f);
	RectTransform m_positionTagUI;

	public float m_smoothTime = 0.3f;
	public float minDistance = 0.000001f;
	public float lerpMinDist = 0.01f;

	public eState State { get; private set; } = eState.None;

	void Start()
	{
	}

	protected override void Update()
	{
		base.Update();
		//HandleEndMove();



		if (Dir == eDir.None)
			ChangeState(m_idleState);
		else
			ChangeState(m_runState);
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();

	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		m_positionTMP.text = $"x = {transform.position.x}, y = {transform.position.y}";

		m_nameTagUI.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_nameTagOffset);
		m_positionTagUI.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_positionTagOffset);
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		MaxSpeed = 4f;
		HP = 100;
		Attack = 5;
		AttackRange = 1;

		GameObject nickname = Util.FindChild(gameObject, true, "Nickname");
		m_nicknameTMP = nickname.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_nameTagUI = nickname.GetComponent<RectTransform>();

		GameObject position = Util.FindChild(gameObject, true, "Position");
		m_positionTMP = position.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_positionTagUI = position.GetComponent<RectTransform>();

		ChangeState(m_idleState);
	}

	public void SetNickname(string _nickname)
	{
		m_strNickname = _nickname;
		m_nicknameTMP.text = m_strNickname;
	}



	private void InputAttack()
	{
		/*
		if (Input.GetKeyDown(KeyCode.Q))
		{
			m_anim.SetBool(m_eCurAttackSkill.ToString(), true);
		}
		else if (Input.GetKeyUp(KeyCode.Q))
		{
			m_anim.SetBool(m_eCurAttackSkill.ToString(), false);
		}
		*/
	}


	private void UpdateAttack()
	{
	}

	// _destXPos가 0이 나오는 경우
	public void EndMovePosition(float _destXPos, float _destYPos)
	{
		CellPos = ConvertToCellPos(_destXPos, _destYPos);
		/*
		m_targetPosition = new Vector3(_destXPos, _destYPos);

		if (Vector3.Distance(transform.position, m_targetPosition) > lerpMinDist)
		{
			m_updateEndMove = true;
			Debug.Log("updateEndMvoe");
			return;
		}
		*/

		//Debug.Log(Vector3.Distance(transform.position, new Vector3(_destXPos, _destYPos, 0)));

		transform.position = new Vector3(_destXPos, _destYPos);
	}
}
