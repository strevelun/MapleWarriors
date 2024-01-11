using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : BaseScene
{
	protected override void Init()
	{
		base.Init();

		Screen.SetResolution(1600, 1200, false);
		SceneType = Define.Scene.Lobby;

        UIManager.Inst.SetSceneUI(Define.Scene.Lobby);

        UIManager.Inst.AddUI(Define.UIChat.UILobbyChat);
        //UIManager.Inst.AddChat(Define.UIChat.UILobbyChat, "Hello Test1");
        //UIManager.Inst.AddChat(Define.UIChat.UILobbyChat, "Hello Test2");
       // UIManager.Inst.AddChat(Define.UIChat.UILobbyChat, "HelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTest");
        //UIManager.Inst.AddChat(Define.UIChat.UILobbyChat, "HelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTest");
       // UIManager.Inst.AddChat(Define.UIChat.UILobbyChat, "HelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTestHelloTest");

        //UIManager.Inst.ShowSceneUI<UI_Lobby>();


    }
	public override void Clear()
    {
    }

    void Start()
    {
        Init(); 
	}

    void Update()
    {
        
    }
}
