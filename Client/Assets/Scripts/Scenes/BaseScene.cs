using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
	public Define.Scene SceneType { get; protected set; } = Define.Scene.None;

	void Start()
	{

	}

	protected virtual void Init()
	{
		//Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
		//if (obj == null)
		//	ResourceManager.Inst.Instantiate("UI/EventSystem").name = "@EventSystem";
		SceneManagerEx.Inst.CurScene = FindObjectOfType<BaseScene>();
	}

	public virtual void Clear()
	{
		UIManager.Inst.Clear();
		ActionQueue.Inst.Clear();
	}

	private void OnApplicationQuit()
	{
		Packet pkt = LoginPacketMaker.ExitGame();
		NetworkManager.Inst.Send(pkt);

		Debug.Log("OnApplicationQuit");
	}
}
