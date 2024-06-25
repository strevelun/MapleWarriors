using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using static Define;

public class CreatureController : MonoBehaviour
{
	public ICreatureState CurState { get; private set; } = null;

	public int Idx { get; set; }

	public Animator Anim { get; private set; }
	protected GameObject m_spriteObject;
	protected Vector2 CenterPos { get; private set; }
	protected SpriteRenderer m_spriteRenderer;
	public byte ByteDir { get; set; } = 0;
	public DirEnum Dir { get; set; } = DirEnum.None;
	public DirEnum LastDir { get; protected set; } = DirEnum.None;
	public float MaxSpeed { get; protected set; } = 1f;
	public float CurSpeed { get; protected set; } = 1f;

	protected Coroutine m_damamgeCoroutine = null;

	protected GameObject m_damageObj;
	protected CanvasGroup m_damageCanvasGroup;
	protected TextMeshProUGUI m_damageTMP;
	protected RectTransform m_damageTMP_RT;

	[SerializeField]
	protected Vector2 m_damageUIOffset = new Vector2(0.5f, 1.5f);

	private Vector3 m_knockbackOrigin;
	private Coroutine m_knockbackCoroutine = null;

	protected bool m_bIsFacingLeft = true;

	public Vector2Int CellPos { get; protected set; }
	public Vector2Int LastCellPos { get; protected set; }

	protected float m_convertCellXPosOffset = 0.02f;
	protected float m_convertCellYPosOffset = 0.02f;

	protected float m_startFontSize, m_endFontSize;

	public int HP { get; set; }
	public int MaxHP { get; set; }
	public int AttackDamage { get; protected set; }
	public int AttackRange { get; protected set; }

	public int HitboxWidth { get; protected set; }
	public int HitboxHeight { get; protected set; }

	public bool IsDead { get { return HP <= 0; } }

	protected virtual void FixedUpdate()
	{
		CurState?.FixedUpdate();
	}

	protected virtual void Update()
	{
		CurState?.Update();
	}
	
	public bool ChangeState(ICreatureState _newState)
	{
		if(_newState.CanEnter(this) == false)								return false; 
		if (CurState != null && CurState.GetType() == _newState.GetType())	return false;

		CurState?.Exit();
		CurState = _newState;
		CurState.Enter(this);
		return true;
	}

