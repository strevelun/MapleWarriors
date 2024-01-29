using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameScene : BaseScene
{
	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1280, 720, false);
		SceneType = Define.Scene.InGame;

		UIScene uiScene = UIManager.Inst.SetSceneUI(Define.Scene.InGame);

		Packet pkt = InGamePacketMaker.ReqInitInfo();
		NetworkManager.Inst.Send(pkt);
		IsLoading = false;
	}

	public override void Clear()
	{
		base.Clear();
	}

	void Start()
	{
		Init();
	}

	void Update()
	{

	}
}
