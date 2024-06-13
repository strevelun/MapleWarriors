using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Types;

public class InGameScene : BaseScene
{
	[SerializeField]
	private GameObject m_allClear, m_clear, m_wasted;

	public override void Init()
	{
		base.Init();

		//Screen.SetResolution(1280, 720, false);
		SceneType = Define.SceneEnum.InGame;

		UIScene uiScene = UIManager.Inst.SetSceneUI(Define.SceneEnum.InGame);
		uiScene.AddUI("SkillPanel"); // room에서도 똑같이
		GameObject ingameConsole = uiScene.AddUI("Ingame_Console");
		GameObject connections = uiScene.AddUI("Connections");
		UIManager.Inst.AddUI(Define.UIEnum.Connections, connections);
		TextMeshProUGUI tmp = connections.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		tmp.text = string.Empty;

		GameObject player;
		PlayerController pc;

		GameManager.Inst.GetPlayerInfo(UserData.Inst.MyRoomSlot, out Define.StPlayerInfo info);
		player = ResourceManager.Inst.Instantiate($"Creature/Player_{info.characterChoice}");
		MyPlayerController mpc = player.AddComponent<MyPlayerController>();
		mpc.Idx = UserData.Inst.MyRoomSlot;
		mpc.SetNickname(info.nickname);
		tmp.text += $"{UserData.Inst.Nickname} [{info.ip}, {info.privateIP}, {info.port}]\n";
		ObjectManager.Inst.AddPlayer(UserData.Inst.MyRoomSlot, player);

		foreach (int idx in GameManager.Inst.OtherPlayersSlot)
		{
			GameManager.Inst.GetPlayerInfo(idx, out info);
			player = ResourceManager.Inst.Instantiate($"Creature/Player_{info.characterChoice}"); // 플레이어 선택

			pc = player.AddComponent<PlayerController>();
			pc.Idx = idx;
			pc.SetNickname(info.nickname);
			pc.name = $"Player_{info.characterChoice}_{idx}";
			tmp.text += $"{info.nickname} [{info.ip}, {info.privateIP}, {info.port}]\n";

			//GameManager.Inst.AddPlayer(idx, player);
			ObjectManager.Inst.AddPlayer(idx, player);
		}


		InGameConsole.Inst.Init(ingameConsole);

		IsLoading = false;

		InputManager.Inst.SetInputEnabled(false);
		StartFadeCoroutine();

		GameManager.Inst.Init(this);

		StartCoroutine(RoomOwnerLogic());
		StartCoroutine(GameStartLogic());

		GameObject camObj = GameObject.Find("CM vcam1");
		//CinemachineVirtualCamera vcam1 = camObj.GetComponent<CinemachineVirtualCamera>();
		GameObject mapObj = MapManager.Inst.Load(UserData.Inst.MapID, camObj); // TestMap : 1

		GameObject monsters = Util.FindChild(mapObj, false, "Monsters");
		int activeCnt = 0;
		for (int i = 0; i < monsters.transform.childCount; i++)
		{
			if (monsters.transform.GetChild(i).gameObject.activeSelf)
			{
				++activeCnt;
			}
		}

		GameManager.Inst.SetMonsterCnt(activeCnt);

		InGameConsole.Inst.Log($"플레이어 수 : {GameManager.Inst.PlayerCnt}");
		InGameConsole.Inst.Log($"몬스터 수 : {GameManager.Inst.MonsterCnt}");
		InGameConsole.Inst.Log($"스테이지 수 : {MapManager.Inst.MaxStage}");
	}

	public override void Clear()
	{
		base.Clear();
		SetAllClearImageVisible(false);
		SetClearImageVisible(false);
		SetWastedImageVisible(false);

		InGameConsole.Inst.GameOver();
		GameManager.Inst.Clear();
		UDPCommunicator.Inst.ClearIngameInfo();
	}

	private void Start()
	{
		Init();
	}

	private void Update()
	{
		GameManager.Inst.UpdateTimer(Time.deltaTime);

		CheckGameState();
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

	private IEnumerator GameStartLogic()
	{
		while (true)
		{
			if (GameManager.Inst.GameStart || GameManager.Inst.CheckGameOver() ||  GameManager.Inst.StageLoading || GameManager.Inst.AllClear)
			{
				yield return null;
				continue;
			}

			if (GameManager.Inst.IsTimerOn())
			{
				//Debug.Log(GameManager.Inst.CheckGameOver());
				GameManager.Inst.CheckStartTimer();
				yield return null;
				continue;
			}

			if (!UserData.Inst.IsRoomOwner)
			{
				Packet pkt = InGamePacketMaker.Ready();
				UDPCommunicator.Inst.Send(pkt, UserData.Inst.RoomOwnerSlot);
			}
			else
			{
				bool start = GameManager.Inst.StartGame();
				if (start)
				{
					Debug.Log("Start!");
					int timer = (int)(GameManager.Inst.Timer * 1000000);
					int startTime = timer + 2 * 1000000;
					Packet pkt = InGamePacketMaker.Start(timer, startTime);
					UDPCommunicator.Inst.SendAll(pkt);
					GameManager.Inst.StartTime = startTime / 1000000f;
					StartCoroutine(UpdateCreaturesInfo());
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator UpdateCreaturesInfo()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.2f);
			if (!GameManager.Inst.GameStart || !UserData.Inst.IsRoomOwner)
			{
				yield return null;
				continue;
			}

			Packet pkt = InGamePacketMaker.AllCreaturesInfo();
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

	private void CheckGameState()
	{
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

			if (!m_wasted.activeSelf && !m_clear.activeSelf && GameManager.Inst.GameStart && GameManager.Inst.CheckGameOver()) // 전멸
			{
				OnAnnihilated();
			}
		}
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
