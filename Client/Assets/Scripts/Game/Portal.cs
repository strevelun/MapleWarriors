﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	private int m_cntPlayer;

	private void Start()
    {
        m_cntPlayer = 0;
    }

	private void Update()
    {
        // 조건이 충족되면 모든 플레이어 비활성화 + 위치이동 + 맵 프리팹 로드
        if(UserData.Inst.IsRoomOwner && GameManager.Inst.CheckMapClear())
        {
            if(m_cntPlayer == GameManager.Inst.PlayerAliveCnt)
			{
				GameManager.Inst.PlayersOnPortal = true;
				GameManager.Inst.OnChangeStage();
            }
        }
    }

	private void OnTriggerEnter2D(Collider2D _collision)
	{
		++m_cntPlayer;
        //InGameConsole.Inst.Log($"현재 포탈에 {m_cntPlayer} 명");
	}

	private void OnTriggerExit2D(Collider2D _collision)
	{
		if (m_cntPlayer <= 0) return;

		--m_cntPlayer;
		//InGameConsole.Inst.Log($"현재 포탈에 {m_cntPlayer} 명");
	}
}
