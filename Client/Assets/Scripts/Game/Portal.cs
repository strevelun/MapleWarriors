using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

	int m_cntPlayer = 0;

    void Start()
    {
        
    }


    void Update()
    {
        // 맵의 몬스터는 다 죽었는지 확인
        // 조건이 충족되면 모든 플레이어 비활성화 + 위치이동 + 맵 프리팹 로드
        if(UserData.Inst.IsRoomOwner && GameManager.Inst.CheckMapClear())
        {
            if(m_cntPlayer == GameManager.Inst.PlayerAliveCnt)
			{
                Packet pkt = InGamePacketMaker.NextStage();
                UDPCommunicator.Inst.SendAll(pkt);
				GameManager.Inst.OnChangeStage();
            }
        }
    }

	private void OnTriggerEnter2D(Collider2D _collision)
	{
        if (!GameManager.Inst.CheckMapClear()) return;

		++m_cntPlayer;
	}

	private void OnTriggerExit2D(Collider2D _collision)
	{
		if (!GameManager.Inst.CheckMapClear()) return;
		if (m_cntPlayer <= 0) return;

		--m_cntPlayer;
	}
}
