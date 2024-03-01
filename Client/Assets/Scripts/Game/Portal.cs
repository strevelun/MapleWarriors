using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
	Vector3[] m_positions = new Vector3[] {
	new Vector3(1, -1), 
    new Vector3(1, -3), 
    new Vector3(3, -1), 
    new Vector3(3, -3)  
};

	int m_cntPlayer = 0;
    Dictionary<string, GameObject> m_playerObj = new Dictionary<string, GameObject>();

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
                (SceneManagerEx.Inst.CurScene as InGameScene).SetClearImageVisible(false);
				int i = 0;
                foreach(GameObject obj in m_playerObj.Values)
                {
                    obj.transform.position = m_positions[i++];
                }
                MapManager.Inst.LoadNextStage();
            }
        }
    }

	private void OnTriggerEnter2D(Collider2D _collision)
	{
        if (!GameManager.Inst.CheckMapClear()) return;

        m_playerObj.Add(_collision.gameObject.name, _collision.gameObject);
		++m_cntPlayer;
	}

	private void OnTriggerExit2D(Collider2D _collision)
	{
		if (!GameManager.Inst.CheckMapClear()) return;
		if (m_cntPlayer <= 0) return;

		m_playerObj.Remove(_collision.gameObject.name);
		--m_cntPlayer;
	}
}
