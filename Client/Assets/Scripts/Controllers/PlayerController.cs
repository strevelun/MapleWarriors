using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : CreatureController
{
	public enum eState
	{
		None,
		Idle,
		Walking,
		Attack
	}

	TextMeshProUGUI m_nickname;
	string m_strNickname;

	[SerializeField]
	Vector2 m_offset = new Vector2(0.5f, 1.5f); 
	RectTransform m_nameTagUI; 

	public float m_smoothTime = 0.3f;
	Vector3 m_targetPosition;
	public float minDistance = 0.000002f;
	Vector3 m_velocity = Vector3.zero;
	bool m_updateEndMove = false;

	long m_moveTime = 0;

	public eState State { get; private set; } = eState.None;

	void Start()
	{
	}

	protected override void Update()
	{
		base.Update();
		HandleEndMove();

		m_nameTagUI.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)m_offset);

	}

	protected override void LateUpdate()
	{
		base.LateUpdate();

	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		m_maxSpeed = 8f;

		GameObject nickname = Util.FindChild(gameObject, true, "Nickname");
		m_nickname = nickname.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		m_nameTagUI = nickname.GetComponent<RectTransform>();

	}

	public void SetNickname(string _nickname)
	{
		m_strNickname = _nickname;
		m_nickname.text = m_strNickname;
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

	public void BeginMove()
	{

	}

	public void SetMoveTime(long _interval)
	{

	}

	public void EndMovePosition(long _tick)
	{
		if (Vector3.Distance(transform.position, m_targetPosition) < minDistance) return;

		float finalPosX = transform.position.x;
		float finalPosY = transform.position.y;
		float latency = (DateTime.Now.Ticks - _tick) / 10000000.0f;
		if(Dir == eDir.Left)
			finalPosX += m_maxSpeed * latency;
		else if(Dir == eDir.Right)
			finalPosX -= m_maxSpeed * latency;
		else if(Dir == eDir.Up)
			finalPosY -= m_maxSpeed * latency;
		else if (Dir == eDir.Down)
			finalPosY += m_maxSpeed * latency;

		//m_targetPosition = new Vector3(_destXPos, _destYPos);
		transform.position = new Vector3(finalPosX, finalPosY);
		CellPos = ConvertToCellPos(finalPosX, finalPosY);
		//m_updateEndMove = true;
	}

	public void HandleEndMove()
	{
		if (Dir != eDir.None)
		{
			m_updateEndMove = false;
			return;
		}

		if (m_updateEndMove)
			transform.position = Vector3.SmoothDamp(transform.position, m_targetPosition, ref m_velocity, m_smoothTime, m_maxSpeed, Time.deltaTime);

		if (m_updateEndMove && Vector3.Distance(transform.position, m_targetPosition) < minDistance)
		{
			m_updateEndMove = false;
			Debug.Log("º¸Á¤ ³¡");
		}
	}

	public void HandleBeginMove()
	{
		
	}
}
