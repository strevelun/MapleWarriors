using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : CreatureController
{
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

	public eState State { get; private set; } = eState.None;

	void Start()
	{
	}

	protected override void Update()
	{
		base.Update();
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
		AttackDamage = 5;
		AttackRange = 2;

		GameObject nickname = Util.FindChild(gameObject, true, "Nickname");
		m_nicknameTMP = nickname.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_nameTagUI = nickname.GetComponent<RectTransform>();

		GameObject position = Util.FindChild(gameObject, true, "Position");
		m_positionTMP = position.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_positionTagUI = position.GetComponent<RectTransform>();

		ChangeState(new PlayerIdleState());
	}

	public void CheckMoveState()
	{
		if (Dir == eDir.None)
			ChangeState(new PlayerIdleState());
		else
			ChangeState(new PlayerRunState());
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
}
