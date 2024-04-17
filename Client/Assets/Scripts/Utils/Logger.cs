using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;

public class Logger 
{
	private List<string> m_logs = new List<string>();



	public void LogInfo()
	{
		if (!GameManager.Inst.GameStart) return;

		string log = $"[{System.DateTime.Now.ToString("HH:mm:ss.fff")}] ";
		PlayerController pc = ObjectManager.Inst.FindPlayer(UserData.Inst.MyRoomSlot);
		log += $"플레이어{UserData.Inst.MyRoomSlot} ----- x : {pc.transform.position.x},		 y : {pc.transform.position.y}		 /		";

		foreach (int slot in GameManager.Inst.OtherPlayersSlot)
		{
			pc = ObjectManager.Inst.FindPlayer(slot);
			log += $"플레이어{pc.Idx} ----- x : {pc.transform.position.x},		 y : {pc.transform.position.y}		 /		";
		}

		m_logs.Add(log);
	}

	public void End()
	{
		string filePath = Path.Combine(System.Environment.CurrentDirectory, "log.txt");
		File.WriteAllLines(filePath, m_logs);
	}
}
