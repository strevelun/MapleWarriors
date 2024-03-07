using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class CreatureController : MonoBehaviour
{
	public ICreatureState CurState { get; private set; } = null;
	//ICreatureState m_nextState = null;

	public enum eDir
	{
		None,
		Up = 1, // 0001
		Right = 2, // 0010
		Down = 4, // 0100
		Left = 8, // 1000
		UpLeft = 9, // 1001
		UpRight = 3, // 0011
		DownLeft = 12, // 1100
		DownRight = 6 // 0110
	}

	const float m_cellSize = 0.5f;

	public Animator Anim { get; private set; }
	protected GameObject m_spriteObject;
	protected Vector2 CenterPos { get; private set; }
	protected SpriteRenderer m_spriteRenderer;
	//private State m_eState = State.Idle;
	public byte ByteDir { get; set; } = 0;
	public eDir Dir { get; set; } = eDir.None;
	public eDir LastDir { get; protected set; } = eDir.None;
	public float MaxSpeed { get; protected set; } = 1f;


	Vector3 m_knockbackOrigin;
	Coroutine m_knockbackCoroutine = null;

	protected bool m_bIsFacingLeft = true;

	public Vector2Int CellPos { get; protected set; }
	public Vector2Int LastCellPos { get; protected set; }

	public int HP { get; set; }
	public int MaxHP { get; set; }
	public int AttackDamage { get; protected set; }
	public int AttackRange { get; protected set; }

	public int HitboxWidth { get; protected set; }
	public int HitboxHeight { get; protected set; }

	public bool IsDead { get { return HP <= 0; } }

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

	public bool ChangeState(ICreatureState _newState)
	{
		if(_newState.CanEnter(this) == false)
		{
			// 10명의 플레이어가 동시에 10번 몬스터 클릭하면 hit 애니가 10번 연속 순차적으로 재생 -> 마지막으로 때린 놈 기준(데미지는 전부 들어가야 : Enter할때 데미지 먹이기)
			//m_nextState = _newState; // 두 명 이상의 플레이어가 동시에 몬스터를 클릭하면 

			return false; 
		}
		if (CurState != null && CurState.GetType() == _newState.GetType()) return false;

		CurState?.Exit();
		CurState = _newState;
		CurState.Enter(this);
		return true;
	}

	public void UpdateMove()
	{
		if (ByteDir == (byte)eDir.UpRight)			Dir = eDir.UpRight;
		else if (ByteDir == (byte)eDir.UpLeft)		Dir = eDir.UpLeft;
		else if (ByteDir == (byte)eDir.DownLeft)	Dir = eDir.DownLeft;
		else if (ByteDir == (byte)eDir.DownRight)	Dir = eDir.DownRight;
		else if (ByteDir == (byte)eDir.Up)			Dir = eDir.Up;
		else if (ByteDir == (byte)eDir.Down)		Dir = eDir.Down;
		else if (ByteDir == (byte)eDir.Left)		Dir = eDir.Left;
		else if (ByteDir == (byte)eDir.Right)		Dir = eDir.Right;
		else										Dir = eDir.None;


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
			case eDir.UpLeft:
				newX = transform.position.x - (MaxSpeed * Time.fixedDeltaTime);
				newY = transform.position.y + (MaxSpeed * Time.fixedDeltaTime);
				break;
			case eDir.UpRight:
				newX = transform.position.x + (MaxSpeed * Time.fixedDeltaTime);
				newY = transform.position.y + (MaxSpeed * Time.fixedDeltaTime);
				break;
			case eDir.DownLeft:
				newX = transform.position.x - (MaxSpeed * Time.fixedDeltaTime);
				newY = transform.position.y - (MaxSpeed * Time.fixedDeltaTime);
				break;
			case eDir.DownRight:
				newX = transform.position.x + (MaxSpeed * Time.fixedDeltaTime);
				newY = transform.position.y - (MaxSpeed * Time.fixedDeltaTime);
				break;
		}

		Debug.Log(Dir);

		if (Dir != eDir.None)
		{
			AdjustXPosition(ref newX, ref newY);
			AdjustYPosition(ref newX, ref newY);

			

			Vector2Int tempCellPos = ConvertToCellPos(newX, newY);
			if (tempCellPos != CellPos)
			{
				LastCellPos = CellPos;
				CellPos = tempCellPos;
				MapManager.Inst.RemoveHitboxTile(LastCellPos.x, LastCellPos.y, HitboxWidth, HitboxHeight);
				MapManager.Inst.SetHitboxTile(CellPos.x, CellPos.y, HitboxWidth, HitboxHeight);
			}

			if (LastDir != Dir) LastDir = Dir;

			AdjustXYPosition(ref newX, ref newY, tempCellPos);

			transform.position = new Vector3(newX, newY);

			//Debug.Log($"{CellPos.x}, {CellPos.y}");
			CenterPos = new Vector2(m_spriteObject.transform.position.x, m_spriteObject.transform.position.y - 0.5f);

		}
	}

	void AdjustXYPosition(ref float _newX, ref float _newY, Vector2Int _tempCellPos)
	{
		if (Dir == eDir.UpRight)
		{
			if (MapManager.Inst.IsBlocked(_tempCellPos.x + 1, _tempCellPos.y, HitboxWidth, HitboxHeight))
			{
				int targetX = _tempCellPos.x;
				if (Mathf.Abs(_newX - targetX) < 0.1f)
					_newX = _tempCellPos.x;
			}
			if (MapManager.Inst.IsBlocked(_tempCellPos.x, _tempCellPos.y - 1, HitboxWidth, HitboxHeight))
			{
				int targetY = -_tempCellPos.y;
				if (Mathf.Abs(_newY - targetY) < 0.1f)
					_newY = targetY;
			}
		}
		else if (Dir == eDir.UpLeft)
		{
			if (MapManager.Inst.IsBlocked(_tempCellPos.x - 1, _tempCellPos.y, HitboxWidth, HitboxHeight))
			{
				int targetX = _tempCellPos.x;
				if (Mathf.Abs(_newX - targetX) < 0.1f)
					_newX = _tempCellPos.x;
			}
			if (MapManager.Inst.IsBlocked(_tempCellPos.x, _tempCellPos.y - 1, HitboxWidth, HitboxHeight))
			{
				int targetY = -_tempCellPos.y;
				if (Mathf.Abs(_newY - targetY) < 0.1f)
					_newY = targetY;
			}
		}
		else if (Dir == eDir.DownRight)
		{
			if (MapManager.Inst.IsBlocked(_tempCellPos.x + 1, _tempCellPos.y, HitboxWidth, HitboxHeight))
			{
				int targetX = _tempCellPos.x;
				if (Mathf.Abs(_newX - targetX) < 0.1f)
					_newX = _tempCellPos.x;
			}
			if (MapManager.Inst.IsBlocked(_tempCellPos.x, _tempCellPos.y + 1, HitboxWidth, HitboxHeight))
			{
				int targetY = -_tempCellPos.y;
				if (Mathf.Abs(_newY - targetY) < 0.1f)
					_newY = targetY;
			}
		}
		else if (Dir == eDir.DownLeft)
		{
			if (MapManager.Inst.IsBlocked(_tempCellPos.x - 1, _tempCellPos.y, HitboxWidth, HitboxHeight))
			{
				int targetX = _tempCellPos.x;
				if (Mathf.Abs(_newX - targetX) < 0.1f)
					_newX = _tempCellPos.x;
			}
			if (MapManager.Inst.IsBlocked(_tempCellPos.x, _tempCellPos.y + 1, HitboxWidth, HitboxHeight))
			{
				int targetY = -_tempCellPos.y;
				if (Mathf.Abs(_newY - targetY) < 0.1f)
					_newY = targetY;
			}
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

		if ((testY != 0.0f && MapManager.Inst.IsBlocked(vecX, vec.y + 1, HitboxWidth, HitboxHeight)) // 0일때는 0번째 줄 이동 가능
						|| MapManager.Inst.IsBlocked(vecX, vec.y, HitboxWidth, HitboxHeight))
		{
			_newX = newX;
		}

		if (MapManager.Inst.IsBlocked(vecX, vec.y, HitboxWidth, HitboxHeight) && !MapManager.Inst.IsBlocked(vecX, vec.y + 1, HitboxWidth, HitboxHeight)) // Plant타일이 약간 띄어져 있어서 어색한 이동 방지 목적의 코드
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
				if (!MapManager.Inst.IsBlocked(vecX, vec.y, HitboxWidth, HitboxHeight) && MapManager.Inst.IsBlocked(vecX, vec.y + 1, HitboxWidth, HitboxHeight))
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
				if (MapManager.Inst.IsBlocked(vecX, vec.y + 1, HitboxWidth, HitboxHeight) && !MapManager.Inst.IsBlocked(vecX, vec.y + 2, HitboxWidth, HitboxHeight))
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

		if ((testX != 0.0f && MapManager.Inst.IsBlocked(vec.x + 1, vecY, HitboxWidth, HitboxHeight))
						|| MapManager.Inst.IsBlocked(vec.x, vecY, HitboxWidth, HitboxHeight))
		{
			_newY = newY;
		}

		if (MapManager.Inst.IsBlocked(vec.x, vecY, HitboxWidth, HitboxHeight) && !MapManager.Inst.IsBlocked(vec.x + 1, vecY, HitboxWidth, HitboxHeight)) // Plant타일이 약간 띄어져 있어서 어색한 이동 방지 목적의 코드
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
				if (!MapManager.Inst.IsBlocked(vec.x, vecY, HitboxWidth, HitboxHeight) && MapManager.Inst.IsBlocked(vec.x + 1, vecY, HitboxWidth, HitboxHeight))
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
				if (MapManager.Inst.IsBlocked(vec.x + 1, vecY, HitboxWidth, HitboxHeight) && !MapManager.Inst.IsBlocked(vec.x + 2, vecY, HitboxWidth, HitboxHeight))
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

	public void SetDir(byte _byteDir) 
	{
		ByteDir = _byteDir;
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

	public virtual void Flip()
	{
		if (((Dir == eDir.Right || Dir == eDir.UpRight || Dir == eDir.DownRight) && m_bIsFacingLeft)
			|| ((Dir == eDir.Left || Dir == eDir.UpLeft || Dir == eDir.DownLeft)&& !m_bIsFacingLeft))
		{
			m_spriteRenderer.flipX = m_bIsFacingLeft;
			m_bIsFacingLeft = !m_bIsFacingLeft;
		}
	}

	public virtual void Die()
	{
		MapManager.Inst.RemoveHitboxTile(CellPos.x, CellPos.y, HitboxWidth, HitboxHeight);
		m_spriteObject.SetActive(false);
		//IsDead = true;
	}

	public void Knockback(float _duration)
	{
		if (m_knockbackCoroutine != null)
		{
			StopCoroutine(m_knockbackCoroutine);
			transform.position = m_knockbackOrigin;
			m_knockbackCoroutine = null;
		}
		else
		{
			m_knockbackOrigin = transform.position;
		}

		//Debug.Log($"시간 : {_duration}");
		m_knockbackCoroutine = StartCoroutine(KnockbackCoroutine(_duration));
	}

	IEnumerator KnockbackCoroutine(float _duration)
	{
		Vector3 objDestPos = transform.position + new Vector3(m_bIsFacingLeft ? 0.2f : -0.2f, 0, 0);
		float elapsed = 0f;

		while (elapsed <= _duration)
		{
			transform.position = Vector3.MoveTowards(transform.position, objDestPos, Time.deltaTime * 3);
			elapsed += Time.deltaTime;
			yield return null;
		}
		transform.position = m_knockbackOrigin;
		m_knockbackCoroutine = null;
	}
}
