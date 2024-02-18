using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class CreatureController : MonoBehaviour
{
	public ICreatureState CurState { get; private set; } = null;
	ICreatureState m_nextState = null;

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

	public Animator Anim { get; private set; }
	protected GameObject m_spriteObject;
	protected Vector2 CenterPos { get; private set; }
	SpriteRenderer m_spriteRenderer;
	//private State m_eState = State.Idle;
	public eDir Dir { get; set; } = eDir.None;
	public eDir LastDir { get; private set; } = eDir.None;
	public float MaxSpeed { get; protected set; } = 1f;

	private bool m_bIsFacingLeft = true;

	public Vector2Int CellPos { get; protected set; }
	public Vector2Int LastCellPos { get; protected set; }

	public int HP { get; set; }
	public int AttackDamage { get; protected set; }
	public int AttackRange { get; protected set; }

	void Start()
	{
	}

    protected virtual void Update()
	{
		CurState?.Update();
	}

	protected virtual void FixedUpdate()
	{
		CurState?.FixedUpdate();
	}

	public void ChangeState(ICreatureState _newState)
	{
		if(_newState.CanEnter(this) == false)
		{
			// 10명의 플레이어가 동시에 10번 몬스터 클릭하면 hit 애니가 10번 연속 순차적으로 재생 -> 마지막으로 때린 놈 기준(데미지는 전부 들어가야 : Enter할때 데미지 먹이기)
			m_nextState = _newState; // 두 명 이상의 플레이어가 동시에 몬스터를 클릭하면 
			return; 
		}
		if (CurState != null && CurState.GetType() == _newState.GetType()) return;

		CurState?.Exit();
		CurState = _newState;
		CurState.Enter(this);
	}

	public void UpdateMove()
	{
		float newX = transform.position.x, newY = transform.position.y;
		switch (Dir)
		{
			case eDir.Up:
				newY = transform.position.y + (MaxSpeed * Time.fixedDeltaTime);
				break;
			case eDir.Down:
				newY = transform.position.y - (MaxSpeed * Time.fixedDeltaTime);
				break;
			case eDir.Left:
				newX = transform.position.x - (MaxSpeed * Time.fixedDeltaTime);
				break;
			case eDir.Right:
				newX = transform.position.x + (MaxSpeed * Time.fixedDeltaTime);
				break;
		}

		if (Dir != eDir.None)
		{
			AdjustXPosition(ref newX, ref newY);
			AdjustYPosition(ref newX, ref newY);

			transform.position = new Vector3(newX, newY);
			Vector2Int tempCellPos = ConvertToCellPos(transform.position.x, transform.position.y);
			if (tempCellPos != CellPos)
				LastCellPos = CellPos;
			CellPos = tempCellPos;
			//Debug.Log($"{CellPos.x}, {CellPos.y}");
			CenterPos = new Vector2(m_spriteObject.transform.position.x, m_spriteObject.transform.position.y - 0.5f);
		}

	}

	void AdjustXPosition(ref float _newX, ref float _newY)
	{
		if (Dir != eDir.Left && Dir != eDir.Right) return;

		Vector2Int vec = MapManager.Inst.WorldToCell(_newX, _newY);
		float absNewY = Math.Abs(_newY);
		float testY = absNewY - Mathf.Floor(absNewY);

		int vecX = 0, newX = 0;
		if (Dir == eDir.Right)
		{
			vecX = vec.x + 1;
			newX = vec.x;
		}
		else if(Dir == eDir.Left)
		{
			vecX = vec.x;
			newX = vec.x + 1;
		}

		if ((testY != 0.0f && MapManager.Inst.IsBlocked(vecX, vec.y + 1)) // 0일때는 0번째 줄 이동 가능
						|| MapManager.Inst.IsBlocked(vecX, vec.y))
		{
			_newX = newX;
		}

		if (MapManager.Inst.IsBlocked(vecX, vec.y) && !MapManager.Inst.IsBlocked(vecX, vec.y + 1)) // Plant타일이 약간 띄어져 있어서 어색한 이동 방지 목적의 코드
		{
			float targetY = -vec.y - 1;
			if (Math.Abs(_newY - targetY) < 0.1f)
				_newY = targetY;
			else
				_newY -= (MaxSpeed * Time.fixedDeltaTime);
		}
		else
		{
			if (testY <= 0.4f)
			{
				if (!MapManager.Inst.IsBlocked(vecX, vec.y) && MapManager.Inst.IsBlocked(vecX, vec.y + 1))
				{
					float targetY = (float)Math.Round((double)_newY, MidpointRounding.AwayFromZero);
					if (Math.Abs(_newY - targetY) < 0.1f)
						_newY = targetY;
					else
						_newY += (MaxSpeed * Time.fixedDeltaTime);
				}
			}
			else if (testY >= 0.9f)
			{
				if (MapManager.Inst.IsBlocked(vecX, vec.y + 1) && !MapManager.Inst.IsBlocked(vecX, vec.y + 2))
				{
					float targetY = (float)Math.Round((double)_newY - 1, MidpointRounding.AwayFromZero);

					if (Math.Abs(_newY - targetY) < 0.1f)
						_newY = targetY;
					else
						_newY -= (MaxSpeed * Time.fixedDeltaTime);
				}

			}
		}
	}

	void AdjustYPosition(ref float _newX, ref float _newY)
	{
		if (Dir != eDir.Up && Dir != eDir.Down) return;

		Vector2Int vec = MapManager.Inst.WorldToCell(_newX, _newY);
		float absNewX = Math.Abs(_newX);
		float testX = absNewX - Mathf.Floor(absNewX);

		int vecY = 0, newY = 0;
		if (Dir == eDir.Up)
		{
			vecY = vec.y;
			newY = -(vec.y + 1);
		}
		else if (Dir == eDir.Down)
		{
			vecY = vec.y + 1;
			newY = -vec.y;
		}

		if ((testX != 0.0f && MapManager.Inst.IsBlocked(vec.x + 1, vecY))
						|| MapManager.Inst.IsBlocked(vec.x, vecY))
		{
			_newY = newY;
		}

		if (MapManager.Inst.IsBlocked(vec.x, vecY) && !MapManager.Inst.IsBlocked(vec.x + 1, vecY)) // Plant타일이 약간 띄어져 있어서 어색한 이동 방지 목적의 코드
		{
			float targetX = vec.x + 1;
			if (Math.Abs(_newX - targetX) < 0.1f)
				_newX = targetX;
			else
				_newX += (MaxSpeed * Time.fixedDeltaTime);
		}
		else
		{
			if (testX <= 0.4f)
			{
				if (!MapManager.Inst.IsBlocked(vec.x, vecY) && MapManager.Inst.IsBlocked(vec.x + 1, vecY))
				{
					float targetX = (float)Math.Round((double)_newX, MidpointRounding.AwayFromZero);
					if (Math.Abs(_newX - targetX) < 0.1f)
						_newX = targetX;
					else
						_newX -= (MaxSpeed * Time.fixedDeltaTime);
				}
			}
			else if (testX >= 0.9f)
			{
				if (MapManager.Inst.IsBlocked(vec.x + 1, vecY) && !MapManager.Inst.IsBlocked(vec.x + 2, vecY))
				{
					float targetX = (float)Math.Round((double)_newX - 1, MidpointRounding.AwayFromZero);

					if (Math.Abs(_newX - targetX) < 0.1f)
						_newX = targetX;
					else
						_newX += (MaxSpeed * Time.fixedDeltaTime);
				}

			}
		}
	}

	public virtual void Init(int _cellXPos, int _cellYPos)
	{
		m_spriteObject = transform.GetChild(0).gameObject;
		Anim = m_spriteObject.GetComponent<Animator>();
		m_spriteRenderer = m_spriteObject.GetComponent<SpriteRenderer>();
		CenterPos = new Vector2(m_spriteObject.transform.position.x, m_spriteObject.transform.position.y - 0.5f);

		SetPosition(_cellXPos, _cellYPos);
	}

	public void SetDir(eDir _eDir) 
	{
		//m_eDir |= _eDir; 
		Dir = _eDir;
	}
	/*
	public void UnSetDir(eDir _eDir)
	{
		Dir &= ~_eDir;
	}
	*/
	public void SetPosition(int _cellXPos, int _cellYPos) 
	{
		if (_cellXPos < 0 || _cellYPos < 0 || _cellXPos >= MapManager.Inst.XSize || _cellYPos >= MapManager.Inst.YSize) return;

		CellPos = new Vector2Int(_cellXPos, _cellYPos);
		LastCellPos = new Vector2Int(_cellXPos, _cellYPos);
		transform.position = new Vector3(_cellXPos, -_cellYPos); 
	}

	// 아래에서 올라올때 y=0이 되는 기준이 -0.5임. 그래서 몬스터가 -0.5에서 더이상 안올라가고 만족함
	public Vector2Int ConvertToCellPos(float _xpos, float _ypos)
	{
		return new Vector2Int((int)(_xpos + m_cellSize), (int)(-_ypos + m_cellSize));
	}

	public void Flip()
	{
		if ((Dir == eDir.Right && m_bIsFacingLeft)
			|| (Dir == eDir.Left && !m_bIsFacingLeft))
		{
			m_spriteRenderer.flipX = m_bIsFacingLeft;
			m_bIsFacingLeft = !m_bIsFacingLeft;
		}
	}
}
