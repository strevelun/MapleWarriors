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

	TextMeshProUGUI m_nickname;
	string m_strNickname;

	public float m_smoothTime = 0.3f;
	Vector3 m_targetPosition;
	public float minDistance = 0.000002f;
	Vector3 m_velocity = Vector3.zero;
	bool m_updateEndMove = false;

	public eState State { get; private set; } = eState.None;

	void Start()
	{
	}

	protected override void Update()
	{
		base.Update();

		HandleEndMove();
	}

	void LateUpdate()
	{
		
	}

	public override void Init(int _cellXPos, int _cellYPos)
	{
		base.Init(_cellXPos, _cellYPos);

		m_maxSpeed = 8f;

		GameObject nickname = Util.FindChild(gameObject, true, "Nickname");
		m_nickname = nickname.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

		m_nickname.text = m_strNickname;
	}

	public void SetNickname(string _nickname)
	{
		m_strNickname = _nickname;
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

	public void EndMovePosition(float _destXPos, float _destYPos)
	{
		if (Vector3.Distance(transform.position, m_targetPosition) < minDistance) return;

		m_targetPosition = new Vector3(_destXPos, _destYPos);
		CellPos = ConvertToCellPos(_destXPos, _destYPos);
		m_updateEndMove = true;
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
}
