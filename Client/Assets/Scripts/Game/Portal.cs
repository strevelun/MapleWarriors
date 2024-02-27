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
        if(GameManager.Inst.CheckMapClear())
        {
            if(m_cntPlayer == GameManager.Inst.PlayerCnt)
            {
                MapManager.Inst.LoadNextStage();
            }
        }
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
        Debug.Log($"{collision.gameObject.name} Enter");

        ++m_cntPlayer;
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		Debug.Log($"{collision.gameObject.name} Exit");
        if (m_cntPlayer <= 0) return;

		--m_cntPlayer;
	}
}
