using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class CreatureController : MonoBehaviour
{
	private enum State
	{
		None,
		Idle,
		Running,
		Attack
	}

	public enum Dir
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	private Animator m_anim;
	private State m_eState = State.Idle;
	protected Dir m_eDir;
	protected float m_maxSpeed = 4f;

	private float m_horizontal;
	private bool m_bIsFacingRight = true;

	public Vector3Int CellPos { get; private set; }

	void Start()
	{

	}

    protected virtual void Update()
	{
		Flip();

		switch (m_eDir)
		{
			case Dir.Up:
				transform.position = new Vector3(transform.position.x, transform.position.y + (m_maxSpeed * Time.deltaTime));
				break;
			case Dir.Down:
				transform.position = new Vector3(transform.position.x, transform.position.y - (m_maxSpeed * Time.deltaTime));
				break;
			case Dir.Left:
				transform.position = new Vector3(transform.position.x - (m_maxSpeed * Time.deltaTime), transform.position.y);
				break;
			case Dir.Right:
				transform.position = new Vector3(transform.position.x + (m_maxSpeed * Time.deltaTime), transform.position.y);
				break;
		}
    }

	public virtual void Init()
	{
		m_anim = GetComponent<Animator>();
		
	}

	public void SetDir(Dir _eDir) { m_eDir = _eDir; }


	private void Flip()
	{
		if (m_bIsFacingRight && m_horizontal < 0f || !m_bIsFacingRight && m_horizontal > 0f)
		{
			m_bIsFacingRight = !m_bIsFacingRight;
			Vector3 localScale = transform.localScale;
			localScale.x *= -1f;
			transform.localScale = localScale;
		}
	}
}
