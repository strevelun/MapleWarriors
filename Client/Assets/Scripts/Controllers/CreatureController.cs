using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class CreatureController : MonoBehaviour
{

	public enum eDir
	{
		None,
		Up = 0b1,
		Right = 0b10,
		Down = 0b100,
		Left = 0b1000,
		UpRight = 0b11,
		DownLeft = 0b1100,
		RightDown = 0b110,
		LeftUp = 0b1001,
	}

	const float m_cellSize = 0.5f;

	private Animator m_anim;
	//private State m_eState = State.Idle;
	public eDir Dir { get; protected set; } = eDir.None;
	protected float m_maxSpeed = 1f;

	private float m_horizontal;
	private bool m_bIsFacingRight = true;

	public Vector2Int CellPos { get; protected set; }

	void Start()
	{

	}

    protected virtual void Update()
	{
		Flip();

		switch (Dir)
		{
			case eDir.Up:
				transform.position = new Vector3(transform.position.x, transform.position.y + (m_maxSpeed * Time.deltaTime));
				break;
			case eDir.Down:
				transform.position = new Vector3(transform.position.x, transform.position.y - (m_maxSpeed * Time.deltaTime));
				break;
			case eDir.Left:
				transform.position = new Vector3(transform.position.x - (m_maxSpeed * Time.deltaTime), transform.position.y);
				break;
			case eDir.Right:
				transform.position = new Vector3(transform.position.x + (m_maxSpeed * Time.deltaTime), transform.position.y);
				break;
			case eDir.UpRight:
				transform.position = new Vector3(transform.position.x + (m_maxSpeed * Time.deltaTime), transform.position.y + (m_maxSpeed * Time.deltaTime));
				break;
			case eDir.RightDown:
				transform.position = new Vector3(transform.position.x + (m_maxSpeed * Time.deltaTime), transform.position.y - (m_maxSpeed * Time.deltaTime));
				break;
			case eDir.DownLeft:
				transform.position = new Vector3(transform.position.x - (m_maxSpeed * Time.deltaTime), transform.position.y - (m_maxSpeed * Time.deltaTime));
				break;
			case eDir.LeftUp:
				transform.position = new Vector3(transform.position.x - (m_maxSpeed * Time.deltaTime), transform.position.y + (m_maxSpeed * Time.deltaTime));
				break;
		}

		if (Dir != eDir.None)
		{
			CellPos = ConvertToCellPos(transform.position.x, transform.position.y);
			//.Log(CellPos);
		}
    }

	protected virtual void LateUpdate()
	{
		
	}

	public virtual void Init(int _cellXPos, int _cellYPos)
	{
		m_anim = GetComponent<Animator>();

		SetPosition(_cellXPos, _cellYPos);
	}

	public void SetDir(eDir _eDir) 
	{
		//m_eDir |= _eDir; 
		Dir = _eDir;
	}

	public void UnSetDir(eDir _eDir)
	{
		Dir &= ~_eDir;
	}

	public void SetPosition(int _cellXPos, int _cellYPos) 
	{
		if (_cellXPos < 0 || _cellYPos < 0 || _cellXPos >= MapManager.Inst.XSize || _cellYPos >= MapManager.Inst.YSize) return;

		CellPos = new Vector2Int(_cellXPos, _cellYPos);
		transform.position = new Vector3(_cellXPos, -_cellYPos); 
	}

	// 아래에서 올라올때 y=0이 되는 기준이 -0.5임. 그래서 몬스터가 -0.5에서 더이상 안올라가고 만족함
	public Vector2Int ConvertToCellPos(float _xpos, float _ypos)
	{
		return new Vector2Int((int)(_xpos + m_cellSize), (int)(-_ypos + m_cellSize));
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
}
