using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseScene : MonoBehaviour
{
	public bool IsLoading { get; set; } = false;
	public Define.eScene SceneType { get; protected set; } = Define.eScene.None;

	void Start()
	{
		Application.runInBackground = true;
	}

	protected virtual void Init()
	{
		UIManager.Inst.ClearAll();
		SceneManagerEx.Inst.CurScene = FindObjectOfType<BaseScene>();
	}

	public virtual void Clear()
	{
		UIManager.Inst.Clear();
	}

	private void OnApplicationQuit()
	{
		Packet pkt = LoginPacketMaker.ExitGame();
		NetworkManager.Inst.Send(pkt);

		UIManager.Inst.ClearAll();

		Debug.Log("OnApplicationQuit");
	}
}
