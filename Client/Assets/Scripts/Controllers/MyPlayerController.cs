using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
	Dir m_ePrevDir = Dir.None;
	KeyCode m_curKeyCode = KeyCode.None;
	bool m_bIsKeyDown = false;
	bool m_bIsKeyUp = false;

    void Start()
    {
        
    }

    protected override void Update()
    {
		base.Update();

		InputKeyboard();
		HandleInput();
	}

	private void LateUpdate()
	{
		Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}


	public override void Init()
	{
		base.Init();


	}
	
	// 왼쪽방향키와 위쪽 방향키를 순서대로 누르고 유지하면 왼쪽으로 가다가 왼쪽방햐이를 떼면 위쪽으로 감
	void InputKeyboard()
	{
		if(Input.GetKeyUp(m_curKeyCode))
		{
			m_curKeyCode = KeyCode.None;
			m_bIsKeyUp = true;
			m_eDir = Dir.None;
		}

		if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.W))
		{
			m_curKeyCode = KeyCode.W;
			m_eDir = Dir.Up;
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.S))
		{
			m_curKeyCode = KeyCode.S;
			m_eDir = Dir.Down;
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.A))
		{
			m_curKeyCode = KeyCode.A;
			m_eDir = Dir.Left; 
			m_bIsKeyDown = true;
		}
		else if (m_curKeyCode == KeyCode.None && Input.GetKey(KeyCode.D))
		{
			m_curKeyCode = KeyCode.D;
			m_eDir = Dir.Right;
			m_bIsKeyDown = true;
		}
		/*
		else if (m_eDir != m_ePrevDir)
		{
			m_ePrevDir = m_eDir;
			m_curKeyCode = KeyCode.None;
			m_eDir = Dir.None;
		}
		*/
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
			Packet pkt = InGamePacketMaker.BeginMove(m_eDir);
			NetworkManager.Inst.Send(pkt);
			m_bIsKeyDown = false;
		}
	}
}
