using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameScene : BaseScene
{
	Logger m_logger = new Logger();

	[SerializeField]
	GameObject m_allClear, m_clear, m_wasted;

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
		SetAllClearImageVisible(false);
		SetClearImageVisible(false);
		SetWastedImageVisible(false);
	}

	void Start()
	{
		Init();

		InvokeRepeating("Log", 0f, 0.1f);
		InvokeRepeating("SendAwake", 0f, 30f);
	}

	void Update()
	{
		if (!GameManager.Inst.GameStart) return;

		if(MapManager.Inst.CurStage == MapManager.Inst.MaxStage)
		{
			if(GameManager.Inst.CheckMapClear())
			{
				StartCoroutine(GameAllClearCoroutine());
				GameManager.Inst.GameStart = false;
			}
		}
		else if (!m_clear.activeSelf && !m_wasted.activeSelf && GameManager.Inst.CheckMapClear())
		{
			SetClearImageVisible(true);
		}
		else if (!m_wasted.activeSelf && !m_clear.activeSelf && GameManager.Inst.CheckGameOver())
		{
			StartCoroutine(GameOverCoroutine());
			GameManager.Inst.GameStart = false;
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

	IEnumerator GameAllClearCoroutine()
	{
		m_allClear.SetActive(true);
		yield return new WaitForSeconds(3f);
		if (UserData.Inst.IsRoomOwner)
		{
			Packet pkt = InGamePacketMaker.GameOver();
			NetworkManager.Inst.Send(pkt);
		}
	}

	public void SetAllClearImageVisible(bool _visible)
	{
		m_allClear.SetActive(_visible);
	}

	public void SetClearImageVisible(bool _visible)
	{
		m_clear.SetActive(_visible);
		Debug.Log($"m_clear : {_visible}");
	}

	public void SetWastedImageVisible(bool _visible)
	{
		m_wasted.SetActive(_visible);
	}

	void Log()
	{
		m_logger.LogInfo();
	}

	void OnApplicationQuit()
	{
		m_logger.End();	
	}

	void SendAwake()
	{
		Packet pkt = InGamePacketMaker.AwakePacket();
		UDPCommunicator.Inst.SendAll(pkt);
		Debug.Log("어웨이크");
	}
}
