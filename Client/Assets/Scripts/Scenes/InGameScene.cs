using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameScene : BaseScene
{
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
		IsLoading = false;
		StartFadeCoroutine();
	}

	public override void Clear()
	{
		base.Clear();
		MapManager.Inst.Destroy();
	}

	void Start()
	{
		Init();
	}

	void Update()
	{
		GameManager.Inst.Update();
	}
}
