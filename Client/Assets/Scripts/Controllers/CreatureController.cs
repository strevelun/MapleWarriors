using System;
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

		UpdateMove();

	}

	protected virtual void LateUpdate()
	{

	}


	void UpdateMove()
	{
		float newX = transform.position.x, newY = transform.position.y;

		switch (Dir)
		{
			case eDir.Up:
				newY = transform.position.y + (m_maxSpeed * Time.deltaTime);
				break;
			case eDir.Down:
				newY = transform.position.y - (m_maxSpeed * Time.deltaTime);
				break;
			case eDir.Left:
				newX = transform.position.x - (m_maxSpeed * Time.deltaTime);
				break;
			case eDir.Right:
				newX = transform.position.x + (m_maxSpeed * Time.deltaTime);
				break;
		}

		// 블록이 10,0에 있을때 캐릭터가 9.9999,0까지 가서 오른쪽을 누르면 10.0으로 맞춰짐. 근데 현재 위치가 블록의 위치라 안움직임.
		// 0~0.2인 경우 y=0존재, y=1비존재인 경우 멈춤. 0.2~0.8인 경우는 y=1만 확인. 

		if (Dir != eDir.None)
		{
			if(Dir == eDir.Left || Dir == eDir.Right)
				AdjustXPosition(ref newX, ref newY);
			/*
			Debug.Log($"{vec.x}, {vec.y}");
			if (Dir == eDir.Right)
			{
				vec.x = vec.x + 1;
			}
			else if (Dir == eDir.Down)
				vec.y = vec.y + 1;
			if (MapManager.Inst.IsBlocked(vec.x, vec.y))
			{
				if (Dir == eDir.Left)
					newX = (float)Math.Round((double)newX, MidpointRounding.AwayFromZero);
				else if (Dir == eDir.Right)
					newX = vec.x - 1;
				else if (Dir == eDir.Up)
					newY = (float)Math.Round((double)newY, MidpointRounding.AwayFromZero);
				else if (Dir == eDir.Down)
					newY = -(vec.y - 1);

				Dir = eDir.None;
			}
			*/
			transform.position = new Vector3(newX, newY);
			CellPos = ConvertToCellPos(transform.position.x, transform.position.y);
			//Debug.Log($"{CellPos.x}, {CellPos.y}");
		}

	}

	void AdjustXPosition(ref float _newX, ref float _newY)
	{
		Vector3Int vec = MapManager.Inst.WorldToCell(_newX, _newY);
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
			newX = vec.x+1;
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
				_newY -= (m_maxSpeed * Time.deltaTime);
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
						_newY += (m_maxSpeed * Time.deltaTime);
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
						_newY -= (m_maxSpeed * Time.deltaTime);
				}

			}
		}
	}

	void AdjustYPosition(ref float _newX, ref float _newY)
	{

	}

	void AdjustXPos(int _xpos, eDir _eDir)
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
