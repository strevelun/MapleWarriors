using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
	eDir m_ePrevDir = eDir.None;
	KeyCode m_curKeyCode = KeyCode.None;
	bool m_bIsKeyDown = false;
	bool m_bIsKeyUp = false;

	eDir m_eDirInput = eDir.None;

	long m_prevMoveTime = 0;

    void Start()
    {
        
    }

    protected override void Update()
    {
		base.Update();

		InputKeyboard();
		HandleInput();
	}

	private void FixedUpdate()
	{
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();

		//Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}
	
	// 왼쪽방향키와 위쪽 방향키를 순서대로 누르고 유지하면 왼쪽으로 가다가 왼쪽방향키를 떼면 위쪽으로 감
	void InputKeyboard()
	{
		if(Input.GetKeyUp(m_curKeyCode))
		{
			m_curKeyCode = KeyCode.None;
			m_bIsKeyUp = true;
			//m_eDirInput = eDir.None;
			Dir = eDir.None;
		}

		if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.W))
		{
			m_curKeyCode = KeyCode.W;
			//m_eDirInput = eDir.Up;
			Dir = eDir.Up;
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.S))
		{
			m_curKeyCode = KeyCode.S;
			//m_eDirInput = eDir.Down;
			Dir = eDir.Down;
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.A))
		{
			m_curKeyCode = KeyCode.A;
			//m_eDirInput = eDir.Left; 
			Dir = eDir.Left;
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.D))
		{
			m_curKeyCode = KeyCode.D;
			//m_eDirInput = eDir.Right;
			Dir = eDir.Right;
			m_bIsKeyDown = true;
		}
	}

	void HandleInput()
	{
		if (m_bIsKeyUp)
		{
			Packet pkt = InGamePacketMaker.EndMove(transform.position.x, transform.position.y);
			NetworkManager.Inst.Send(pkt);
			m_bIsKeyUp = false;
		}

		if (m_bIsKeyDown)
		{
			Packet pkt = InGamePacketMaker.BeginMove(Dir);
			NetworkManager.Inst.Send(pkt);
			m_bIsKeyDown = false;
			m_prevMoveTime = DateTime.Now.Ticks;
		}
	}
}
