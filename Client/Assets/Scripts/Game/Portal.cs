using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

	int m_cntPlayer;

    void Start()
    {
        m_cntPlayer = 0;
    }


    void Update()
    {
        // 맵의 몬스터는 다 죽었는지 확인
        // 조건이 충족되면 모든 플레이어 비활성화 + 위치이동 + 맵 프리팹 로드
        if(UserData.Inst.IsRoomOwner && GameManager.Inst.CheckMapClear())
        {
            if(m_cntPlayer == GameManager.Inst.PlayerAliveCnt)
			{
                Packet pkt = InGamePacketMaker.NextStage(); // 다음이 어느 스테이지인지 보내기. OnChangeStage 함수에서 현재 스테이지 갱신 후 두 번째로 패킷이 들어오면 리턴. 
				// 스테이지 바뀐 후 ready가 안된 유저가 몇 초 이상 지속되면 탈락

				UDPCommunicator.Inst.SendAll(pkt);
				GameManager.Inst.OnChangeStage();
            }
        }
    }

	private void OnTriggerEnter2D(Collider2D _collision)
	{
        if (!GameManager.Inst.CheckMapClear()) return;

		++m_cntPlayer;
        InGameConsole.Inst.Log($"현재 포탈에 {m_cntPlayer} 명");
	}

	private void OnTriggerExit2D(Collider2D _collision)
	{
		if (!GameManager.Inst.CheckMapClear()) return;
		if (m_cntPlayer <= 0) return;

		--m_cntPlayer;
		InGameConsole.Inst.Log($"현재 포탈에 {m_cntPlayer} 명");
	}
}