	public void UpdateMove(float _deltaTime)
	{
		if (ByteDir == (byte)DirEnum.UpRight)			Dir = DirEnum.UpRight;
		else if (ByteDir == (byte)DirEnum.UpLeft)		Dir = DirEnum.UpLeft;
		else if (ByteDir == (byte)DirEnum.DownLeft)	Dir = DirEnum.DownLeft;
		else if (ByteDir == (byte)DirEnum.DownRight)	Dir = DirEnum.DownRight;
		else if (ByteDir == (byte)DirEnum.Up)			Dir = DirEnum.Up;
		else if (ByteDir == (byte)DirEnum.Down)		Dir = DirEnum.Down;
		else if (ByteDir == (byte)DirEnum.Left)		Dir = DirEnum.Left;
		else if (ByteDir == (byte)DirEnum.Right)		Dir = DirEnum.Right;
		else										Dir = DirEnum.None;

		float newX = transform.position.x, newY = transform.position.y;
		switch (Dir)
		{
			case DirEnum.Up:
				newY = transform.position.y + (CurSpeed * _deltaTime);
				m_convertCellXPosOffset = 0;
				m_convertCellYPosOffset = 0;
				break;
			case DirEnum.Down:
				newY = transform.position.y - (CurSpeed * _deltaTime);
				m_convertCellXPosOffset = 0;
				m_convertCellYPosOffset = 0;
				break;
			case DirEnum.Left:
				newX = transform.position.x - (CurSpeed * _deltaTime);
				m_convertCellXPosOffset =0;
				m_convertCellYPosOffset = 0;
				break;
			case DirEnum.Right:
				newX = transform.position.x + (CurSpeed * _deltaTime);
				m_convertCellXPosOffset = 0;
				m_convertCellYPosOffset = 0;
				break;
			case DirEnum.UpLeft: //
				newX = transform.position.x - (CurSpeed * _deltaTime);
				newY = transform.position.y + (CurSpeed * _deltaTime);
				m_convertCellXPosOffset = 0;
				m_convertCellYPosOffset = -(CurSpeed * _deltaTime);
				break;
			case DirEnum.UpRight:
				newX = transform.position.x + (CurSpeed * _deltaTime);
				newY = transform.position.y + (CurSpeed * _deltaTime);
				m_convertCellXPosOffset = 0;
				m_convertCellYPosOffset = 0;
				break;
			case DirEnum.DownLeft: //
				newX = transform.position.x - (CurSpeed * _deltaTime);
				newY = transform.position.y - (CurSpeed * _deltaTime);
				m_convertCellXPosOffset = -(CurSpeed * _deltaTime);
				m_convertCellYPosOffset = 0;
				break;
			case DirEnum.DownRight:
				newX = transform.position.x + (CurSpeed * _deltaTime);
				newY = transform.position.y - (CurSpeed * _deltaTime);
				m_convertCellXPosOffset = 0;
				m_convertCellYPosOffset = -(CurSpeed * _deltaTime);
				break;
		}

		newY = (float)Math.Round(newY, 3);
		newX = (float)Math.Round(newX, 3);

		if (Dir != DirEnum.None)
		{
			if (CompareTag("Player"))
			{
				AdjustXPosition(ref newX, ref newY);
				AdjustYPosition(ref newX, ref newY);
				AdjustXYPosition(ref newX, ref newY);
			}

			Vector2Int tempCellPos = ConvertToCellPos(newX, newY); 
			if (tempCellPos != CellPos) 
			{
				LastCellPos = CellPos;
				CellPos = tempCellPos;
			}

			if (LastDir != Dir) LastDir = Dir;

			transform.position = new Vector3(newX, newY);
			CenterPos = new Vector2(m_spriteObject.transform.position.x, m_spriteObject.transform.position.y - 0.5f);
		}
	}

