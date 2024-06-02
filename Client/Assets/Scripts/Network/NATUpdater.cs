using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NATUpdater : MonoBehaviour
{
    private void Start()
    {
		//StartCoroutine(UpdateNAT());
    }

	//private IEnumerator UpdateNAT()
	//{
		/*
		while (true)
		{
			Packet pkt = InGamePacketMaker.SendAwake();
			UDPCommunicator.Inst.SendAll(pkt);
			yield return new WaitForSeconds(5f);
		}
		*/
	//}
}
