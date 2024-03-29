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
		uiScene.AddUI("SkillPanel"); // room에서도 똑같이

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
				StartCoroutine(GameAllClearCoroutine());
				
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
			GameManager.Inst.GameStart = false;
		}
	}

	IEnumerator GameAllClearCoroutine()
	{
		m_clear.SetActive(true);
		yield return new WaitForSeconds(3f);
		if (UserData.Inst.IsRoomOwner)
		{
			Packet pkt = InGamePacketMaker.GameOver();
			NetworkManager.Inst.Send(pkt);
			GameManager.Inst.GameStart = false;
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