	private void AdjustXYPosition(ref float _newX, ref float _newY)
	{
		Vector2Int tempCellPos = ConvertToCellPos(_newX, _newY);

		if (Dir == DirEnum.UpRight)
		{
			bool rightBlocked = MapManager.Inst.IsBlocked(tempCellPos.x + 1, tempCellPos.y, HitboxWidth, HitboxHeight);
			bool upBlocked = MapManager.Inst.IsBlocked(tempCellPos.x, tempCellPos.y - 1, HitboxWidth, HitboxHeight);

			if (rightBlocked)
			{
				int targetX = tempCellPos.x;
				if (Mathf.Abs(_newX - targetX) < 0.1f)
					_newX = tempCellPos.x;
			}
			if (upBlocked)
			{
				int targetY = -tempCellPos.y;
				if (Mathf.Abs(_newY - targetY) < 0.1f)
					_newY = targetY;
			}
			if (!rightBlocked && !upBlocked)
			{
				if (MapManager.Inst.IsBlocked(tempCellPos.x + 1, tempCellPos.y - 1, HitboxWidth, HitboxHeight))
				{
					if (Mathf.RoundToInt(transform.position.x + 0.5f) == CellPos.x + 1)
					{
						int targetY = -tempCellPos.y;
						if (Mathf.Abs(_newY - targetY) < 0.1f)
							_newY = targetY;
					}
					else
					{
						int targetX = tempCellPos.x;
						if (Mathf.Abs(_newX - targetX) < 0.1f)
							_newX = tempCellPos.x;
					}
				}
				// UpRight인채로 모서리를 지날 때 미세한 겹침 방지
				else if(MapManager.Inst.IsBlocked(tempCellPos.x - 1, tempCellPos.y - 1, HitboxWidth, HitboxHeight)) 
				{
					if (Mathf.RoundToInt(transform.position.x) == CellPos.x) 
					{
						int targetY = -tempCellPos.y;
						if (Mathf.Abs(_newY - targetY) < 0.1f)
							_newY = targetY;
					}
				}
			}
		}
		else if (Dir == DirEnum.UpLeft)
		{
			bool leftBlocked = MapManager.Inst.IsBlocked(tempCellPos.x - 1, tempCellPos.y, HitboxWidth, HitboxHeight);
			bool upBlocked = MapManager.Inst.IsBlocked(tempCellPos.x, tempCellPos.y - 1, HitboxWidth, HitboxHeight);

			if (leftBlocked)
			{
				int targetX = tempCellPos.x;
				if (Mathf.Abs(_newX - targetX) < 0.1f)
					_newX = tempCellPos.x;
			}
			if (upBlocked)
			{
				int targetY = -tempCellPos.y;
				if (Mathf.Abs(_newY - targetY) < 0.1f)
					_newY = targetY;
			}
			if (!leftBlocked && !upBlocked)
			{
				if (MapManager.Inst.IsBlocked(tempCellPos.x - 1, tempCellPos.y - 1, HitboxWidth, HitboxHeight))
				{
					if (Mathf.FloorToInt(transform.position.x) == CellPos.x)  
					{
						int targetX = tempCellPos.x;
						if (Mathf.Abs(_newX - targetX) < 0.1f)
							_newX = tempCellPos.x;
					}
					else
					{
						int targetY = -tempCellPos.y;
						if (Mathf.Abs(_newY - targetY) < 0.1f)
							_newY = targetY;
					}
				}
				else if (MapManager.Inst.IsBlocked(tempCellPos.x + 1, tempCellPos.y - 1, HitboxWidth, HitboxHeight))
				{
					if (Mathf.RoundToInt(transform.position.x) == CellPos.x)
					{
						int targetY = -tempCellPos.y;
						if (Mathf.Abs(_newY - targetY) < 0.1f)
							_newY = targetY;
					}
				}
			}
		}
		else if (Dir == DirEnum.DownRight)
		{
			bool rightBlocked = MapManager.Inst.IsBlocked(tempCellPos.x + 1, tempCellPos.y, HitboxWidth, HitboxHeight);
			bool downBlocked = MapManager.Inst.IsBlocked(tempCellPos.x, tempCellPos.y + 1, HitboxWidth, HitboxHeight);

			if (rightBlocked)
			{
				int targetX = tempCellPos.x;
				if (Mathf.Abs(_newX - targetX) < 0.1f)
					_newX = tempCellPos.x;
			}
			if (downBlocked)
			{
				int targetY = -tempCellPos.y;
				if (Mathf.Abs(_newY - targetY) < 0.1f)
					_newY = targetY;
			}
			if (!rightBlocked && !downBlocked)
			{
				if (MapManager.Inst.IsBlocked(tempCellPos.x + 1, tempCellPos.y + 1, HitboxWidth, HitboxHeight))
				{
					if (Mathf.RoundToInt(transform.position.x + 0.5f) == CellPos.x + 1)  // cellxpos=24 : 24.5~25.5
					{
						int targetY = -tempCellPos.y;
						if (Mathf.Abs(_newY - targetY) < 0.1f)
							_newY = targetY;
					}
					else
					{
						int targetX = tempCellPos.x;
						if (Mathf.Abs(_newX - targetX) < 0.1f)
							_newX = tempCellPos.x;
					}
				}
				else if (MapManager.Inst.IsBlocked(tempCellPos.x - 1, tempCellPos.y + 1, HitboxWidth, HitboxHeight))
				{
					if (Mathf.RoundToInt(transform.position.x) == CellPos.x) // cellpos:14는 13.5~14.5
					{
						int targetY = -tempCellPos.y;
						if (Mathf.Abs(_newY - targetY) < 0.1f)
							_newY = targetY;
					}
				}
			}
		}
		else if (Dir == DirEnum.DownLeft)
		{
			bool lefttBlocked = MapManager.Inst.IsBlocked(tempCellPos.x - 1, tempCellPos.y, HitboxWidth, HitboxHeight);
			bool downBlocked = MapManager.Inst.IsBlocked(tempCellPos.x, tempCellPos.y + 1, HitboxWidth, HitboxHeight);

			if (lefttBlocked)
			{
				int targetX = tempCellPos.x;
				if (Mathf.Abs(_newX - targetX) < 0.1f)
					_newX = tempCellPos.x;
			}
			if (downBlocked)
			{
				int targetY = -tempCellPos.y;
				if (Mathf.Abs(_newY - targetY) < 0.1f)
					_newY = targetY;
			}
			if (!lefttBlocked && !downBlocked)
			{
				if (MapManager.Inst.IsBlocked(tempCellPos.x - 1, tempCellPos.y + 1, HitboxWidth, HitboxHeight))
				{
					if (Mathf.FloorToInt(transform.position.x) == CellPos.x)  // cellxpos=24 : 24.5~25.5
					{
						int targetX = tempCellPos.x;
						if (Mathf.Abs(_newX - targetX) < 0.1f)
							_newX = tempCellPos.x;
					}
					else
					{
						int targetY = -tempCellPos.y;
						if (Mathf.Abs(_newY - targetY) < 0.1f)
							_newY = targetY;
					}
				}
				else if (MapManager.Inst.IsBlocked(tempCellPos.x + 1, tempCellPos.y + 1, HitboxWidth, HitboxHeight))
				{
					if (Mathf.RoundToInt(transform.position.x) == CellPos.x) // cellpos:14는 13.5~14.5
					{
						int targetY = -tempCellPos.y;
						if (Mathf.Abs(_newY - targetY) < 0.1f)
							_newY = targetY;
					}
				}
			}
		}
	}

