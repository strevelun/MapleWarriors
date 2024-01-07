using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private enum PlayerState
	{
		None,
		Idle,
		Running,
		Jumping,
		Inair,
		Attack
	}

	private enum Attack
	{
		None,
		Shoot,
	}

	private const float GroundedThreshold = 0.05f;

	private PlayerState m_eState = PlayerState.Idle;
	private Attack m_eCurAttackSkill = Attack.Shoot;

	private Rigidbody2D m_rb;
	[SerializeField] private LayerMask m_groundLayer;
	private CapsuleCollider2D m_capsuleCollider;
	private Animator m_anim;

	private bool m_bIsGrounded = true;
	private float m_horizontal;
	private float m_maxSpeed = 4f;
	private float m_jumpingPower = 8f;
	private bool m_bIsFacingRight = true;

	void Start()
	{
		m_capsuleCollider = GetComponent<CapsuleCollider2D>();
		m_rb = GetComponent<Rigidbody2D>();
		m_anim = GetComponent<Animator>();

		InputManager.Inst.KeyAction -= InputKeyboard;
		InputManager.Inst.KeyAction += InputKeyboard;
	}

	void Update()
	{
		Flip();

		switch (m_eState)
		{
			case PlayerState.Idle:
				UpdateIdle();
				break;
			case PlayerState.Running:
				UpdateRunning();
				break;
			case PlayerState.Jumping:
				UpdateJumping();
				break;
			case PlayerState.Inair:
				UpdateInair();
				break;
			case PlayerState.Attack:
				UpdateAttack();
				break;
		}
	}

	private void FixedUpdate()
	{
		IsGrounded();
	}

	private void InputKeyboard()
	{
		switch(m_eState)
		{
			case PlayerState.Idle:
				InputMovement();
				InputAttack();
				break;
			case PlayerState.Running:
				InputMovement();
				break;
			case PlayerState.Jumping:
			case PlayerState.Inair:
				m_horizontal = Input.GetAxisRaw("Horizontal");
				break;
		}
	}

	private void InputAttack()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			m_anim.SetBool(m_eCurAttackSkill.ToString(), true);
		}
		else if (Input.GetKeyUp(KeyCode.Q))
		{
			m_anim.SetBool(m_eCurAttackSkill.ToString(), false);
		}
	}

	private void IsGrounded()
	{
		m_bIsGrounded = Physics2D.OverlapCapsule(m_capsuleCollider.bounds.center, m_capsuleCollider.bounds.size, CapsuleDirection2D.Horizontal, 0f, m_groundLayer) && m_rb.velocity.y < 0.01f;
	}

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

	private void UpdateAttack()
	{
	}

	private void InputMovement()
	{
		m_horizontal = Input.GetAxisRaw("Horizontal");
		m_rb.velocity = new Vector2(m_horizontal * m_maxSpeed, m_rb.velocity.y);
		//bool isRunning = m_horizontal != 0f && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A));
		bool isRunning = (m_horizontal != 0f);//&& (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)); 
		m_eState = isRunning ? PlayerState.Running : PlayerState.Idle;
	}

	private void UpdateIdle()
	{
		m_anim.SetBool("isRunning", false);

		if (m_bIsGrounded && Input.GetButton("Jump"))
		{
			m_eState = PlayerState.Jumping;
			m_anim.SetBool("isJumping", true);
		}
	}

	private void UpdateRunning()
	{
		m_anim.SetBool("isRunning", true);

		if (m_bIsGrounded && Input.GetButton("Jump"))
		{
			m_eState = PlayerState.Jumping;
			m_anim.SetBool("isJumping", true);
			m_anim.SetBool("isInair", false);
		}
		else if (!m_bIsGrounded)
		{
			m_eState = PlayerState.Inair;
			m_anim.SetBool("isInair", true);
		}
	}

	private void UpdateJumping() // 점프를 누르는 순간 한번만 호출됨
	{
		m_rb.velocity = new Vector2(m_horizontal * m_maxSpeed, m_jumpingPower);

		m_anim.SetBool("isInair", true);

		m_eState = PlayerState.Inair;
	}

	private void UpdateInair() // Inair상태에서 계속 호출되어야 함
	{
		m_rb.velocity = new Vector2(m_horizontal * m_maxSpeed, m_rb.velocity.y);

		if (m_bIsGrounded)
		{
			m_anim.SetBool("isInair", false);
			m_anim.SetBool("isJumping", false);
			bool isRunning = m_horizontal != 0f && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A));
			m_eState = isRunning ? PlayerState.Running : PlayerState.Idle;
		}
	}
}
