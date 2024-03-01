using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameScene : BaseScene
{
	[SerializeField]
	GameObject m_clear, m_wasted;

	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.eScene.InGame;

		UIScene uiScene = UIManager.Inst.SetSceneUI(Define.eScene.InGame);

		Packet pkt = InGamePacketMaker.ReqInitInfo();
		NetworkManager.Inst.Send(pkt);
		IsLoading = false;

		InputManager.Inst.SetInputEnabled(false);
		StartFadeCoroutine();
	}

	public override void Clear()
	{
		base.Clear();
		//MapManager.Inst.Destroy();
		SetClearImageVisible(false);
		SetWastedImageVisible(false);
	}

	void Start()
	{
		Init();
	}

	void Update()
	{
		if (!GameManager.Inst.GameStart) return;

		if(MapManager.Inst.CurStage == MapManager.Inst.MaxStage)
		{
			if(GameManager.Inst.CheckMapClear())
			{
				if (UserData.Inst.IsRoomOwner)
				{
					Packet pkt = InGamePacketMaker.GameOver();
					NetworkManager.Inst.Send(pkt);
					GameManager.Inst.GameStart = false;
				}
			}
		}

		if(!m_wasted.activeSelf && !m_clear.activeSelf && GameManager.Inst.CheckGameOver())
		{
			StartCoroutine(GameOverCoroutine());
		}
		else if (!m_clear.activeSelf && !m_wasted.activeSelf && GameManager.Inst.CheckMapClear())
		{
			m_clear.SetActive(true);
		}
	}

	IEnumerator GameOverCoroutine()
	{
		m_wasted.SetActive(true);
		yield return new WaitForSeconds(3f);
		if (UserData.Inst.IsRoomOwner)
		{
			Packet pkt = InGamePacketMaker.GameOver();
			NetworkManager.Inst.Send(pkt);
		}
	}

	public void SetClearImageVisible(bool _visible)
	{
		m_clear.SetActive(_visible);
	}

	public void SetWastedImageVisible(bool _visible)
	{
		m_wasted.SetActive(_visible);
	}
}