	private void AdjustXPosition(ref float _newX, ref float _newY)
	{
		if (Dir != DirEnum.Left && Dir != DirEnum.Right) return;

		Vector2Int vec = MapManager.Inst.WorldToCell(_newX, _newY);
		float absNewY = Math.Abs(_newY);
		float testY = absNewY - Mathf.Floor(absNewY);

		int vecX = 0, newX = 0;
		if (Dir == DirEnum.Right)
		{
			vecX = vec.x + 1;
			newX = vec.x;
		}
		else if(Dir == DirEnum.Left)
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
				_newY -= (CurSpeed * Time.deltaTime);
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
						_newY += (CurSpeed * Time.deltaTime);
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
						_newY -= (CurSpeed * Time.deltaTime);
				}
			}
		}
	}

	private void AdjustYPosition(ref float _newX, ref float _newY)
	{
		if (Dir != DirEnum.Up && Dir != DirEnum.Down) return;

		Vector2Int vec = MapManager.Inst.WorldToCell(_newX, _newY);
		float absNewX = Math.Abs(_newX);
		float testX = absNewX - Mathf.Floor(absNewX);

		int vecY = 0, newY = 0;
		if (Dir == DirEnum.Up)
		{
			vecY = vec.y;
			newY = -(vec.y + 1);
		}
		else if (Dir == DirEnum.Down)
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
				_newX += (CurSpeed * Time.deltaTime);
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
						_newX -= (CurSpeed * Time.deltaTime);
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
						_newX += (CurSpeed * Time.deltaTime);
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
	}

	public void SetByteDir(byte _byteDir) 
	{
		if (IsDead) return;

		ByteDir = _byteDir;
	}

	public DirEnum GetDir(byte _byteDir)
	{
		DirEnum dir;

		if (_byteDir == (byte)DirEnum.UpRight) dir = DirEnum.UpRight;
		else if (_byteDir == (byte)DirEnum.UpLeft) dir = DirEnum.UpLeft;
		else if (_byteDir == (byte)DirEnum.DownLeft) dir = DirEnum.DownLeft;
		else if (_byteDir == (byte)DirEnum.DownRight) dir = DirEnum.DownRight;
		else if (_byteDir == (byte)DirEnum.Up) dir = DirEnum.Up;
		else if (_byteDir == (byte)DirEnum.Down) dir = DirEnum.Down;
		else if (_byteDir == (byte)DirEnum.Left) dir = DirEnum.Left;
		else if (_byteDir == (byte)DirEnum.Right) dir = DirEnum.Right;
		else dir = DirEnum.None;

		return dir;
	}

	public virtual void SetPosition(int _cellXPos, int _cellYPos) 
	{
		if (_cellXPos < 0 || _cellYPos < 0 || _cellXPos >= MapManager.Inst.XSize || _cellYPos >= MapManager.Inst.YSize) return;

		CellPos = new Vector2Int(_cellXPos, _cellYPos);
		LastCellPos = new Vector2Int(_cellXPos, _cellYPos);
		transform.position = new Vector3(_cellXPos, -_cellYPos); 
	}

	// 아래에서 올라올때 y=0이 되는 기준이 -0.5임. 그래서 몬스터가 -0.5에서 더이상 안올라가고 만족함
	public Vector2Int ConvertToCellPos(float _xpos, float _ypos)
	{
		float fxpos = _xpos + m_convertCellXPosOffset + 0.5f;
		float fypos = _ypos + m_convertCellYPosOffset + 0.5f; 
		int xpos = (int)Math.Floor(fxpos);
		int ypos = (int)Math.Floor(fypos);
		return new Vector2Int(xpos, -ypos);
	}

	public virtual void Flip()
	{
		if (((Dir == DirEnum.Right || Dir == DirEnum.UpRight || Dir == DirEnum.DownRight) && m_bIsFacingLeft)
			|| ((Dir == DirEnum.Left || Dir == DirEnum.UpLeft || Dir == DirEnum.DownLeft) && !m_bIsFacingLeft))
		{
			m_spriteRenderer.flipX = m_bIsFacingLeft;
			m_bIsFacingLeft = !m_bIsFacingLeft;
		}
	}

	public virtual void Flip(DirEnum _dir)
	{
		if (((_dir == DirEnum.Right || _dir == DirEnum.UpRight || _dir == DirEnum.DownRight) && m_bIsFacingLeft)
			|| ((_dir == DirEnum.Left || _dir == DirEnum.UpLeft || _dir == DirEnum.DownLeft) && !m_bIsFacingLeft))
		{
			m_spriteRenderer.flipX = m_bIsFacingLeft;
			m_bIsFacingLeft = !m_bIsFacingLeft;
		}
	}

	public virtual void Die()
	{
		MapManager.Inst.RemoveHitboxTile(CellPos.x, CellPos.y, HitboxWidth, HitboxHeight);
		m_spriteObject.SetActive(false);
	}

	public void Knockback(float _duration)
	{
		if (m_knockbackCoroutine != null)
		{
			StopCoroutine(m_knockbackCoroutine);
			m_spriteObject.transform.localPosition = m_knockbackOrigin;
			m_knockbackCoroutine = null;
		}
		else
		{
			m_knockbackOrigin = m_spriteObject.transform.localPosition;
		}

		m_knockbackCoroutine = StartCoroutine(KnockbackCoroutine(_duration));
	}

	private IEnumerator KnockbackCoroutine(float _duration)
	{
		Vector3 objDestPos = m_spriteObject.transform.localPosition + new Vector3(m_bIsFacingLeft ? 0.2f : -0.2f, 0, 0);
		float elapsed = 0f;

		while (elapsed <= _duration)
		{
			m_spriteObject.transform.localPosition = Vector3.MoveTowards(m_spriteObject.transform.localPosition, objDestPos, Time.deltaTime * 3);
			elapsed += Time.deltaTime;
			yield return null;
		}
		m_spriteObject.transform.localPosition = m_knockbackOrigin;
		m_knockbackCoroutine = null;
	}

	protected IEnumerator DamageCoroutine(float _delay)
	{
		float elapsedTime = 0f;
		Vector3 startPos = m_damageTMP_RT.position;
		Vector3 destPos = startPos + new Vector3(0, 50, 0);
		float t;

		while (elapsedTime < _delay)
		{
			elapsedTime += Time.deltaTime;
			t = elapsedTime / _delay;

			m_damageTMP_RT.position = Vector3.Lerp(startPos, destPos, t);
			m_damageTMP.fontSize = Mathf.Lerp(m_startFontSize, m_endFontSize, t);
			m_damageCanvasGroup.alpha = 1.0f - t;

			yield return null;
		}

		m_damageTMP.fontSize = m_startFontSize;
		m_damageObj.SetActive(false);
	}
}
