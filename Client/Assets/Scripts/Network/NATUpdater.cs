using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NATUpdater : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);
		StartCoroutine(UpdateNAT());
    }

	private IEnumerator UpdateNAT()
	{
		while (true)
		{
			UDPCommunicator.Inst.SendAwake();
			yield return new WaitForSeconds(30f);
		}
	}
}
