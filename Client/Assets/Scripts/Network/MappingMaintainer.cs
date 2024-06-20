using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MappingMaintainer : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(Send());
		DontDestroyOnLoad(this);
	}

	private IEnumerator Send()
	{
		byte[] pkt = new byte[1];
	
		while (true)
		{
			UDPCommunicator.Inst.Send(pkt, Define.ServerIP, Define.ServerPort);
			Debug.Log("MappingMaintainer::Send");
			yield return new WaitForSeconds(45f);
		}
	}
}
