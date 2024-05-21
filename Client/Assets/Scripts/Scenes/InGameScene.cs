using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameScene : BaseScene
{
	[SerializeField]
	private GameObject m_allClear, m_clear, m_wasted;

	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.SceneEnum.InGame;

		UIScene uiScene = UIManager.Inst.SetSceneUI(Define.SceneEnum.InGame);
		uiScene.AddUI("SkillPanel"); // room에서도 똑같이
		GameObject ingameConsole = uiScene.AddUI("Ingame_Console");
		GameObject connections = uiScene.AddUI("Connections");
		UIManager.Inst.AddUI(Define.UIEnum.Connections, connections);

		InGameConsole.Inst.Init(ingameConsole);

		IsLoading = false;

		InputManager.Inst.SetInputEnabled(false);
		StartFadeCoroutine();

		GameManager.Inst.Init(this);

		StartCoroutine(RoomOwnerLogic());

		Packet pkt = InGamePacketMaker.ReqInitInfo();
		NetworkManager.Inst.Send(pkt);
	}

	public override void Clear()
	{
		base.Clear();
		SetAllClearImageVisible(false);
		SetClearImageVisible(false);
		SetWastedImageVisible(false);

		InGameConsole.Inst.GameOver();
		GameManager.Inst.Clear();
		//UDPCommunicator.Inst.ClearIngameInfo();
		UDPCommunicator.Inst.Disconnect();
	}

	private void Start()
	{
		Init();
	}

	private void Update()
	{
		GameManager.Inst.UpdateTimer(Time.deltaTime);

		if (!GameManager.Inst.GameStart && !GameManager.Inst.StageLoading && !GameManager.Inst.AllClear && GameManager.Inst.PlayerAliveCnt != 0)
		{
			if (GameManager.Inst.IsTimerOn())
			{
				GameManager.Inst.CheckStartTimer();	
				return;
			}

			if(!UserData.Inst.IsRoomOwner)
			{

				Packet pkt = InGamePacketMaker.Ready();
				UDPCommunicator.Inst.Send(pkt, UserData.Inst.RoomOwnerSlot);
			}
			else
			{
				bool start = GameManager.Inst.StartGame();
				if(start)
				{
					int timer = (int)(GameManager.Inst.Timer * 1000000); 
					int startTime = timer + 2 * 1000000; 
					Packet pkt = InGamePacketMaker.Start(timer, startTime);
					UDPCommunicator.Inst.SendAll(pkt);
					GameManager.Inst.StartTime = startTime / 1000000f;
					StartCoroutine(UpdateMonstersInfo());
				}
			}
			return;
		}

		if (UserData.Inst.IsRoomOwner)
		{
			if (MapManager.Inst.CurStage == MapManager.Inst.MaxStage) // 맵 클리어
			{
				if (GameManager.Inst.CheckMapClear())
				{
					OnMapClear();
				}
			}
			else if (!m_clear.activeSelf && !m_wasted.activeSelf && GameManager.Inst.CheckMapClear()) // 스테이지 클리어
			{
				OnStageClear();
			}
			
			if (!m_wasted.activeSelf && !m_clear.activeSelf && GameManager.Inst.CheckGameOver()) // 전멸
			{
				OnAnnihilated();
			}
		}
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}

	private IEnumerator RoomOwnerLogic()
	{
		while(true)
		{
			if (!UserData.Inst.IsRoomOwner)
			{
				yield return null;
				continue;
			}

			if (GameManager.Inst.AllClear)
			{
				Packet pkt = InGamePacketMaker.MapClear();
				UDPCommunicator.Inst.SendAll(pkt);
			}
			else if (GameManager.Inst.StageClear)
			{
				Packet pkt = InGamePacketMaker.StageClear();
				UDPCommunicator.Inst.SendAll(pkt);
			}
			else if (GameManager.Inst.GameOver)
			{
				//InGameConsole.Inst.Log("Annihilated 보냄");
				Packet pkt = InGamePacketMaker.Annihilated();
				UDPCommunicator.Inst.SendAll(pkt);
			}

			if (GameManager.Inst.IsTimerOn())
			{
				int timer = (int)(GameManager.Inst.Timer * 1000000);
				Packet pkt = InGamePacketMaker.Start(timer, (int)(GameManager.Inst.StartTime * 1000000));
				UDPCommunicator.Inst.SendAll(pkt);
			}

			if(GameManager.Inst.PlayersOnPortal)
			{
				Packet pkt = InGamePacketMaker.NextStage();
				UDPCommunicator.Inst.SendAll(pkt);
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator UpdateMonstersInfo()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.2f);
			if (!GameManager.Inst.GameStart || !UserData.Inst.IsRoomOwner)
			{
				yield return null;
				continue;
			}

			Packet pkt = InGamePacketMaker.AllMonstersInfo();
			UDPCommunicator.Inst.SendAll(pkt);
		}
	}

	private IEnumerator GameOverCoroutine()
	{
		m_wasted.SetActive(true);
		yield return new WaitForSeconds(3f);
		if (UserData.Inst.IsRoomOwner)
		{
			Packet pkt = InGamePacketMaker.GameOver();
			NetworkManager.Inst.Send(pkt);
		}
	}

	private IEnumerator GameAllClearCoroutine()
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

	public void OnMapClear()
	{
		if (GameManager.Inst.AllClear) return;
		if (!GameManager.Inst.GameStart) return;

		StartCoroutine(GameAllClearCoroutine());
		GameManager.Inst.AllClear = true;
		InGameConsole.Inst.Log($"OnMapClear : {MapManager.Inst.CurStage} / {MapManager.Inst.MaxStage}, PlayerCnt : {GameManager.Inst.PlayerCnt}, PlayerAliveCnt : {GameManager.Inst.PlayerAliveCnt}, MonsterCnt : {GameManager.Inst.MonsterCnt}, allClear : {GameManager.Inst.AllClear}");
	}

	public void OnStageClear()
	{
		if (GameManager.Inst.StageLoading || GameManager.Inst.StageClear) return;

		SetClearImageVisible(true);
		GameManager.Inst.StageClear = true;

		InGameConsole.Inst.Log($"OnStageClear : {MapManager.Inst.CurStage} / {MapManager.Inst.MaxStage}, PlayerCnt : {GameManager.Inst.PlayerCnt}, PlayerAliveCnt : {GameManager.Inst.PlayerAliveCnt}, MonsterCnt : {GameManager.Inst.MonsterCnt}, allClear : {GameManager.Inst.AllClear}");
	}

	public void OnAnnihilated()
	{
		if (!GameManager.Inst.GameStart) return;
		if (GameManager.Inst.GameOver) return;
		
		StartCoroutine(GameOverCoroutine());
		GameManager.Inst.GameStart = false;
		GameManager.Inst.GameOver = true;

		InGameConsole.Inst.Log($"OnAnnihilated : {MapManager.Inst.CurStage} / {MapManager.Inst.MaxStage}, PlayerCnt : {GameManager.Inst.PlayerCnt}, PlayerAliveCnt : {GameManager.Inst.PlayerAliveCnt}, MonsterCnt : {GameManager.Inst.MonsterCnt}, allClear : {GameManager.Inst.AllClear}");
	}
}
